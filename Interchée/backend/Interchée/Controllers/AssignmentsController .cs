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
    [Route("assignments")]
    [Authorize]
    public class AssignmentsController(AppDbContext db, AssignmentStatusService statusService) : ControllerBase
    {
        private readonly AppDbContext _db = db;
        private readonly AssignmentStatusService _statusService = statusService;

        /// <summary>List assignments in user's departments</summary>
        [HttpGet]
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
                .Select(a => new AssignmentReadDto(
                    a.Id, a.Title, a.Description, a.DepartmentId, a.CreatedByUserId,
                    a.DueAt, a.Status, a.CreatedAt, a.Assignees.Count,
                    a.Submissions.Count(s => s.Status == "Submitted" || s.Status == "Reviewed")
                ))
                .ToListAsync();

            return Ok(assignments);
        }

        /// <summary>Create new assignment</summary>
        [HttpPost]
        [Authorize(Roles = "Admin,HR,Supervisor")]
        [ProducesResponseType(typeof(AssignmentReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AssignmentReadDto>> Create([FromBody] AssignmentCreateDto dto)
        {
            var userId = User.GetUserId();

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
                Status = "Created"
            };

            _db.Assignments.Add(assignment);
            await _db.SaveChangesAsync();

            var readDto = new AssignmentReadDto(
                assignment.Id, assignment.Title, assignment.Description, assignment.DepartmentId,
                assignment.CreatedByUserId, assignment.DueAt, assignment.Status, assignment.CreatedAt,
                0, 0
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
                .CountAsync(s => s.AssignmentId == id && (s.Status == "Submitted" || s.Status == "Reviewed"));

            var readDto = new AssignmentReadDto(
                assignment.Id, assignment.Title, assignment.Description, assignment.DepartmentId,
                assignment.CreatedByUserId, assignment.DueAt, assignment.Status, assignment.CreatedAt,
                assignment.Assignees.Count, submissionCount
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
                .CountAsync(s => s.AssignmentId == id && (s.Status == "Submitted" || s.Status == "Reviewed"));
            var assigneeCount = await _db.AssignmentAssignees
                .CountAsync(aa => aa.AssignmentId == id);

            var readDto = new AssignmentReadDto(
                assignment.Id, assignment.Title, assignment.Description, assignment.DepartmentId,
                assignment.CreatedByUserId, assignment.DueAt, assignment.Status, assignment.CreatedAt,
                assigneeCount, submissionCount
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
            var validStatuses = new[] { "Assigned", "Closed", "Archived" };
            if (!validStatuses.Contains(dto.Status))
                return BadRequest($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}");

            var assignment = await _db.Assignments
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null) return NotFound();

            assignment.Status = dto.Status;
            await _db.SaveChangesAsync();

            var submissionCount = await _db.AssignmentSubmissions
                .CountAsync(s => s.AssignmentId == id && (s.Status == "Submitted" || s.Status == "Reviewed"));
            var assigneeCount = await _db.AssignmentAssignees
                .CountAsync(aa => aa.AssignmentId == id);

            var readDto = new AssignmentReadDto(
                assignment.Id, assignment.Title, assignment.Description, assignment.DepartmentId,
                assignment.CreatedByUserId, assignment.DueAt, assignment.Status, assignment.CreatedAt,
                assigneeCount, submissionCount
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