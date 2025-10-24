using Interchée.Contracts.Assignments;
using Interchée.Data;
using Interchée.Entities;
using Interchée.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Controllers
{
    [ApiController]
    [Route("grading")]
    [Authorize]
    public class GradingController(AppDbContext db) : ControllerBase
    {
        private readonly AppDbContext _db = db;

        /// <summary>Grade a submission (Supervisors only)</summary>
        [HttpPost("submissions/{submissionId:long}/grade")]
        [Authorize(Roles = "Admin,HR,Supervisor")]
        [ProducesResponseType(typeof(GradeReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<GradeReadDto>> GradeSubmission(long submissionId, [FromBody] GradeCreateDto dto)
        {
            var userId = User.GetUserId();

            var submission = await _db.AssignmentSubmissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null) return NotFound();

            // Verify grader has access to submission's department
            var hasAccess = submission.Assignment != null &&
                await _db.DepartmentRoleAssignments
                    .AnyAsync(ra => ra.UserId == userId && ra.DepartmentId == submission.Assignment.DepartmentId &&
                                   (ra.RoleName == "Admin" || ra.RoleName == "HR" || ra.RoleName == "Supervisor"));

            if (!hasAccess) return Forbid();

            var grade = await _db.Grades.FirstOrDefaultAsync(g => g.SubmissionId == submissionId);

            if (grade == null)
            {
                grade = new Grade
                {
                    SubmissionId = submissionId,
                    Score = dto.Score,
                    MaxScore = dto.MaxScore,
                    RubricJson = dto.RubricJson,
                    GradedByUserId = userId,
                    GradedAt = DateTime.UtcNow
                };
                _db.Grades.Add(grade);
            }
            else
            {
                grade.Score = dto.Score;
                grade.MaxScore = dto.MaxScore;
                grade.RubricJson = dto.RubricJson;
                grade.GradedByUserId = userId;
                grade.GradedAt = DateTime.UtcNow;
            }

            // Update submission status to Reviewed when graded
            submission.Status = "Reviewed"; // Keep as string
            await _db.SaveChangesAsync();

            var gradedByUserName = await _db.Users
                .Where(u => u.Id == grade.GradedByUserId)
                .Select(u => $"{u.FirstName} {u.LastName}")
                .FirstOrDefaultAsync() ?? "Unknown";

            var readDto = new GradeReadDto(
                grade.Id, grade.SubmissionId, grade.Score, grade.MaxScore, grade.RubricJson,
                grade.GradedByUserId, grade.GradedAt, gradedByUserName
            );

            return Ok(readDto);
        }

        /// <summary>Get all graded submissions by current supervisor</summary>
        [HttpGet("my-graded-submissions")]
        [Authorize(Roles = "Admin,HR,Supervisor")]
        [ProducesResponseType(typeof(IEnumerable<GradedSubmissionReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GradedSubmissionReadDto>>> GetMyGradedSubmissions()
        {
            var userId = User.GetUserId();

            var gradedSubmissions = await _db.Grades
                .Where(g => g.GradedByUserId == userId)
                .Include(g => g.Submission)
                    .ThenInclude(s => s!.Assignment)
                .Include(g => g.Submission)
                    .ThenInclude(s => s!.User)
                .Select(g => new GradedSubmissionReadDto(
                    g.SubmissionId,
                    g.Submission!.AssignmentId,
                    g.Submission.Assignment!.Title,
                    g.Submission.UserId,
                    $"{g.Submission.User!.FirstName} {g.Submission.User.LastName}",
                    g.Score,
                    g.MaxScore,
                    g.GradedAt,
                    g.Submission.Status // String status
                ))
                .OrderByDescending(g => g.GradedAt)
                .ToListAsync();

            return Ok(gradedSubmissions);
        }

        /// <summary>Get graded submission for current user (Intern/Attaché only)</summary>
        [HttpGet("my-grades")]
        [Authorize(Roles = "Intern,Attache")]
        [ProducesResponseType(typeof(IEnumerable<StudentGradeReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<StudentGradeReadDto>>> GetMyGrades()
        {
            var userId = User.GetUserId();

            var grades = await _db.Grades
                .Where(g => g.Submission!.UserId == userId)
                .Include(g => g.Submission)
                    .ThenInclude(s => s!.Assignment)
                .Include(g => g.GradedByUser)
                .Select(g => new StudentGradeReadDto(
                    g.SubmissionId,
                    g.Submission!.AssignmentId,
                    g.Submission.Assignment!.Title,
                    g.Score,
                    g.MaxScore,
                    g.RubricJson,
                    g.GradedByUserId,
                    $"{g.GradedByUser!.FirstName} {g.GradedByUser.LastName}",
                    g.GradedAt,
                    g.Submission.Status // String status
                ))
                .OrderByDescending(g => g.GradedAt)
                .ToListAsync();

            return Ok(grades);
        }

        /// <summary>Get grade for a specific submission</summary>
        [HttpGet("submissions/{submissionId:long}/grade")]
        [ProducesResponseType(typeof(GradeReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GradeReadDto>> GetSubmissionGrade(long submissionId)
        {
            var userId = User.GetUserId();
            var submission = await _db.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null) return NotFound();

            // Authorization: User can only see their own grade or supervisors can see any in their department
            var isOwner = submission.UserId == userId;
            var isSupervisor = User.IsInRole("Admin") || User.IsInRole("HR") || User.IsInRole("Supervisor");

            if (!isOwner && !isSupervisor)
                return Forbid();

            if (isSupervisor && !isOwner)
            {
                // Verify supervisor has access to this submission's department
                var hasAccess = await _db.DepartmentRoleAssignments
                    .AnyAsync(ra => ra.UserId == userId && ra.DepartmentId == submission.Assignment!.DepartmentId &&
                                   (ra.RoleName == "Admin" || ra.RoleName == "HR" || ra.RoleName == "Supervisor"));
                if (!hasAccess) return Forbid();
            }

            var grade = await _db.Grades
                .Include(g => g.GradedByUser)
                .FirstOrDefaultAsync(g => g.SubmissionId == submissionId);

            if (grade == null) return NotFound();

            var gradedByUserName = $"{grade.GradedByUser!.FirstName} {grade.GradedByUser.LastName}";

            var readDto = new GradeReadDto(
                grade.Id, grade.SubmissionId, grade.Score, grade.MaxScore, grade.RubricJson,
                grade.GradedByUserId, grade.GradedAt, gradedByUserName
            );

            return Ok(readDto);
        }
    }
}