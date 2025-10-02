using Interchée.Contracts.Onboarding;
using Interchée.Data;
using Interchée.Entities;
using Interchée.Services;
using Interchée.Utils;                  //  canonicalize roles
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Interchée.Controllers
{
    [ApiController]
    [Route("onboarding-requests")]
    public class OnboardingRequestsController(AppDbContext db, OnboardingService svc, UserManager<AppUser> users) : ControllerBase
    {
        private readonly AppDbContext _db = db;
        private readonly OnboardingService _svc = svc;
        private readonly UserManager<AppUser> _users = users;

        /// <summary>Create a pending onboarding request (anonymous or HR).</summary>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(OnboardingRequestReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<OnboardingRequestReadDto>> Create(OnboardingRequestCreateDto dto)
        {
            try
            {
                var result = await _svc.CreateAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>List requests (Admin/HR). Filter by status and/or department.</summary>
        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        [ProducesResponseType(typeof(IEnumerable<OnboardingRequestReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OnboardingRequestReadDto>>> GetAll([FromQuery] string? status = null, [FromQuery] int? departmentId = null)
            => Ok(await _svc.GetAsync(status, departmentId));

        /// <summary>
        /// Approve a pending request (Admin/HR). Creates the AppUser and assigns a department-scoped role.
        /// Role is read from body (dto.RoleName); an optional ?roleName= query can override it.
        /// The role text is canonicalized (e.g., "attaché" -> "Attache") against Interchée.Config.Roles.
        /// </summary>
        [HttpPost("{id:long}/approve")]
        [Authorize(Roles = "Admin,HR")]
        [ProducesResponseType(typeof(OnboardingRequestReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OnboardingRequestReadDto>> Approve(
            long id,
            [FromBody] ApproveOnboardingDto dto,
            [FromQuery] string? roleName = null)
        {
            try
            {
                var approverId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(approverId, out var approverGuid))
                    return Unauthorized();

                // Prefer query override if present; else take from body
                var rawRole = string.IsNullOrWhiteSpace(roleName) ? dto.RoleName : roleName;
                rawRole = rawRole?.Trim() ?? string.Empty;

                //  Canonicalize to your config constants (Admin/HR/Supervisor/Attache/Intern)
                var canonical = RoleHelper.ToCanonical(rawRole);
                if (canonical is null)
                    return BadRequest(new { message = "Invalid role name." });

                // Approve: creates user, assigns department role (canonical)
                var result = await _svc.ApproveAsync(id, dto, approverGuid, canonical);

                // (Optional) also grant the GLOBAL Identity role using the same canonical name
                var email = result.Email.Trim().ToLowerInvariant();
                var user = await _users.FindByEmailAsync(email);
                if (user != null && !(await _users.IsInRoleAsync(user, canonical)))
                {
                    await _users.AddToRoleAsync(user, canonical);
                }

                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Reject a pending request (Admin/HR).</summary>
        [HttpPost("{id:long}/reject")]
        [Authorize(Roles = "Admin,HR")]
        [ProducesResponseType(typeof(OnboardingRequestReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OnboardingRequestReadDto>> Reject(long id, [FromBody] RejectOnboardingDto dto)
        {
            try
            {
                var approverId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(approverId, out var approverGuid))
                    return Unauthorized();

                var result = await _svc.RejectAsync(id, approverGuid);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
