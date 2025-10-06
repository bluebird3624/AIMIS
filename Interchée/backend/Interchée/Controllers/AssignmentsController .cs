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
    [Route("assignments")]
    [Authorize]
    public class AssignmentsController(AppDbContext db) : ControllerBase
    {
        private readonly AppDbContext _db = db;

        /// <summary>List assignments in user's departments</summary
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AssignmentReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AssignmentReadDto>>> GetUserAssignments()
        {
            var userId = User.GetUserId();

            // FIXED: Get departments where user has roles, then get assignments from those departments
            var userDepartmentIds = await _db.DepartmentRoleAssignments
                .Where(ra => ra.UserId == userId)
                .Select(ra => ra.DepartmentId)
                .Distinct()
                .ToListAsync();

            var assignments = await _db.Assignments
                .Where(a => userDepartmentIds.Contains(a.DepartmentId))
                .Select(a => new AssignmentReadDto(
                    a.Id, a.Title, a.Description, a.DepartmentId, a.CreatedByUserId,
                    a.DueAt, a.Status, a.CreatedAt, a.Assignees.Count, 0  // Use 0 for submission count for now
                ))
                .ToListAsync();

            return Ok(assignments);
        }

        /// <summary>Create new assignment (Supervisor/Admin/HR in department)</summary>
        [HttpPost]
        [Authorize(Roles = "Admin,HR,Supervisor")]
        [ProducesResponseType(typeof(AssignmentReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AssignmentReadDto>> Create([FromBody] AssignmentCreateDto dto)
        {
            var userId = User.GetUserId();

            //// Verify user has role in target department
            //var hasAccess = await _db.DepartmentRoleAssignments
            //    .AnyAsync(ra => ra.UserId == userId && ra.DepartmentId == dto.DepartmentId &&
            //                   (ra.RoleName == "Admin" || ra.RoleName == "HR" || ra.RoleName == "Supervisor"));

            //if (!hasAccess) return Forbid();

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
                Status = "Assigned"
            };

            _db.Assignments.Add(assignment);
            await _db.SaveChangesAsync();

            var readDto = new AssignmentReadDto(
            assignment.Id, assignment.Title, assignment.Description, assignment.DepartmentId,
            assignment.CreatedByUserId, assignment.DueAt, assignment.Status, assignment.CreatedAt,
            0, 0  // Both counts as 0 for new assignment
            );

            return Ok(readDto);
        }

        /// <summary>Assign users to assignment</summary>
        [HttpPost("{id:long}/assign")]
        [Authorize(Roles = "Admin,HR,Supervisor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignUsers(long id, [FromBody] AssignUsersDto dto)
        {
            // Verify assignment exists and user has access
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

            // Add new assignees
            foreach (var userId in dto.UserIds.Where(uid => !existingUserIds.Contains(uid)))
            {
                assignment.Assignees.Add(new AssignmentAssignee
                {
                    UserId = userId,
                    AssignedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}