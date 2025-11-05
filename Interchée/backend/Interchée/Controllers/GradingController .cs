using Interchée.Contracts.Assignments;
using Interchée.Data;
using Interchée.Entities;
using Interchée.Entities.Enums;
using Interchée.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;

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

            // Validate input
            if (dto.MaxScore > 100)
            {
                return BadRequest("Maximum score cannot exceed 100.");
            }

            if (dto.Score > dto.MaxScore)
            {
                return BadRequest("Score cannot exceed maximum score.");
            }

            // Validate rubric if provided
            if (dto.RubricId.HasValue)
            {
                var rubricExists = await _db.Rubrics.AnyAsync(r => r.Id == dto.RubricId.Value && r.IsActive);
                if (!rubricExists)
                {
                    return BadRequest("Invalid rubric ID or rubric is not active.");
                }

                // Validate criteria scores match rubric
                if (dto.CriteriaScores != null)
                {
                    var rubric = await _db.Rubrics
                        .Include(r => r.Items)
                        .FirstOrDefaultAsync(r => r.Id == dto.RubricId.Value);

                    if (rubric != null)
                    {
                        foreach (var criteria in dto.CriteriaScores)
                        {
                            var rubricItem = rubric.Items.FirstOrDefault(i => i.Criteria == criteria.Key);
                            if (rubricItem == null)
                            {
                                return BadRequest($"Invalid criteria: {criteria.Key}");
                            }
                            if (criteria.Value > rubricItem.MaxScore)
                            {
                                return BadRequest($"Score for {criteria.Key} cannot exceed {rubricItem.MaxScore}");
                            }
                        }
                    }
                }
            }

            var submission = await _db.AssignmentSubmissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null) return NotFound("Submission not found");

            // Verify grader has access to submission's department
            var hasAccess = submission.Assignment != null &&
                await _db.DepartmentRoleAssignments
                    .AnyAsync(ra => ra.UserId == userId && ra.DepartmentId == submission.Assignment.DepartmentId &&
                                   (ra.RoleName == "Admin" || ra.RoleName == "HR" || ra.RoleName == "Supervisor"));

            if (!hasAccess) return Forbid();

            var grade = await _db.Grades.FirstOrDefaultAsync(g => g.SubmissionId == submissionId);

            // Serialize criteria scores to JSON
            string? rubricScoresJson = null;
            if (dto.CriteriaScores != null && dto.CriteriaScores.Count != 0)
            {
                rubricScoresJson = JsonSerializer.Serialize(dto.CriteriaScores);
            }

            if (grade == null)
            {
                grade = new Grade
                {
                    SubmissionId = submissionId,
                    Score = dto.Score,
                    MaxScore = dto.MaxScore,
                    RubricId = dto.RubricId,
                    RubricScoresJson = rubricScoresJson, 
                    GradedByUserId = userId,
                    GradedAt = DateTime.UtcNow
                };
                _db.Grades.Add(grade);
            }
            else
            {
                grade.Score = dto.Score;
                grade.MaxScore = dto.MaxScore;
                grade.RubricId = dto.RubricId;
                grade.RubricScoresJson = rubricScoresJson;   
                grade.GradedByUserId = userId;
                grade.GradedAt = DateTime.UtcNow;
            }

            // Update submission status to Reviewed when graded
            submission.Status = SubmissionStatus.Reviewed;
            await _db.SaveChangesAsync();

            var gradedByUserName = await _db.Users
                .Where(u => u.Id == grade.GradedByUserId)
                .Select(u => $"{u.FirstName} {u.LastName}")
                .FirstOrDefaultAsync() ?? "Unknown";

            // Get rubric name if rubric was used
            string? rubricName = null;
            if (grade.RubricId.HasValue)
            {
                rubricName = await _db.Rubrics
                    .Where(r => r.Id == grade.RubricId.Value)
                    .Select(r => r.Name)
                    .FirstOrDefaultAsync();
            }

            // Use the UPDATED GradeReadDto constructor with ALL parameters
            var readDto = new GradeReadDto(
                grade.Id,
                grade.SubmissionId,
                grade.Score,
                grade.MaxScore,
                grade.RubricId,
                rubricName,
                grade.RubricScoresJson,
                grade.GradedByUserId,
                grade.GradedAt,
                gradedByUserName
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
                .OrderByDescending(g => g.GradedAt)
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
                    g.Submission.Status
                ))
                .ToListAsync();

            return Ok(gradedSubmissions);
        }

        /// <summary>Get graded submission for current user (Intern/Attaché only) - WITHOUT RUBRIC IDS</summary>
        [HttpGet("my-grades")]
        [Authorize(Roles = "Intern,Attache")]
        [ProducesResponseType(typeof(IEnumerable<StudentGradeReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<StudentGradeReadDto>>> GetMyGrades()
        {
            var userId = User.GetUserId();

            var grades = await _db.Grades
                .Where(g => g.Submission!.UserId == userId)
                .OrderByDescending(g => g.GradedAt)
                .Include(g => g.Submission)
                    .ThenInclude(s => s!.Assignment)
                .Include(g => g.GradedByUser)
                .Include(g => g.Rubric)
                .Select(g => new
                {
                    g.SubmissionId,
                    g.Submission!.AssignmentId,
                    AssignmentTitle = g.Submission.Assignment!.Title,
                    g.Score,
                    g.MaxScore,
                    g.RubricId,
                    g.RubricScoresJson,
                    RubricName = g.Rubric != null ? g.Rubric.Name : null,
                    g.GradedByUserId,
                    GradedByUserName = $"{g.GradedByUser!.FirstName} {g.GradedByUser.LastName}",
                    g.GradedAt,
                    g.Submission.Status
                })
                .ToListAsync();

            // Build the breakdown for students (without rubric IDs)
            var result = grades.Select(g =>
            {
                List<CriteriaScoreDto>? breakdown = null;

                // Build breakdown from rubric scores if available
                if (!string.IsNullOrEmpty(g.RubricScoresJson) && g.RubricId.HasValue)
                {
                    var criteriaScores = JsonSerializer.Deserialize<Dictionary<string, decimal>>(g.RubricScoresJson);
                    if (criteriaScores != null)
                    {
                        breakdown = criteriaScores.Select(cs => new CriteriaScoreDto(
                            cs.Key,
                            cs.Value
                        )).ToList();
                    }
                }

                return new StudentGradeReadDto(
                    g.SubmissionId,
                    g.AssignmentId,
                    g.AssignmentTitle,
                    g.Score,
                    g.MaxScore,
                    breakdown,
                    g.GradedByUserId,
                    g.GradedByUserName,
                    g.GradedAt,
                    g.Status
                );
            });

            return Ok(result);
        }

        /// <summary>Update an existing grade (Supervisors only)</summary>
        [HttpPut("submissions/{submissionId:long}/grade")]
        [Authorize(Roles = "Admin,HR,Supervisor")]
        [ProducesResponseType(typeof(GradeReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GradeReadDto>> UpdateGrade(long submissionId, [FromBody] GradeUpdateDto dto)
        {
            var userId = User.GetUserId();

            // Validate input
            if (dto.MaxScore > 100)
            {
                return BadRequest("Maximum score cannot exceed 100.");
            }

            if (dto.Score > dto.MaxScore)
            {
                return BadRequest("Score cannot exceed maximum score.");
            }

            // Validate rubric if provided
            if (dto.RubricId.HasValue)
            {
                var rubricExists = await _db.Rubrics.AnyAsync(r => r.Id == dto.RubricId.Value && r.IsActive);
                if (!rubricExists)
                {
                    return BadRequest("Invalid rubric ID or rubric is not active.");
                }
            }

            var grade = await _db.Grades
                .Include(g => g.Submission)
                    .ThenInclude(s => s!.Assignment)
                .Include(g => g.GradedByUser)
                .FirstOrDefaultAsync(g => g.SubmissionId == submissionId);

            if (grade == null) return NotFound("Grade not found");

            // Verify user has access to update this grade's department
            var hasAccess = grade.Submission?.Assignment != null &&
                await _db.DepartmentRoleAssignments
                    .AnyAsync(ra => ra.UserId == userId && ra.DepartmentId == grade.Submission.Assignment.DepartmentId &&
                                   (ra.RoleName == "Admin" || ra.RoleName == "HR" || ra.RoleName == "Supervisor"));

            if (!hasAccess) return Forbid();

            // Serialize criteria scores to JSON
            string? rubricScoresJson = null;
            if (dto.CriteriaScores != null && dto.CriteriaScores.Count != 0)
            {
                rubricScoresJson = JsonSerializer.Serialize(dto.CriteriaScores);
            }

            // Update grade properties
            grade.Score = dto.Score;
            grade.MaxScore = dto.MaxScore;
            grade.RubricId = dto.RubricId;
            grade.RubricScoresJson = rubricScoresJson; // Use RubricScoresJson, NOT RubricJson
            grade.GradedByUserId = userId;
            grade.GradedAt = DateTime.UtcNow;

            // Update submission status to Reviewed when grade is updated
            if (grade.Submission != null)
            {
                grade.Submission.Status = SubmissionStatus.Reviewed;
            }

            await _db.SaveChangesAsync();

            var gradedByUserName = $"{grade.GradedByUser!.FirstName} {grade.GradedByUser.LastName}";

            // Get rubric name if rubric was used
            string? rubricName = null;
            if (grade.RubricId.HasValue)
            {
                rubricName = await _db.Rubrics
                    .Where(r => r.Id == grade.RubricId.Value)
                    .Select(r => r.Name)
                    .FirstOrDefaultAsync();
            }

            // Use the UPDATED GradeReadDto constructor with ALL parameters
            var readDto = new GradeReadDto(
                grade.Id,
                grade.SubmissionId,
                grade.Score,
                grade.MaxScore,
                grade.RubricId,
                rubricName,
                grade.RubricScoresJson,
                grade.GradedByUserId,
                grade.GradedAt,
                gradedByUserName
            );

            return Ok(readDto);
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

            // Get rubric name if rubric was used
            string? rubricName = null;
            if (grade.RubricId.HasValue)
            {
                rubricName = await _db.Rubrics
                    .Where(r => r.Id == grade.RubricId.Value)
                    .Select(r => r.Name)
                    .FirstOrDefaultAsync();
            }

            // Use the UPDATED GradeReadDto constructor with ALL parameters
            var readDto = new GradeReadDto(
                grade.Id,
                grade.SubmissionId,
                grade.Score,
                grade.MaxScore,
                grade.RubricId,
                rubricName,
                grade.RubricScoresJson,
                grade.GradedByUserId,
                grade.GradedAt,
                gradedByUserName
            );

            return Ok(readDto);
        }
    }
}