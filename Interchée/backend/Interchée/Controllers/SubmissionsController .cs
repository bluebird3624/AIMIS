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
    public class SubmissionsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly SimpleGitService _gitService;

        public SubmissionsController(AppDbContext db, SimpleGitService gitService)
        {
            _db = db;
            _gitService = gitService;
        }

        /// <summary>Submit assignment (Intern/Attaché only)</summary>
        [HttpPost]
        [Authorize(Roles = "Intern,Attache")]
        [ProducesResponseType(typeof(SubmissionReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<SubmissionReadDto>> Submit([FromBody] SubmissionCreateDto dto)
        {
            var userId = User.GetUserId();

            // ADD GIT VALIDATION
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
                    Branch = dto.Branch ?? "main", // Default to main branch
                    Status = "Submitted",
                    SubmittedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow // ADD THIS MISSING PROPERTY
                };
                _db.AssignmentSubmissions.Add(submission);
            }
            else
            {
                submission.RepoUrl = dto.RepoUrl;
                submission.Branch = dto.Branch ?? submission.Branch;
                submission.Status = "Submitted";
                submission.SubmittedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            // GET ACTUAL COUNTS INSTEAD OF HARDCODED 0,0
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

        // ADD THESE NEW ENDPOINTS FOR GIT INTEGRATION:

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
    }
}