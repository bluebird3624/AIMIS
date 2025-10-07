using Interchée.Contracts.Assignments;
using Interchée.Data;
using Interchée.Entities;
using Interchée.Extensions;
using Interchée.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Controllers
{
    [ApiController]
    [Route("submissions")]
    [Authorize]
    public class SubmissionsController(AppDbContext db, SimpleGitService gitService, SubmissionStatusService statusService) : ControllerBase
    {
        private readonly AppDbContext _db = db;
        private readonly SimpleGitService _gitService = gitService;
        private readonly SubmissionStatusService _statusService = statusService;

        /// <summary>Submit assignment (Intern/Attaché only)</summary>
        [HttpPost]
        [Authorize(Roles = "Intern,Attache")]
        [ProducesResponseType(typeof(SubmissionReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<SubmissionReadDto>> Submit([FromBody] SubmissionCreateDto dto)
        {
            var userId = User.GetUserId();

            // GIT VALIDATION
            if (!_gitService.ValidateRepoUrl(dto.RepoUrl))
                return BadRequest("Invalid Git repository URL. Must be a valid GitHub or GitLab repository.");

            // Verify user is assigned to the assignment
            var assignment = await _db.Assignments
                .Include(a => a.Assignees)
                .FirstOrDefaultAsync(a => a.Id == dto.AssignmentId);

            if (assignment == null) return NotFound("Assignment not found");

            var isAssigned = assignment.Assignees.Any(aa => aa.UserId == userId);
            if (!isAssigned) return Forbid("You are not assigned to this assignment");

            var submission = await _db.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.AssignmentId == dto.AssignmentId && s.UserId == userId);

            if (submission == null)
            {
                submission = new AssignmentSubmission
                {
                    AssignmentId = dto.AssignmentId,
                    UserId = userId,
                    RepoUrl = dto.RepoUrl,
                    Branch = dto.Branch ?? "main",
                    Status = "Submitted", // AUTOMATIC STATUS
                    SubmittedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _db.AssignmentSubmissions.Add(submission);
            }
            else
            {
                submission.RepoUrl = dto.RepoUrl;
                submission.Branch = dto.Branch ?? submission.Branch;
                submission.Status = "Submitted"; // AUTOMATIC STATUS ON RE-SUBMIT
                submission.SubmittedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            // AUTOMATICALLY CHECK IF ASSIGNMENT SHOULD BE CLOSED
            await _statusService.AutoCloseAssignmentIfAllSubmitted(dto.AssignmentId);

            var commitCount = await _db.SubmissionCommits
                .CountAsync(c => c.SubmissionId == submission.Id);
            var feedbackCount = await _db.FeedbackComments
                .CountAsync(f => f.SubmissionId == submission.Id);

            var readDto = new SubmissionReadDto(
                submission.Id, submission.AssignmentId, submission.UserId, submission.RepoUrl,
                submission.Branch, submission.LatestCommitSha, submission.SubmittedAt,
                submission.Status, submission.CreatedAt, null, commitCount, feedbackCount
            );

            return Ok(readDto);
        }

        /// <summary>Get user's submission for an assignment</summary>
        [HttpGet("assignment/{assignmentId:long}")]
        [ProducesResponseType(typeof(SubmissionReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<SubmissionReadDto>> GetUserSubmission(long assignmentId)
        {
            var userId = User.GetUserId();

            var submission = await _db.AssignmentSubmissions
                .Include(s => s.Grade)
                .Where(s => s.AssignmentId == assignmentId && s.UserId == userId)
                .Select(s => new SubmissionReadDto(
                    s.Id, s.AssignmentId, s.UserId, s.RepoUrl, s.Branch, s.LatestCommitSha,
                    s.SubmittedAt, s.Status, s.CreatedAt,
                    s.Grade != null ? new GradeReadDto(
                        s.Grade.Id, s.Grade.SubmissionId, s.Grade.Score, s.Grade.MaxScore,
                        s.Grade.RubricJson, s.Grade.GradedByUserId, s.Grade.GradedAt, ""
                    ) : null,
                    s.Commits.Count, s.FeedbackComments.Count
                ))
                .FirstOrDefaultAsync();

            return submission != null ? Ok(submission) : NotFound();
        }

        /// <summary>Add commit to submission (for webhooks or manual entry)</summary>
        [HttpPost("{submissionId:long}/commits")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddCommit(long submissionId, [FromBody] CommitCreateDto dto)
        {
            var submission = await _db.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null) return NotFound("Submission not found");

            // Check if commit already exists
            var commitExists = await _db.SubmissionCommits
                .AnyAsync(c => c.SubmissionId == submissionId && c.Sha == dto.Sha);

            if (commitExists)
                return Ok(new { message = "Commit already exists" });

            var commit = new SubmissionCommit
            {
                SubmissionId = submissionId,
                Sha = dto.Sha,
                Message = dto.Message?.Trim(),
                AuthorEmail = dto.AuthorEmail?.Trim(),
                CommittedAt = dto.CommittedAt
            };

            // Update latest commit
            submission.LatestCommitSha = dto.Sha;

            // AUTOMATIC STATUS UPDATE BASED ON COMMIT CONTENT
            await _statusService.UpdateSubmissionStatusFromCommit(submission, dto.Message);

            _db.SubmissionCommits.Add(commit);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Commit added successfully" });
        }

        /// <summary>Get submission commits</summary>
        [HttpGet("{submissionId:long}/commits")]
        [ProducesResponseType(typeof(IEnumerable<CommitReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CommitReadDto>>> GetCommits(long submissionId)
        {
            var commits = await _db.SubmissionCommits
                .Where(c => c.SubmissionId == submissionId)
                .OrderByDescending(c => c.CommittedAt)
                .Select(c => new CommitReadDto(
                    c.Id, c.SubmissionId, c.Sha, c.Message, c.AuthorEmail, c.CommittedAt
                ))
                .ToListAsync();

            return Ok(commits);
        }

        /// <summary>Get all submissions for an assignment (Supervisors only)</summary>
        [HttpGet("assignment/{assignmentId:long}/all")]
        [Authorize(Roles = "Admin,HR,Supervisor")]
        [ProducesResponseType(typeof(IEnumerable<SubmissionReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SubmissionReadDto>>> GetAssignmentSubmissions(long assignmentId)
        {
            var submissions = await _db.AssignmentSubmissions
                .Where(s => s.AssignmentId == assignmentId)
                .Include(s => s.User)
                .Include(s => s.Grade)
                .Select(s => new SubmissionReadDto(
                    s.Id, s.AssignmentId, s.UserId, s.RepoUrl, s.Branch, s.LatestCommitSha,
                    s.SubmittedAt, s.Status, s.CreatedAt,
                    s.Grade != null ? new GradeReadDto(
                        s.Grade.Id, s.Grade.SubmissionId, s.Grade.Score, s.Grade.MaxScore,
                        s.Grade.RubricJson, s.Grade.GradedByUserId, s.Grade.GradedAt, ""
                    ) : null,
                    s.Commits.Count, s.FeedbackComments.Count
                ))
                .ToListAsync();

            return Ok(submissions);
        }

        /// <summary>Update submission (change repo URL or branch)</summary>
        [HttpPut("{id:long}")]
        [Authorize(Roles = "Intern,Attache")]
        [ProducesResponseType(typeof(SubmissionReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SubmissionReadDto>> Update(long id, [FromBody] SubmissionUpdateDto dto)
        {
            var userId = User.GetUserId();
            var submission = await _db.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (submission == null) return NotFound();

            // Validate new repo URL if provided
            if (!string.IsNullOrEmpty(dto.RepoUrl) && !_gitService.ValidateRepoUrl(dto.RepoUrl))
                return BadRequest("Invalid Git repository URL");

            if (!string.IsNullOrEmpty(dto.RepoUrl))
                submission.RepoUrl = dto.RepoUrl;

            if (!string.IsNullOrEmpty(dto.Branch))
                submission.Branch = dto.Branch;

            await _db.SaveChangesAsync();

            var commitCount = await _db.SubmissionCommits
                .CountAsync(c => c.SubmissionId == submission.Id);
            var feedbackCount = await _db.FeedbackComments
                .CountAsync(f => f.SubmissionId == submission.Id);

            var readDto = new SubmissionReadDto(
                submission.Id, submission.AssignmentId, submission.UserId, submission.RepoUrl,
                submission.Branch, submission.LatestCommitSha, submission.SubmittedAt,
                submission.Status, submission.CreatedAt, null, commitCount, feedbackCount
            );

            return Ok(readDto);
        }

        /// <summary>Update submission status (Supervisors only - for grading)</summary>
        [HttpPut("{id:long}/status")]
        [Authorize(Roles = "Admin,HR,Supervisor")]
        [ProducesResponseType(typeof(SubmissionReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SubmissionReadDto>> UpdateStatus(long id, [FromBody] SubmissionStatusDto dto)
        {
            var submission = await _db.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null) return NotFound();

            submission.Status = dto.Status;
            await _db.SaveChangesAsync();

            // AUTOMATICALLY CHECK IF ASSIGNMENT SHOULD BE CLOSED
            await _statusService.AutoCloseAssignmentIfAllReviewed(submission.AssignmentId);

            var commitCount = await _db.SubmissionCommits
                .CountAsync(c => c.SubmissionId == submission.Id);
            var feedbackCount = await _db.FeedbackComments
                .CountAsync(f => f.SubmissionId == submission.Id);

            var readDto = new SubmissionReadDto(
                submission.Id, submission.AssignmentId, submission.UserId, submission.RepoUrl,
                submission.Branch, submission.LatestCommitSha, submission.SubmittedAt,
                submission.Status, submission.CreatedAt, null, commitCount, feedbackCount
            );

            return Ok(readDto);
        }

        /// <summary>Delete submission</summary>
        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Intern,Attache,Admin,HR,Supervisor")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(long id)
        {
            var userId = User.GetUserId();
            var submission = await _db.AssignmentSubmissions
                .Include(s => s.Commits)
                .Include(s => s.FeedbackComments)
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null) return NotFound();

            // Users can only delete their own submissions unless they're supervisors
            var isOwner = submission.UserId == userId;
            var isSupervisor = User.IsInRole("Admin") || User.IsInRole("HR") || User.IsInRole("Supervisor");

            if (!isOwner && !isSupervisor)
                return Forbid();

            // Remove related data first
            _db.SubmissionCommits.RemoveRange(submission.Commits);
            _db.FeedbackComments.RemoveRange(submission.FeedbackComments);
            if (submission.Grade != null)
                _db.Grades.Remove(submission.Grade);

            _db.AssignmentSubmissions.Remove(submission);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}