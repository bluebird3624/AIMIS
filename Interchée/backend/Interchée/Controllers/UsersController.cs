using Interchée.Contracts.Users;
using Interchée.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Interchée.Controllers
{
    /// <summary>
    /// Admin/HR endpoints for managing users (list, get, create, enable/disable).
    /// Department roles and identity roles are handled by their respective endpoints.
    /// </summary>
    [ApiController]
    [Route("users")]
    [Authorize(Roles = "Admin,HR")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _svc;

        public UsersController(UserService svc) => _svc = svc;

        /// <summary>
        /// List users. Optional filter by departmentId+roleName to list only users holding that department role.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserSummaryDto>>> GetAll([FromQuery] int? departmentId, [FromQuery] string? roleName)
            => Ok(await _svc.GetAllAsync(departmentId, roleName));

        /// <summary>
        /// Get one user by id.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(UserSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserSummaryDto>> Get(Guid id)
        {
            var u = await _svc.GetAsync(id);
            return u is null ? NotFound() : Ok(u);
        }

        /// <summary>
        /// Create a user directly (bypasses onboarding).
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(UserSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserSummaryDto>> Create([FromBody] CreateUserDto dto)
        {
            try
            {
                var created = await _svc.CreateAsync(dto);
                return Ok(created);
            }
            catch (InvalidOperationException ex)
            {
                // Map Identity errors to 400 with a friendly message
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Enable/disable an account. Disabled users cannot log in.
        /// </summary>
        [HttpPut("{id:guid}/active")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleActive(Guid id, [FromBody] ToggleActiveDto dto)
        {
            try
            {
                await _svc.ToggleActiveAsync(id, dto.IsActive);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
