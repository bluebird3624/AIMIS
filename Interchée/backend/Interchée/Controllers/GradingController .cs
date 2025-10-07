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
    [Authorize(Roles = "Admin,HR,Supervisor")]
    public class GradingController(AppDbContext db) : ControllerBase
    {
        private readonly AppDbContext _db = db;

        /// <summary>Grade a submission</summary>
        [HttpPost("submissions/{submissionId:long}/grade")]
        [ProducesResponseType(typeof(GradeReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<GradeReadDto>> GradeSubmission(long submissionId, [FromBody] GradeCreateDto dto)
        {
            var userId = User.GetUserId();

            var submission = await _db.AssignmentSubmissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null) return NotFound();

            // Verify grader has access to submission's department
            var hasAccess = await _db.DepartmentRoleAssignments
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

            submission.Status = "Reviewed";
            await _db.SaveChangesAsync();

            var readDto = new GradeReadDto(
                grade.Id, grade.SubmissionId, grade.Score, grade.MaxScore, grade.RubricJson,
                grade.GradedByUserId, grade.GradedAt, ""
            );

            return Ok(readDto);
        }
    }
}