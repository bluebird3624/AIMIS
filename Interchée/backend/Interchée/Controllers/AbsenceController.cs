using Interchée.Contracts.Absence;
using Interchée.Data;
using Interchée.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Interchée.Controllers
{
    [ApiController]
    [Route("absence-requests")]
    public class AbsenceRequestsController(AppDbContext db) : ControllerBase
    {
        private readonly AppDbContext _db = db;

        /// <summary>Submit a new absence request</summary>
        [HttpPost]
        [Authorize(Roles = "Intern,Attache")]
        [ProducesResponseType(typeof(AbsenceRequestReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<AbsenceRequestReadDto>> Create([FromBody] AbsenceRequestCreateDto dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Get user's department from role assignments
            var userDept = await _db.DepartmentRoleAssignments
                .Where(x => x.UserId == userId && (x.RoleName == "Intern" || x.RoleName == "Attache"))
                .Select(x => x.DepartmentId)
                .FirstOrDefaultAsync();

            if (userDept == 0) return BadRequest("User is not assigned to a department as Intern/Attache");

            var days = (decimal)(dto.EndDate.DayNumber - dto.StartDate.DayNumber) + 1;

            var request = new AbsenceRequest
            {
                UserId = userId,
                DepartmentId = userDept,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Days = days,
                Reason = dto.Reason.Trim(),
                Status = "Pending",
                RequestedAt = DateTime.UtcNow
            };

            _db.AbsenceRequests.Add(request);
            await _db.SaveChangesAsync();

            var readDto = new AbsenceRequestReadDto(
                request.Id, request.UserId, request.DepartmentId,
                request.StartDate, request.EndDate, request.Days,
                request.Reason, request.Status, request.RequestedAt, null
            );

            return Ok(readDto);
        }

        /// <summary>Get my absence requests</summary>
        [HttpGet("my")]
        [Authorize(Roles = "Intern,Attache")]
        [ProducesResponseType(typeof(IEnumerable<AbsenceRequestReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AbsenceRequestReadDto>>> GetMyRequests()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var requests = await _db.AbsenceRequests
                .Where(x => x.UserId == userId)
                .Include(x => x.Decision)
                .OrderByDescending(x => x.RequestedAt)
                .Select(x => new AbsenceRequestReadDto(
                    x.Id, x.UserId, x.DepartmentId,
                    x.StartDate, x.EndDate, x.Days,
                    x.Reason, x.Status, x.RequestedAt,
                    x.Decision != null ? new AbsenceDecisionReadDto(
                        x.Decision.Id, x.Decision.DecidedByUserId,
                        x.Decision.Decision, x.Decision.Comment, x.Decision.DecidedAt
                    ) : null
                ))
                .ToListAsync();

            return Ok(requests);
        }

        /// <summary>Get department absence requests (for Supervisors/HR)</summary>
        [HttpGet("department/{departmentId:int}")]
        [Authorize(Roles = "Supervisor,HR,Admin")]
        [ProducesResponseType(typeof(IEnumerable<AbsenceRequestReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AbsenceRequestReadDto>>> GetDepartmentRequests(int departmentId)
        {
            var requests = await _db.AbsenceRequests
                .Where(x => x.DepartmentId == departmentId)
                .Include(x => x.User)
                .Include(x => x.Decision)
                .OrderByDescending(x => x.RequestedAt)
                .Select(x => new AbsenceRequestReadDto(
                    x.Id, x.UserId, x.DepartmentId,
                    x.StartDate, x.EndDate, x.Days,
                    x.Reason, x.Status, x.RequestedAt,
                    x.Decision != null ? new AbsenceDecisionReadDto(
                        x.Decision.Id, x.Decision.DecidedByUserId,
                        x.Decision.Decision, x.Decision.Comment, x.Decision.DecidedAt
                    ) : null
                ))
                .ToListAsync();

            return Ok(requests);
        }

        /// <summary>Approve/Reject an absence request</summary>
        [HttpPost("{id:long}/decision")]
        [Authorize(Roles = "Supervisor,HR,Admin")]
        [ProducesResponseType(typeof(AbsenceRequestReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<AbsenceRequestReadDto>> Decide(
            long id, [FromBody] AbsenceDecisionCreateDto dto)
        {
            var request = await _db.AbsenceRequests
                .Include(x => x.Decision)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (request == null) return NotFound();
            if (request.Decision != null) return BadRequest("Request already has a decision");

            var decidedByUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var decision = new AbsenceDecision
            {
                RequestId = id,
                DecidedByUserId = decidedByUserId,
                Decision = dto.Decision,
                Comment = dto.Comment?.Trim(),
                DecidedAt = DateTime.UtcNow
            };

            request.Status = dto.Decision; // Approved|Rejected
            request.Decision = decision;

            await _db.SaveChangesAsync();

            var readDto = new AbsenceRequestReadDto(
                request.Id, request.UserId, request.DepartmentId,
                request.StartDate, request.EndDate, request.Days,
                request.Reason, request.Status, request.RequestedAt,
                new AbsenceDecisionReadDto(
                    decision.Id, decision.DecidedByUserId,
                    decision.Decision, decision.Comment, decision.DecidedAt
                )
            );

            return Ok(readDto);
        }

        /// <summary>Update my absence request (only if Pending)</summary>
        [HttpPut("{id:long}")]
        [Authorize(Roles = "Intern,Attache")]
        [ProducesResponseType(typeof(AbsenceRequestReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AbsenceRequestReadDto>> UpdateMyRequest(
            long id, [FromBody] AbsenceRequestCreateDto dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var request = await _db.AbsenceRequests
                .Include(x => x.Decision)
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (request == null) return NotFound();
            if (request.Status != "Pending") return BadRequest("Can only update pending requests");

            var days = (decimal)(dto.EndDate.DayNumber - dto.StartDate.DayNumber) + 1;

            request.StartDate = dto.StartDate;
            request.EndDate = dto.EndDate;
            request.Days = days;
            request.Reason = dto.Reason.Trim();

            await _db.SaveChangesAsync();

            var readDto = new AbsenceRequestReadDto(
                request.Id, request.UserId, request.DepartmentId,
                request.StartDate, request.EndDate, request.Days,
                request.Reason, request.Status, request.RequestedAt,
                request.Decision != null ? new AbsenceDecisionReadDto(
                    request.Decision.Id, request.Decision.DecidedByUserId,
                    request.Decision.Decision, request.Decision.Comment, request.Decision.DecidedAt
                ) : null
            );

            return Ok(readDto);
        }
        /// <summary>Delete my absence request (only if Pending)</summary>
        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Intern,Attache")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteMyRequest(long id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var request = await _db.AbsenceRequests
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (request == null) return NotFound();
            if (request.Status != "Pending") return BadRequest("Can only delete pending requests");

            _db.AbsenceRequests.Remove(request);
            await _db.SaveChangesAsync();

            return NoContent();
        }
        /// <summary>Create or update absence limit policy</summary>
        [HttpPost("policies")]
        [Authorize(Roles = "HR,Admin")]
        [ProducesResponseType(typeof(AbsencePolicyReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AbsencePolicyReadDto>> CreatePolicy([FromBody] AbsencePolicyCreateDto dto)
        {
            // Validate department exists if scope is Department
            if (dto.Scope == "Department" && dto.DepartmentId.HasValue)
            {
                var departmentExists = await _db.Departments.AnyAsync(d => d.Id == dto.DepartmentId.Value);
                if (!departmentExists) return BadRequest("Department not found");
            }

            // Check for overlapping policies
            var overlapping = await _db.AbsenceLimitPolicies
                .Where(p => p.Scope == dto.Scope &&
                           p.DepartmentId == dto.DepartmentId &&
                           p.EffectiveFrom <= (dto.EffectiveTo ?? DateOnly.MaxValue) &&
                           (p.EffectiveTo >= dto.EffectiveFrom || p.EffectiveTo == null))
                .AnyAsync();

            if (overlapping) return BadRequest("Policy already exists for this period");

            var policy = new AbsenceLimitPolicy
            {
                Scope = dto.Scope,
                DepartmentId = dto.Scope == "Department" ? dto.DepartmentId : null,
                MaxDaysPerTerm = dto.MaxDaysPerTerm,
                MaxDaysPerMonth = dto.MaxDaysPerMonth,
                EffectiveFrom = dto.EffectiveFrom,
                EffectiveTo = dto.EffectiveTo
            };

            _db.AbsenceLimitPolicies.Add(policy);
            await _db.SaveChangesAsync();

            var readDto = new AbsencePolicyReadDto(
                policy.Id, policy.Scope, policy.DepartmentId,
                policy.MaxDaysPerTerm, policy.MaxDaysPerMonth,
                policy.EffectiveFrom, policy.EffectiveTo
            );

            return Ok(readDto);
        }

        /// <summary>Get all absence limit policies</summary>
        [HttpGet("policies")]
        [Authorize(Roles = "HR,Admin,Supervisor")]
        [ProducesResponseType(typeof(IEnumerable<AbsencePolicyReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AbsencePolicyReadDto>>> GetPolicies()
        {
            var policies = await _db.AbsenceLimitPolicies
                .Include(p => p.Department)
                .OrderBy(p => p.Scope)
                .ThenBy(p => p.DepartmentId)
                .ThenBy(p => p.EffectiveFrom)
                .Select(p => new AbsencePolicyReadDto(
                    p.Id, p.Scope, p.DepartmentId,
                    p.MaxDaysPerTerm, p.MaxDaysPerMonth,
                    p.EffectiveFrom, p.EffectiveTo
                ))
                .ToListAsync();

            return Ok(policies);
        }

        /// <summary>Get current effective policy for a department</summary>
        [HttpGet("policies/current")]
        [Authorize]
        [ProducesResponseType(typeof(AbsencePolicyReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<AbsencePolicyReadDto>> GetCurrentPolicy(
            [FromQuery] int? departmentId = null)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Get global policy
            var globalPolicy = await _db.AbsenceLimitPolicies
                .Where(p => p.Scope == "Global" &&
                           p.EffectiveFrom <= today &&
                           (p.EffectiveTo >= today || p.EffectiveTo == null))
                .OrderByDescending(p => p.EffectiveFrom)
                .FirstOrDefaultAsync();

            // Get department-specific policy if departmentId provided
            AbsenceLimitPolicy? departmentPolicy = null;
            if (departmentId.HasValue)
            {
                departmentPolicy = await _db.AbsenceLimitPolicies
                    .Where(p => p.Scope == "Department" &&
                               p.DepartmentId == departmentId &&
                               p.EffectiveFrom <= today &&
                               (p.EffectiveTo >= today || p.EffectiveTo == null))
                    .OrderByDescending(p => p.EffectiveFrom)
                    .FirstOrDefaultAsync();
            }

            // Prefer department policy over global policy
            var effectivePolicy = departmentPolicy ?? globalPolicy;
            if (effectivePolicy == null) return NotFound("No active policy found");

            var readDto = new AbsencePolicyReadDto(
                effectivePolicy.Id, effectivePolicy.Scope, effectivePolicy.DepartmentId,
                effectivePolicy.MaxDaysPerTerm, effectivePolicy.MaxDaysPerMonth,
                effectivePolicy.EffectiveFrom, effectivePolicy.EffectiveTo
            );

            return Ok(readDto);
        }

        /// <summary>Delete a policy</summary>
        [HttpDelete("policies/{id:int}")]
        [Authorize(Roles = "HR,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePolicy(int id)
        {
            var policy = await _db.AbsenceLimitPolicies.FindAsync(id);
            if (policy == null) return NotFound();

            _db.AbsenceLimitPolicies.Remove(policy);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}