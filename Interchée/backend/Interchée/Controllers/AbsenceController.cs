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
    }
}