using Interchée.Contracts.Assignments;
using Interchée.Data;
using Interchée.Entities;
using Interchée.Entities.Enums;
using Interchée.Extensions;
using Interchée.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog.Parsing;
using System.Linq;

namespace Interchée.Controllers
{
    [ApiController]
    [Route("assignments")]
    [Authorize]
    public class AssignmentsController(AppDbContext db, AssignmentStatusService statusService) : ControllerBase
    {
        private readonly AppDbContext _db = db;
        private readonly AssignmentStatusService _statusService = statusService;

        /// <summary>List assignments in user's departments (For Supervisors/Admins)</summary>
        [HttpGet]
        [Authorize(Roles = "Admin,HR,Supervisor")]
        [ProducesResponseType(typeof(IEnumerable<AssignmentReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AssignmentReadDto>>> GetUserAssignments()
        {
            var userId = User.GetUserId();

            // Get departments where user has roles, then get assignments from those departments
            var userDepartmentIds = await _db.DepartmentRoleAssignments
                .Where(ra => ra.UserId == userId)
                .Select(ra => ra.DepartmentId)
                .Distinct()
                .ToListAsync();

            var assignments = await _db.Assignments
                .Where(a => userDepartmentIds.Contains(a.DepartmentId))
                .Include(a => a.Rubric)
                .Select(a => new AssignmentReadDto(
                    a.Id, a.Title, a.Description, a.DepartmentId, a.CreatedByUserId,
                    a.DueAt, a.Status, a.CreatedAt, a.Assignees.Count,
                   a.Submissions.Count(s => s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.Reviewed),
                   a.AllowedSubmissionType,  a.RubricId, a.Rubric.Name 
                ))
                .ToListAsync();

            return Ok(assignments);
        }

        /// <summary>Get assignments assigned to current user (Intern/Attaché only)</summary>
        [HttpGet("my-assignments")]
        [Authorize(Roles = "Intern,Attache")]
        [ProducesResponseType(typeof(IEnumerable<StudentAssignmentReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<StudentAssignmentReadDto>>> GetMyAssignments()
        {
            var userId = User.GetUserId();

            // First, get the assignments assigned to the user
            var assignedAssignments = await _db.AssignmentAssignees
                .Where(aa => aa.UserId == userId)
                .Include(aa => aa.Assignment)
                    .ThenInclude(a => a!.Department)
                .Select(aa => new
                {
                    aa.Assignment,
                    aa.AssignedAt
                })
                .ToListAsync();

            // Then get the user's submissions for these assignments
            var assignmentIds = assignedAssignments.Select(a => a.Assignment!.Id).ToList();
            var userSubmissions = await _db.AssignmentSubmissions
                .Where(s => s.UserId == userId && assignmentIds.Contains(s.AssignmentId))
                .Include(s => s.Grade)
                .ToListAsync();

            // Build the result
            var result = assignedAssignments.Select(aa =>
            {
                var assignment = aa.Assignment!;
                var submission = userSubmissions.FirstOrDefault(s => s.AssignmentId == assignment.Id);

                return new StudentAssignmentReadDto(
                    assignment.Id,
                    assignment.Title,
                    assignment.Description,
                    assignment.DepartmentId,
                    assignment.Department!.Name,
                    assignment.DueAt,
                    assignment.Status,
                    assignment.CreatedAt,
                    aa.AssignedAt,
                    submission != null, // Has submission
                    submission?.Status ?? SubmissionStatus.NotStarted, // Submission status
                    submission?.SubmittedAt, // Submission date
                    submission?.Grade != null // Is graded
                );
            })
            .OrderByDescending(a => a.DueAt)
            .ToList();

            return Ok(result);
        }

        /// <summary>Create new assignment</summary>
        [HttpPost]
        [Authorize(Roles = "Admin,HR,Supervisor")]
        [ProducesResponseType(typeof(AssignmentReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AssignmentReadDto>> Create([FromBody] AssignmentCreateDto dto)
        {
            var userId = User.GetUserId();


            if (dto.DueAt.HasValue && dto.DueAt.Value < DateTime.UtcNow)
            {
                return BadRequest("Due date cannot be in the past. Please set a future due date.");
            }
            //  VALIDATE RUBRIC IF PROVIDED
            var rubricExists = await _db.Rubrics.AnyAsync(r => r.Id == dto.RubricId && r.IsActive);
            if (!rubricExists)
            {
                return BadRequest("Invalid rubric ID or rubric is not active.");
            }
            // Verify user has role in target department
            var hasAccess = await _db.DepartmentRoleAssignments
                .AnyAsync(ra => ra.UserId == userId && ra.DepartmentId == dto.DepartmentId &&
                               (ra.RoleName == "Admin" || ra.RoleName == "HR" || ra.RoleName == "Supervisor"));

            if (!hasAccess) return Forbid();

            var userExists = await _db.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return BadRequest($"User with ID {userId} not found in database. Please log in again.");
            }

            var assignment = new Assignment
            {
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                DepartmentId = dto.DepartmentId,
                CreatedByUserId = userId,
                DueAt = dto.DueAt,
                AllowedSubmissionType = dto.AllowedSubmissionType, 
                RubricId = dto.RubricId, 
                Status = AssignmentStatus.Created
            };

            _db.Assignments.Add(assignment);
            await _db.SaveChangesAsync();

            string rubricName = await _db.Rubrics
       .Where(r => r.Id == assignment.RubricId)
       .Select(r => r.Name)
       .FirstOrDefaultAsync() ?? "Unknown";

            var readDto = new AssignmentReadDto(
                assignment.Id, assignment.Title, assignment.Description, assignment.DepartmentId,
                assignment.CreatedByUserId, assignment.DueAt, assignment.Status, assignment.CreatedAt,
                0, 0, assignment.AllowedSubmissionType, assignment.RubricId, rubricName
            );

            return Ok(readDto);
        }

        /// <summary>Assign users to assignment</summary>
        [HttpPost("{id:long}/assign")]
        [Authorize(Roles = "Admin,HR,Supervisor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignUsers(long id, [FromBody] AssignUsersDto dto)
        {
            var assignment = await _db.Assignments
                .Include(a => a.Assignees)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null) return NotFound();

            // Update assignment status to Assigned when users are assigned
            if (assignment.Status == AssignmentStatus.Created && dto.UserIds.Length != 0)
            {
                assignment.Status = AssignmentStatus.Assigned;
            }

            // Remove existing assignees not in new list
            var existingUserIds = assignment.Assignees.Select(aa => aa.UserId).ToHashSet();
            var newUserIds = dto.UserIds.ToHashSet();

            var toRemove = assignment.Assignees.Where(aa => !newUserIds.Contains(aa.UserId)).ToList();
            foreach (var remove in toRemove)
            {
                _db.AssignmentAssignees.Remove(remove);
            }

            // Add new assignees with student role validation
            foreach (var userId in dto.UserIds.Where(uid => !existingUserIds.Contains(uid)))
            {
                var isStudent = await _db.DepartmentRoleAssignments
                    .AnyAsync(ra => ra.UserId == userId &&
                                   ra.DepartmentId == assignment.DepartmentId &&
                                   (ra.RoleName == "Intern" || ra.RoleName == "Attache"));

                if (isStudent)
                {
                    assignment.Assignees.Add(new AssignmentAssignee
                    {
                        UserId = userId,
                        AssignedAt = DateTime.UtcNow
                    });
                }
            }

            await _db.SaveChangesAsync();
            return Ok();
        }

        /// <summary>Get names of interns/attachés assigned to an assignment</summary>
[HttpGet("{assignmentId:long}/assignees")]
[Authorize(Roles = "Admin,HR,Supervisor")]
[ProducesResponseType(typeof(IEnumerable<AssignmentAssigneeReadDto>), StatusCodes.Status200OK)]
public async Task<ActionResult<IEnumerable<AssignmentAssigneeReadDto>>> GetAssignmentAssignees(long assignmentId)
{
    var userId = User.GetUserId();

    // Verify user has access to this assignment's department
    var assignment = await _db.Assignments
        .FirstOrDefaultAsync(a => a.Id == assignmentId);

    if (assignment == null) return NotFound("Assignment not found");

    var hasAccess = await _db.DepartmentRoleAssignments
        .AnyAsync(ra => ra.UserId == userId && ra.DepartmentId == assignment.DepartmentId &&
                       (ra.RoleName == "Admin" || ra.RoleName == "HR" || ra.RoleName == "Supervisor"));

    if (!hasAccess) return Forbid();

            var assignees = await _db.AssignmentAssignees
                .Where(aa => aa.AssignmentId == assignmentId)
                .Select(aa => new
                {
                    aa.UserId,
                    aa.User!.FirstName,
                    aa.User.LastName,
                    aa.User.Email,
                    aa.AssignedAt,
                    RoleName = _db.DepartmentRoleAssignments
                        .Where(dra => dra.UserId == aa.UserId && dra.DepartmentId == assignment.DepartmentId)
                        .Select(dra => dra.RoleName)
                        .FirstOrDefault()
                })
                .Select(x => new AssignmentAssigneeReadDto(
                    x.UserId,
                    x.FirstName,
                    x.LastName,
                    x.Email!,
                    x.RoleName ?? "Unknown",
                    x.AssignedAt
                ))
                .OrderBy(a => a.FirstName)
                .ThenBy(a => a.LastName)
                .ToListAsync();

            return Ok(assignees);
        }

        /// <summary>Get assignment by ID</summary>
        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(AssignmentReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AssignmentReadDto>> GetById(long id)
        {
            var assignment = await _db.Assignments
                .Include(a => a.Assignees)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null) return NotFound();

            var submissionCount = await _db.AssignmentSubmissions
                .CountAsync(s => s.AssignmentId == id && (s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.Reviewed));

            var readDto = new AssignmentReadDto(
                assignment.Id, assignment.Title, assignment.Description, assignment.DepartmentId,
                assignment.CreatedByUserId, assignment.DueAt, assignment.Status, assignment.CreatedAt,
                assignment.Assignees.Count, submissionCount, assignment.AllowedSubmissionType, assignment.RubricId, null
            );

            return Ok(readDto);
        }

        /// <summary>Update assignment</summary>
        [HttpPut("{id:long}")]
        [Authorize(Roles = "Admin,HR,Supervisor")]
        [ProducesResponseType(typeof(AssignmentReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AssignmentReadDto>> Update(long id, [FromBody] AssignmentUpdateDto dto)
        {
            var userId = User.GetUserId();
            var assignment = await _db.Assignments
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null) return NotFound();

             if (dto.DueAt.HasValue && dto.DueAt.Value < DateTime.UtcNow)
    {
        return BadRequest("Due date cannot be in the past. Please set a future due date.");
    }

            // Verify user has access to update this assignment's department
            var hasAccess = await _db.DepartmentRoleAssignments
                .AnyAsync(ra => ra.UserId == userId && ra.DepartmentId == assignment.DepartmentId &&
                               (ra.RoleName == "Admin" || ra.RoleName == "HR" || ra.RoleName == "Supervisor"));

            if (!hasAccess) return Forbid();

            assignment.Title = dto.Title.Trim();
            assignment.Description = dto.Description?.Trim();
            assignment.DueAt = dto.DueAt;

            // AUTO-CHECK: Update status if deadline passed
            await _statusService.AutoUpdateAssignmentStatus(assignment);

            await _db.SaveChangesAsync();

            var submissionCount = await _db.AssignmentSubmissions
                .CountAsync(s => s.AssignmentId == id && (s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.Reviewed));
            var assigneeCount = await _db.AssignmentAssignees
                .CountAsync(aa => aa.AssignmentId == id);

            var readDto = new AssignmentReadDto(
                assignment.Id, assignment.Title, assignment.Description, assignment.DepartmentId,
                assignment.CreatedByUserId, assignment.DueAt, assignment.Status, assignment.CreatedAt,
                assigneeCount, submissionCount, assignment.AllowedSubmissionType, assignment.RubricId, null
            );

            return Ok(readDto);
        }

        /// <summary>Update assignment status</summary>
        [HttpPut("{id:long}/status")]
        [Authorize(Roles = "Admin,HR,Supervisor")]
        [ProducesResponseType(typeof(AssignmentReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AssignmentReadDto>> UpdateStatus(long id, [FromBody] AssignmentStatusDto dto)
        {
            var validStatuses = new[] { AssignmentStatus.Assigned, AssignmentStatus.Closed, AssignmentStatus.Archived };
            if (!validStatuses.Contains(dto.Status))
                return BadRequest($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}");

            var assignment = await _db.Assignments
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null) return NotFound();

            assignment.Status = dto.Status;
            await _db.SaveChangesAsync();

            var submissionCount = await _db.AssignmentSubmissions
                .CountAsync(s => s.AssignmentId == id && (s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.Reviewed));
            var assigneeCount = await _db.AssignmentAssignees
                .CountAsync(aa => aa.AssignmentId == id);

            var readDto = new AssignmentReadDto(
                assignment.Id, assignment.Title, assignment.Description, assignment.DepartmentId,
                assignment.CreatedByUserId, assignment.DueAt, assignment.Status, assignment.CreatedAt,
                assigneeCount, submissionCount, assignment.AllowedSubmissionType, assignment.RubricId, null
            );

            return Ok(readDto);
        }

        /// <summary>Delete assignment</summary>
        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Admin,HR,Supervisor")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(long id)
        {
            var assignment = await _db.Assignments
                .Include(a => a.Assignees)
                .Include(a => a.Submissions)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null) return NotFound();

            // Check if there are submissions (prevent orphaned data)
            if (assignment.Submissions.Count != 0)
            {
                return Conflict(new
                {
                    message = "Cannot delete assignment with existing submissions. Archive it instead.",
                    submissionCount = assignment.Submissions.Count
                });
            }

            // Remove assignees first (due to foreign key constraints)
            _db.AssignmentAssignees.RemoveRange(assignment.Assignees);
            _db.Assignments.Remove(assignment);

            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Get assignment progress summary</summary>
        [HttpGet("{id:long}/progress")]
        [Authorize(Roles = "Admin,HR,Supervisor")]
        [ProducesResponseType(typeof(AssignmentProgressDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<AssignmentProgressDto>> GetProgress(long id)
        {
            var progress = await _statusService.GetAssignmentProgress(id);

            if (progress == null) return NotFound();

            return Ok(progress);
        }
    }
}