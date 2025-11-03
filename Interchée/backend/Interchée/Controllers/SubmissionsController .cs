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
    public class SubmissionsController(AppDbContext db, SimpleGitService gitService, SubmissionStatusService statusService, FileService fileService) : ControllerBase
    {
        private readonly AppDbContext _db = db;
        private readonly SimpleGitService _gitService = gitService;
        private readonly SubmissionStatusService _statusService = statusService;
        private readonly FileService _fileService = fileService;

        /// <summary>Submit assignment (Intern/Attaché only)</summary>
        [HttpPost]
        [Authorize(Roles = "Intern,Attache")]
        [ProducesResponseType(typeof(SubmissionReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<SubmissionReadDto>> Submit([FromBody] SubmissionCreateDto dto)
        {
            var userId = User.GetUserId();

            // CHECK IF ASSIGNMENT IS CLOSED - PREVENT SUBMISSION
            if (!await _statusService.CanSubmitToAssignment(dto.AssignmentId))
            {
                return BadRequest("Cannot submit to a closed assignment.");
            }

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

            // CHECK IF ASSIGNMENT IS PAST DUE
            if (assignment.DueAt.HasValue && assignment.DueAt.Value < DateTime.UtcNow)
            {
                return BadRequest("Cannot submit after assignment due date.");
            }

            var submission = await _db.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.AssignmentId == dto.AssignmentId && s.UserId == userId);

            if (submission == null)
            {
                submission = new AssignmentSubmission
                {
                    AssignmentId = dto.AssignmentId,
                    UserId = userId,
                    SubmissionType = "GitHub",
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
                submission.SubmissionType = "GitHub";
                submission.Status = "Submitted"; //  Keep as Submitted on re-submit
                submission.SubmittedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            var commitCount = await _db.SubmissionCommits
                .CountAsync(c => c.SubmissionId == submission.Id);
            var feedbackCount = await _db.FeedbackComments
                .CountAsync(f => f.SubmissionId == submission.Id);

            var readDto = new SubmissionReadDto(
                submission.Id, submission.AssignmentId, submission.UserId, "GitHub", submission.RepoUrl,
                submission.Branch, submission.LatestCommitSha, submission.SubmittedAt,
                submission.Status, submission.CreatedAt, null, commitCount, feedbackCount, new List<AttachmentReadDto>()
            );

            return Ok(readDto);
        }

        /// <summary>Submit assignment via file upload (Intern/Attaché only)</summary>
        /// <summary>Submit assignment via file upload (Intern/Attaché only)</summary>
        [HttpPost("file")]
        [Authorize(Roles = "Intern,Attache")]
        [RequestSizeLimit(10_485_760)] // 10MB limit
        [ProducesResponseType(typeof(SubmissionReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<SubmissionReadDto>> SubmitFile([FromForm] FileSubmissionCreateDto dto)
        {
            var userId = User.GetUserId();

            // CHECK IF ASSIGNMENT IS CLOSED - PREVENT SUBMISSION
            if (!await _statusService.CanSubmitToAssignment(dto.AssignmentId))
            {
                return BadRequest("Cannot submit to a closed assignment.");
            }

            // Verify user is assigned to the assignment
            var assignment = await _db.Assignments
                .Include(a => a.Assignees)
                .FirstOrDefaultAsync(a => a.Id == dto.AssignmentId);

            if (assignment == null) return NotFound("Assignment not found");

            var isAssigned = assignment.Assignees.Any(aa => aa.UserId == userId);
            if (!isAssigned) return Forbid("You are not assigned to this assignment");

            // CHECK IF ASSIGNMENT IS PAST DUE
            if (assignment.DueAt.HasValue && assignment.DueAt.Value < DateTime.UtcNow)
            {
                return BadRequest("Cannot submit after assignment due date.");
            }

            // Check if file exists
            if (dto.File == null || dto.File.Length == 0)
            {
                return BadRequest("File is required for file submission.");
            }

            var submission = await _db.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.AssignmentId == dto.AssignmentId && s.UserId == userId);

            if (submission == null)
            {
                // Create a new submission record for file upload
                submission = new AssignmentSubmission
                {
                    AssignmentId = dto.AssignmentId,
                    UserId = userId,
                    SubmissionType = "File",
                    RepoUrl = null, // No GitHub URL for file submissions
                    Branch = null,
                    Status = "Submitted",
                    SubmittedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _db.AssignmentSubmissions.Add(submission);
                await _db.SaveChangesAsync(); // Save to get the submission ID
            }
            else
            {
                // Update existing submission
                submission.SubmissionType = "File";
                submission.Status = "Submitted";
                submission.SubmittedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            // Now upload the file and link it to this submission
            try
            {
                var attachment = await _fileService.SaveFileAsync(dto.File, "Submission", submission.Id, userId);
                _db.Attachments.Add(attachment);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // If file upload fails, delete the submission record
                if (submission.Id > 0)
                {
                    _db.AssignmentSubmissions.Remove(submission);
                    await _db.SaveChangesAsync();
                }
                return BadRequest($"File upload failed: {ex.Message}");
            }

            // Get the uploaded file info for response
            var attachments = await _db.Attachments
                .Where(a => a.EntityType == "Submission" && a.EntityId == submission.Id)
                .Include(a => a.UploadedByUser)
                .Select(a => new AttachmentReadDto(
                    a.Id,
                    a.FileName,
                    a.ContentType,
                    a.FileSize,
                    a.UploadedAt,
                    a.UploadedByUserId,
                    $"{a.UploadedByUser!.FirstName} {a.UploadedByUser.LastName}",
                    a.EntityType,
                    a.EntityId
                ))
                .ToListAsync();

            var commitCount = await _db.SubmissionCommits
                .CountAsync(c => c.SubmissionId == submission.Id);
            var feedbackCount = await _db.FeedbackComments
                .CountAsync(f => f.SubmissionId == submission.Id);

            var readDto = new SubmissionReadDto(
                submission.Id,
                submission.AssignmentId,
                submission.UserId,
                "File", // SubmissionType = File
                null, // No RepoUrl
                null, // No Branch
                null, // No LatestCommitSha
                submission.SubmittedAt,
                submission.Status,
                submission.CreatedAt,
                null, // No Grade
                commitCount,
                feedbackCount,
                attachments
            );

            return Ok(readDto);
        }

        /// <summary>Get user's submission for an assignment</summary>
        [HttpGet("assignment/{assignmentId:long}")]
        [Authorize(Roles = "Intern,Attache")]
        [ProducesResponseType(typeof(SubmissionReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<SubmissionReadDto>> GetUserSubmission(long assignmentId)
        {
            var userId = User.GetUserId();

            // Get submission (could be GitHub or File type)
            var submission = await _db.AssignmentSubmissions
                .Include(s => s.Grade)
                .ThenInclude(g => g!.GradedByUser)
                .Where(s => s.AssignmentId == assignmentId && s.UserId == userId)
                .FirstOrDefaultAsync();

            if (submission == null) return NotFound("No submission found for this assignment");

            // Determine submission type
            var submissionType = string.IsNullOrEmpty(submission.RepoUrl) ? "File" : "GitHub";

            // Get attachments for file submissions
            var attachments = new List<AttachmentReadDto>();
            if (submissionType == "File")
            {
                attachments = await _db.Attachments
                    .Where(a => a.EntityType == "Submission" && a.EntityId == submission.Id)
                    .Include(a => a.UploadedByUser)
                    .Select(a => new AttachmentReadDto(
                        a.Id,
                        a.FileName,
                        a.ContentType,
                        a.FileSize,
                        a.UploadedAt,
                        a.UploadedByUserId,
                        $"{a.UploadedByUser!.FirstName} {a.UploadedByUser.LastName}",
                        a.EntityType,
                        a.EntityId
                    ))
                    .ToListAsync();
            }

            var commitCount = await _db.SubmissionCommits
                .CountAsync(c => c.SubmissionId == submission.Id);
            var feedbackCount = await _db.FeedbackComments
                .CountAsync(f => f.SubmissionId == submission.Id);

            var readDto = new SubmissionReadDto(
                submission.Id,
                submission.AssignmentId,
                submission.UserId,
                submissionType,
                submission.RepoUrl,
                submission.Branch,
                submission.LatestCommitSha,
                submission.SubmittedAt,
                submission.Status,
                submission.CreatedAt,
                submission.Grade != null ? new GradeReadDto(
                    submission.Grade.Id,
                    submission.Grade.SubmissionId,
                    submission.Grade.Score,
                    submission.Grade.MaxScore,
                    submission.Grade.RubricId,
                    null,
                    submission.Grade.RubricScoresJson,
                    submission.Grade.GradedByUserId,
                    submission.Grade.GradedAt,
                    $"{submission.Grade.GradedByUser!.FirstName} {submission.Grade.GradedByUser.LastName}"
                ) : null,
                commitCount,
                feedbackCount,
                attachments
            );

            return Ok(readDto);
        }

        /// <summary>Get all submissions for current user (Intern/Attaché only)</summary>
        [HttpGet("my-submissions")]
        [Authorize(Roles = "Intern,Attache")]
        [ProducesResponseType(typeof(IEnumerable<SubmissionReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SubmissionReadDto>>> GetMySubmissions()
        {
            var userId = User.GetUserId();

            // Get all submissions (both GitHub and File)
            var submissions = await _db.AssignmentSubmissions
                .Where(s => s.UserId == userId)
                .Include(s => s.Assignment)
                .Include(s => s.Grade)
                .ThenInclude(g => g!.GradedByUser)
                .ToListAsync();

            var result = new List<SubmissionReadDto>();

            foreach (var submission in submissions)
            {
                // Determine submission type
                var submissionType = string.IsNullOrEmpty(submission.RepoUrl) ? "File" : "GitHub";

                // Get attachments for file submissions
                var attachments = new List<AttachmentReadDto>();
                if (submissionType == "File")
                {
                    attachments = await _db.Attachments
                        .Where(a => a.EntityType == "Submission" && a.EntityId == submission.Id)
                        .Include(a => a.UploadedByUser)
                        .Select(a => new AttachmentReadDto(
                            a.Id,
                            a.FileName,
                            a.ContentType,
                            a.FileSize,
                            a.UploadedAt,
                            a.UploadedByUserId,
                            $"{a.UploadedByUser!.FirstName} {a.UploadedByUser.LastName}",
                            a.EntityType,
                            a.EntityId
                        ))
                        .ToListAsync();
                }

                var commitCount = await _db.SubmissionCommits
                    .CountAsync(c => c.SubmissionId == submission.Id);
                var feedbackCount = await _db.FeedbackComments
                    .CountAsync(f => f.SubmissionId == submission.Id);

                var readDto = new SubmissionReadDto(
                    submission.Id,
                    submission.AssignmentId,
                    submission.UserId,
                    submissionType,
                    submission.RepoUrl,
                    submission.Branch,
                    submission.LatestCommitSha,
                    submission.SubmittedAt,
                    submission.Status,
                    submission.CreatedAt,
                    submission.Grade != null ? new GradeReadDto(
                        submission.Grade.Id,
                        submission.Grade.SubmissionId,
                        submission.Grade.Score,
                        submission.Grade.MaxScore,
                        submission.Grade.RubricId,
                        null,
                        submission.Grade.RubricScoresJson,
                        submission.Grade.GradedByUserId,
                        submission.Grade.GradedAt,
                        $"{submission.Grade.GradedByUser!.FirstName} {submission.Grade.GradedByUser.LastName}"
                    ) : null,
                    commitCount,
                    feedbackCount,
                    attachments
                );

                result.Add(readDto);
            }

            return Ok(result);
        }
        /// <summary>Add commit to submission (for webhooks or manual entry)</summary>
        [HttpPost("{submissionId:long}/commits")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddCommit(long submissionId, [FromBody] CommitCreateDto dto)
        {
            var submission = await _db.AssignmentSubmissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null) return NotFound("Submission not found");

            // Check if assignment is closed
            if (submission.Assignment?.Status == "Closed" || submission.Assignment?.Status == "Archived")
            {
                return BadRequest("Cannot add commits to a submission in a closed assignment.");
            }

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
            // Get all submissions for this assignment
            var submissions = await _db.AssignmentSubmissions
                .Where(s => s.AssignmentId == assignmentId)
                .Include(s => s.User)
                .Include(s => s.Grade)
                .ThenInclude(g => g!.GradedByUser)
                .ToListAsync();

            var result = new List<SubmissionReadDto>();

            foreach (var submission in submissions)
            {
                // Determine submission type
                var submissionType = string.IsNullOrEmpty(submission.RepoUrl) ? "File" : "GitHub";

                // Get attachments for file submissions
                var attachments = new List<AttachmentReadDto>();
                if (submissionType == "File")
                {
                    attachments = await _db.Attachments
                        .Where(a => a.EntityType == "Submission" && a.EntityId == submission.Id)
                        .Include(a => a.UploadedByUser)
                        .Select(a => new AttachmentReadDto(
                            a.Id,
                            a.FileName,
                            a.ContentType,
                            a.FileSize,
                            a.UploadedAt,
                            a.UploadedByUserId,
                            $"{a.UploadedByUser!.FirstName} {a.UploadedByUser.LastName}",
                            a.EntityType,
                            a.EntityId
                        ))
                        .ToListAsync();
                }

                var commitCount = await _db.SubmissionCommits
                    .CountAsync(c => c.SubmissionId == submission.Id);
                var feedbackCount = await _db.FeedbackComments
                    .CountAsync(f => f.SubmissionId == submission.Id);

                var readDto = new SubmissionReadDto(
                    submission.Id,
                    submission.AssignmentId,
                    submission.UserId,
                    submissionType,
                    submission.RepoUrl,
                    submission.Branch,
                    submission.LatestCommitSha,
                    submission.SubmittedAt,
                    submission.Status,
                    submission.CreatedAt,
                    submission.Grade != null ? new GradeReadDto(
                        submission.Grade.Id,
                        submission.Grade.SubmissionId,
                        submission.Grade.Score,
                        submission.Grade.MaxScore,
                        submission.Grade.RubricId,
                        null,
                        submission.Grade.RubricScoresJson,
                        submission.Grade.GradedByUserId,
                        submission.Grade.GradedAt,
                        $"{submission.Grade.GradedByUser!.FirstName} {submission.Grade.GradedByUser.LastName}"
                    ) : null,
                    commitCount,
                    feedbackCount,
                    attachments
                );

                result.Add(readDto);
            }

            return Ok(result);
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
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (submission == null) return NotFound();

            // Check if assignment is closed
            if (submission.Assignment?.Status == "Closed" || submission.Assignment?.Status == "Archived")
            {
                return BadRequest("Cannot update a submission in a closed assignment.");
            }

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
                 submission.Id, submission.AssignmentId, submission.UserId, "GitHub", submission.RepoUrl,
                submission.Branch, submission.LatestCommitSha, submission.SubmittedAt,
                submission.Status, submission.CreatedAt, null, commitCount, feedbackCount, new List<AttachmentReadDto>()
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

            // If changing to "Reviewed", use the service method for proper validation
            if (dto.Status == "Reviewed")
            {
                var success = await _statusService.MarkAsReviewed(id);
                if (!success)
                {
                    return BadRequest("Cannot mark submission as reviewed. Either assignment is closed or submission is not in Submitted status.");
                }
            }
            else
            {
                // For other status changes, check if assignment is closed

                if (submission.Assignment?.Status == "Closed" || submission.Assignment?.Status == "Archived")
                {
                    return BadRequest("Cannot update status of submission in closed assignment.");
                }

                submission.Status = dto.Status;
                await _db.SaveChangesAsync();
            }

            var commitCount = await _db.SubmissionCommits
                .CountAsync(c => c.SubmissionId == submission.Id);
            var feedbackCount = await _db.FeedbackComments
                .CountAsync(f => f.SubmissionId == submission.Id);

            var readDto = new SubmissionReadDto(
                  submission.Id, submission.AssignmentId, submission.UserId, "GitHub", submission.RepoUrl,
                submission.Branch, submission.LatestCommitSha, submission.SubmittedAt,
                submission.Status, submission.CreatedAt, null, commitCount, feedbackCount, new List<AttachmentReadDto>()
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