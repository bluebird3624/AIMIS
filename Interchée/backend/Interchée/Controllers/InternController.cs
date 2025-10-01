using Interchée.Data;
using Interchée.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InternsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<InternsController> _logger;

        public InternsController(AppDbContext context, ILogger<InternsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetInterns()
        {
            try
            {
                var interns = await _context.Interns
                    .Include(i => i.User)
                    .Include(i => i.Supervisor)
                    .Select(i => new
                    {
                        i.Id,
                        i.UserId,
                        InternName = $"{i.User.FirstName} {i.User.LastName}",
                        i.User.Email,
                        i.University,
                        i.CourseOfStudy,
                        i.StartDate,
                        i.EndDate,
                        i.Status,
                        SupervisorName = i.Supervisor != null ?
                            $"{i.Supervisor.FirstName} {i.Supervisor.LastName}" : "No Supervisor",
                        TotalAbsenceRequests = i.AbsenceRequests.Count,
                        PendingAbsenceRequests = i.AbsenceRequests.Count(ar => ar.Status == AbsenceStatus.Pending)
                    })
                    .ToListAsync();

                return Ok(interns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching interns");
                return StatusCode(500, "An error occurred while fetching interns");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetIntern(int id)
        {
            try
            {
                var intern = await _context.Interns
                    .Include(i => i.User)
                    .Include(i => i.Supervisor)
                    .Include(i => i.AbsenceRequests)
                    .Where(i => i.Id == id)
                    .Select(i => new
                    {
                        i.Id,
                        i.UserId,
                        InternName = $"{i.User.FirstName} {i.User.LastName}",
                        i.User.Email,
                        i.University,
                        i.CourseOfStudy,
                        i.StartDate,
                        i.EndDate,
                        i.Status,
                        SupervisorName = i.Supervisor != null ?
                            $"{i.Supervisor.FirstName} {i.Supervisor.LastName}" : "No Supervisor",
                        SupervisorEmail = i.Supervisor != null ? i.Supervisor.Email : null,
                        AbsenceRequests = i.AbsenceRequests.Select(ar => new
                        {
                            ar.Id,
                            ar.Reason,
                            ar.StartDate,
                            ar.EndDate,
                            ar.Status,
                            ar.RequestedAt,
                            TotalDays = (ar.EndDate - ar.StartDate).Days + 1
                        }).OrderByDescending(ar => ar.RequestedAt).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (intern == null) return NotFound();
                return intern;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching intern with ID: {InternId}", id);
                return StatusCode(500, "An error occurred while fetching the intern");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Intern>> CreateIntern(CreateInternDto request)
        {
            try
            {
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null) return BadRequest("User not found");

                var existingIntern = await _context.Interns
                    .FirstOrDefaultAsync(i => i.UserId == request.UserId);
                if (existingIntern != null) return BadRequest("Intern already exists for this user");

                if (request.SupervisorId.HasValue)
                {
                    var supervisor = await _context.Users.FindAsync(request.SupervisorId.Value);
                    if (supervisor == null) return BadRequest("Supervisor not found");
                }

                var intern = new Intern
                {
                    UserId = request.UserId,
                    SupervisorId = request.SupervisorId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    University = request.University,
                    CourseOfStudy = request.CourseOfStudy,
                    Status = InternStatus.Active
                };

                _context.Interns.Add(intern);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetIntern), new { id = intern.Id }, intern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating intern");
                return StatusCode(500, "An error occurred while creating the intern");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIntern(int id, UpdateInternDto request)
        {
            try
            {
                var intern = await _context.Interns.FindAsync(id);
                if (intern == null) return NotFound();

                if (request.SupervisorId.HasValue)
                {
                    var supervisor = await _context.Users.FindAsync(request.SupervisorId.Value);
                    if (supervisor == null) return BadRequest("Supervisor not found");
                }

                intern.SupervisorId = request.SupervisorId;
                intern.StartDate = request.StartDate;
                intern.EndDate = request.EndDate;
                intern.University = request.University;
                intern.CourseOfStudy = request.CourseOfStudy;
                intern.Status = request.Status;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating intern with ID: {InternId}", id);
                return StatusCode(500, "An error occurred while updating the intern");
            }
        }

        [HttpGet("supervisor/{supervisorId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetInternsBySupervisor(Guid supervisorId)
        {
            try
            {
                var interns = await _context.Interns
                    .Where(i => i.SupervisorId == supervisorId)
                    .Include(i => i.User)
                    .Include(i => i.AbsenceRequests)
                    .Select(i => new
                    {
                        i.Id,
                        InternName = $"{i.User.FirstName} {i.User.LastName}",
                        i.User.Email,
                        i.University,
                        i.StartDate,
                        i.EndDate,
                        i.Status,
                        PendingAbsenceRequests = i.AbsenceRequests.Count(ar => ar.Status == AbsenceStatus.Pending),
                        TotalAbsenceDays = i.AbsenceRequests
                            .Where(ar => ar.Status == AbsenceStatus.Approved)
                            .Sum(ar => (ar.EndDate - ar.StartDate).Days + 1)
                    })
                    .ToListAsync();

                return Ok(interns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching interns for supervisor ID: {SupervisorId}", supervisorId);
                return StatusCode(500, "An error occurred while fetching interns");
            }
        }
    }

    public class CreateInternDto
    {
        public Guid UserId { get; set; }
        public Guid? SupervisorId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string University { get; set; }
        public string CourseOfStudy { get; set; }
    }

    public class UpdateInternDto
    {
        public Guid? SupervisorId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string University { get; set; }
        public string CourseOfStudy { get; set; }
        public InternStatus Status { get; set; }
    }
}
