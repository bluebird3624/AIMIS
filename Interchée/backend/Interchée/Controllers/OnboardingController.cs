using Interchée.Contracts.Onboarding;
using Interchée.Data;
using Interchée.Entities;
using Interchée.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Controllers
{
    [ApiController]
    [Route("onboarding-requests")]
    public class OnboardingRequestsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly OnboardingService _svc;
        private readonly UserManager<AppUser> _users;
        private static readonly HashSet<string> KnownIdentityRoles =
            new(StringComparer.OrdinalIgnoreCase) { "Admin", "HR", "Supervisor", "Instructor", "Intern" };

        public OnboardingRequestsController(AppDbContext db, OnboardingService svc, UserManager<AppUser> users)
        {
            _db = db;
            _svc = svc;
            _users = users;
        }

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
        /// Approve a pending request (Admin/HR). Creates the AppUser and assigns department role.
        /// Pass the department-scoped role via query (e.g., ?roleName=Intern).
        /// Optionally, we also grant the <em>global Identity</em> role with the same name if it exists.
        /// </summary>
        [HttpPost("{id:long}/approve")]
        [Authorize(Roles = "Admin,HR")]
        [ProducesResponseType(typeof(OnboardingRequestReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OnboardingRequestReadDto>> Approve(long id,[FromBody] ApproveOnboardingDto dto,[FromQuery] string? roleName = null)   // optional: allow query override if provided
        {
            try
            {
                var approverId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(approverId, out var approverGuid))
                    return Unauthorized();

                // Prefer query roleName if provided; otherwise use body RoleName
                var role = string.IsNullOrWhiteSpace(roleName) ? dto.RoleName : roleName;
                role = role.Trim();

                var result = await _svc.ApproveAsync(id, dto, approverGuid, role);

                // (Optional) also add Identity global role if you keep those in sync
                var email = result.Email.Trim().ToLowerInvariant();
                var user = await _users.FindByEmailAsync(email);
                if (user != null && !(await _users.IsInRoleAsync(user, role)))
                {
                    await _users.AddToRoleAsync(user, role);
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
