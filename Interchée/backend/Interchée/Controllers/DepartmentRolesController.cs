using Interchée.Contracts.Roles;
using Interchée.Data;
using Interchée.Entities;
using Interchée.Utils;                      //  canonicalize roles
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Controllers
{
    /// <summary>
    /// Assign/unassign department-scoped roles to users.
    /// Roles are canonicalized to Interchée.Config.Roles (e.g., "attaché" -> "Attache").
    /// </summary>
    [ApiController]
    [Route("department-roles")]
    public class DepartmentRolesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _users;

        public DepartmentRolesController(AppDbContext db, UserManager<AppUser> users)
        {
            _db = db;
            _users = users;
        }

        /// <summary>
        /// Assign a department-scoped role to a user (idempotent).
        /// Also (optionally) adds the GLOBAL Identity role with the same canonical name.
        /// </summary>
        [HttpPost("assign")]
        [Authorize(Roles = "Admin,HR")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Assign([FromBody] AssignRoleDto dto)
        {
            // Canonicalize role (accepts attache/attaché/etc.)
            var canonical = RoleHelper.ToCanonical(dto.RoleName);
            if (canonical is null)
                return BadRequest(new { message = "Invalid role name." });

            // User must exist
            var user = await _users.FindByIdAsync(dto.UserId.ToString());
            if (user is null) return NotFound(new { message = "User not found." });

            // Department must exist (active optional; uncomment if you want to enforce)
            var dept = await _db.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == dto.DepartmentId);
            if (dept is null) return NotFound(new { message = "Department not found." });
            // if (!dept.IsActive) return BadRequest(new { message = "Department is inactive." });

            // Idempotent check
            var exists = await _db.DepartmentRoleAssignments
                .AnyAsync(a => a.UserId == dto.UserId
                               && a.DepartmentId == dto.DepartmentId
                               && a.RoleName == canonical);
            if (!exists)
            {
                _db.DepartmentRoleAssignments.Add(new DepartmentRoleAssignment
                {
                    UserId = dto.UserId,
                    DepartmentId = dto.DepartmentId,
                    RoleName = canonical,
                    AssignedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
            }

            // Optional: keep GLOBAL Identity role in sync
            if (!await _users.IsInRoleAsync(user, canonical))
            {
                // Ignore errors silently; department role was already stored above
                await _users.AddToRoleAsync(user, canonical);
            }

            return Ok(new
            {
                message = "Role assigned.",
                userId = dto.UserId,
                departmentId = dto.DepartmentId,
                roleName = canonical
            });
        }

        /// <summary>
        /// Remove a department-scoped role from a user (idempotent).
        /// Does not remove GLOBAL Identity role (leave that to separate admin flow if desired).
        /// </summary>
        [HttpDelete("unassign")]
        [Authorize(Roles = "Admin,HR")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Unassign([FromBody] UnassignRoleDto dto)
        {
            var canonical = RoleHelper.ToCanonical(dto.RoleName);
            if (canonical is null)
                return BadRequest(new { message = "Invalid role name." });

            // Validate user + department existence to give good errors
            var userExists = await _users.FindByIdAsync(dto.UserId.ToString()) != null;
            if (!userExists) return NotFound(new { message = "User not found." });

            var deptExists = await _db.Departments.AsNoTracking().AnyAsync(d => d.Id == dto.DepartmentId);
            if (!deptExists) return NotFound(new { message = "Department not found." });

            var assignment = await _db.DepartmentRoleAssignments.FirstOrDefaultAsync(a =>
                a.UserId == dto.UserId &&
                a.DepartmentId == dto.DepartmentId &&
                a.RoleName == canonical);

            if (assignment != null)
            {
                _db.DepartmentRoleAssignments.Remove(assignment);
                await _db.SaveChangesAsync();
            }

            // NOTE: we do not remove the GLOBAL Identity role here.
            // If you want to do that as well, uncomment:
            // var user = await _users.FindByIdAsync(dto.UserId.ToString());
            // if (user != null && await _users.IsInRoleAsync(user, canonical))
            //     await _users.RemoveFromRoleAsync(user, canonical);

            return NoContent();
        }
    }
}
