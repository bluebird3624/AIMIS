using Interchée.Data;
using Interchée.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AbsenceRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AbsenceRequestsController> _logger;

        public AbsenceRequestsController(AppDbContext context, ILogger<AbsenceRequestsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAbsenceRequests()
        {
            try
            {
                var absenceRequests = await _context.AbsenceRequests
                    .Include(ar => ar.Intern).ThenInclude(i => i.User)
                    .Include(ar => ar.ApprovedBy)
                    .OrderByDescending(ar => ar.RequestedAt)
                    .Select(ar => new
                    {
                        ar.Id,
                        ar.InternId,
                        InternName = $"{ar.Intern.User.FirstName} {ar.Intern.User.LastName}",
                        ar.Reason,
                        ar.StartDate,
                        ar.EndDate,
                        ar.Status,
                        ar.RejectionReason,
                        ApprovedByName = ar.ApprovedBy != null ? $"{ar.ApprovedBy.FirstName} {ar.ApprovedBy.LastName}" : null,
                        ar.RequestedAt,
                        ar.ReviewedAt,
                        TotalDays = (ar.EndDate - ar.StartDate).Days + 1
                    })
                    .ToListAsync();

                return Ok(absenceRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching absence requests");
                return StatusCode(500, "An error occurred while fetching absence requests");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetAbsenceRequest(int id)
        {
            try
            {
                var absenceRequest = await _context.AbsenceRequests
                    .Include(ar => ar.Intern).ThenInclude(i => i.User)
                    .Include(ar => ar.ApprovedBy)
                    .Where(ar => ar.Id == id)
                    .Select(ar => new
                    {
                        ar.Id,
                        ar.InternId,
                        InternName = $"{ar.Intern.User.FirstName} {ar.Intern.User.LastName}",
                        InternEmail = ar.Intern.User.Email,
                        ar.Reason,
                        ar.StartDate,
                        ar.EndDate,
                        ar.Status,
                        ar.RejectionReason,
                        ApprovedByName = ar.ApprovedBy != null ? $"{ar.ApprovedBy.FirstName} {ar.ApprovedBy.LastName}" : null,
                        ApprovedByEmail = ar.ApprovedBy != null ? ar.ApprovedBy.Email : null,
                        ar.RequestedAt,
                        ar.ReviewedAt,
                        TotalDays = (ar.EndDate - ar.StartDate).Days + 1
                    })
                    .FirstOrDefaultAsync();

                if (absenceRequest == null) return NotFound();
                return absenceRequest;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching absence request with ID: {AbsenceRequestId}", id);
                return StatusCode(500, "An error occurred while fetching the absence request");
            }
        }

        [HttpPost]
        public async Task<ActionResult<AbsenceRequest>> CreateAbsenceRequest(CreateAbsenceRequestDto request)
        {
            try
            {
                if (request.StartDate > request.EndDate)
                    return BadRequest("Start date cannot be after end date");

                if (request.StartDate < DateTime.Today)
                    return BadRequest("Cannot request absence for past dates");

                var intern = await _context.Interns
                    .Include(i => i.User)
                    .FirstOrDefaultAsync(i => i.UserId == request.UserId);

                if (intern == null) return BadRequest("Intern not found");

                var absenceRequest = new AbsenceRequest
                {
                    InternId = intern.Id,
                    Reason = request.Reason,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Status = AbsenceStatus.Pending,
                    RequestedAt = DateTime.UtcNow
                };

                _context.AbsenceRequests.Add(absenceRequest);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAbsenceRequest), new { id = absenceRequest.Id }, absenceRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating absence request");
                return StatusCode(500, "An error occurred while creating the absence request");
            }
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveAbsenceRequest(int id, ApproveAbsenceRequestDto request)
        {
            try
            {
                var absenceRequest = await _context.AbsenceRequests
                    .Include(ar => ar.Intern).ThenInclude(i => i.User)
                    .FirstOrDefaultAsync(ar => ar.Id == id);

                if (absenceRequest == null) return NotFound();
                if (absenceRequest.Status != AbsenceStatus.Pending)
                    return BadRequest("Absence request has already been processed");

                var approver = await _context.Users.FindAsync(request.ApproverId);
                if (approver == null) return BadRequest("Approver not found");

                absenceRequest.Status = request.IsApproved ? AbsenceStatus.Approved : AbsenceStatus.Rejected;
                absenceRequest.ApprovedById = request.ApproverId;
                absenceRequest.ReviewedAt = DateTime.UtcNow;

                if (!request.IsApproved && !string.IsNullOrEmpty(request.RejectionReason))
                    absenceRequest.RejectionReason = request.RejectionReason;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing absence request with ID: {AbsenceRequestId}", id);
                return StatusCode(500, "An error occurred while processing the absence request");
            }
        }

        [HttpGet("intern/{internId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetAbsenceRequestsByIntern(int internId)
        {
            try
            {
                var absenceRequests = await _context.AbsenceRequests
                    .Where(ar => ar.InternId == internId)
                    .Include(ar => ar.Intern).ThenInclude(i => i.User)
                    .Include(ar => ar.ApprovedBy)
                    .OrderByDescending(ar => ar.RequestedAt)
                    .Select(ar => new
                    {
                        ar.Id,
                        ar.Reason,
                        ar.StartDate,
                        ar.EndDate,
                        ar.Status,
                        ar.RejectionReason,
                        ApprovedByName = ar.ApprovedBy != null ? $"{ar.ApprovedBy.FirstName} {ar.ApprovedBy.LastName}" : null,
                        ar.RequestedAt,
                        ar.ReviewedAt,
                        TotalDays = (ar.EndDate - ar.StartDate).Days + 1
                    })
                    .ToListAsync();

                return Ok(absenceRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching absence requests for intern ID: {InternId}", internId);
                return StatusCode(500, "An error occurred while fetching absence requests");
            }
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<object>>> GetPendingAbsenceRequests()
        {
            try
            {
                var pendingRequests = await _context.AbsenceRequests
                    .Where(ar => ar.Status == AbsenceStatus.Pending)
                    .Include(ar => ar.Intern).ThenInclude(i => i.User)
                    .Include(ar => ar.Intern.Supervisor)
                    .OrderBy(ar => ar.StartDate)
                    .Select(ar => new
                    {
                        ar.Id,
                        ar.InternId,
                        InternName = $"{ar.Intern.User.FirstName} {ar.Intern.User.LastName}",
                        InternEmail = ar.Intern.User.Email,
                        SupervisorName = ar.Intern.Supervisor != null ?
                            $"{ar.Intern.Supervisor.FirstName} {ar.Intern.Supervisor.LastName}" : "No Supervisor",
                        ar.Reason,
                        ar.StartDate,
                        ar.EndDate,
                        ar.RequestedAt,
                        TotalDays = (ar.EndDate - ar.StartDate).Days + 1
                    })
                    .ToListAsync();

                return Ok(pendingRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending absence requests");
                return StatusCode(500, "An error occurred while fetching pending absence requests");
            }
        }

        [HttpGet("summary/{internId}")]
        public async Task<ActionResult<object>> GetAbsenceSummary(int internId)
        {
            try
            {
                var intern = await _context.Interns
                    .Include(i => i.User)
                    .FirstOrDefaultAsync(i => i.Id == internId);

                if (intern == null) return NotFound("Intern not found");

                var absenceRequests = await _context.AbsenceRequests
                    .Where(ar => ar.InternId == internId)
                    .ToListAsync();

                var summary = new
                {
                    InternId = internId,
                    InternName = $"{intern.User.FirstName} {intern.User.LastName}",
                    TotalAbsenceDays = absenceRequests
                        .Where(r => r.Status == AbsenceStatus.Approved)
                        .Sum(r => (r.EndDate - r.StartDate).Days + 1),
                    PendingRequests = absenceRequests.Count(r => r.Status == AbsenceStatus.Pending),
                    ApprovedRequests = absenceRequests.Count(r => r.Status == AbsenceStatus.Approved),
                    RejectedRequests = absenceRequests.Count(r => r.Status == AbsenceStatus.Rejected),
                    RemainingAbsenceDays = Math.Max(0, 20 - absenceRequests
                        .Where(r => r.Status == AbsenceStatus.Approved)
                        .Sum(r => (r.EndDate - r.StartDate).Days + 1))
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching absence summary for intern ID: {InternId}", internId);
                return StatusCode(500, "An error occurred while fetching absence summary");
            }
        }
    }

    public class CreateAbsenceRequestDto
    {
        public Guid UserId { get; set; }
        public string? Reason { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ApproveAbsenceRequestDto
    {
        public Guid ApproverId { get; set; }
        public bool IsApproved { get; set; }
        public string RejectionReason { get; set; }
    }
}