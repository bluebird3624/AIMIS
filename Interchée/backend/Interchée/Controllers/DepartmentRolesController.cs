using Interchée.Contracts.Roles;
using Interchée.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Interchée.Controllers
{
    /// <summary>
    /// Admin/HR endpoints for managing department-scoped roles (User ↔ RoleName ↔ Department).
    /// </summary>
    [ApiController]
    [Route("department-roles")]
    [Authorize(Roles = "Admin,HR")]
    public class DepartmentRolesController(RoleAssignmentService svc) : ControllerBase
    {
        private readonly RoleAssignmentService _svc = svc;

        /// <summary>
        /// Assign a department role to a user (e.g., Instructor in Department 1).
        /// </summary>
        [HttpPost("assign")]
        [ProducesResponseType(typeof(UserRoleInDepartmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserRoleInDepartmentDto>> Assign([FromBody] AssignRoleDto dto)
        {
            try
            {
                var result = await _svc.AssignAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Remove a department role from a user. Idempotent (no error if it wasn't assigned).
        /// </summary>
        [HttpDelete("unassign")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Unassign([FromBody] UnassignRoleDto dto)
        {
            await _svc.UnassignAsync(dto);
            return NoContent();
        }

        /// <summary>
        /// List all department roles for a specific user.
        /// </summary>
        [HttpGet("user/{userId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<UserRoleInDepartmentDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserRoleInDepartmentDto>>> GetUserRoles(Guid userId)
            => Ok(await _svc.GetUserRolesAsync(userId));
    }
}
