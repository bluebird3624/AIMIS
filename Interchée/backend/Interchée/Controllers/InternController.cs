using Interchee.Common;
using Interchée.Data;
using Interchée.Dtos;
using Interchée.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Interchee.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InternsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<InternsController> _logger;

        public InternsController(AppDbContext context, UserManager<AppUser> userManager, ILogger<InternsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "HR,Admin,Supervisor")]
        public async Task<ActionResult<ApiResponse<List<InternDto>>>> GetInterns()
        {
            try
            {
                var interns = await _context.Interns
                    .Include(i => i.User)
                    .ThenInclude(u => u.Department)
                    .Include(i => i.Supervisor)
                    .Include(i => i.AbsenceRequests)
                    .Select(i => new InternDto
                    {
                        Id = i.Id,
                        UserId = i.UserId,
                        UserName = i.User.UserName,
                        Email = i.User.Email,
                        FirstName = i.User.FirstName,
                        LastName = i.User.LastName,
                        FullName = i.User.FullName,
                        DepartmentId = i.User.DepartmentId,
                        DepartmentName = i.User.Department != null ? i.User.Department.Name : null,
                        SupervisorId = i.SupervisorId,
                        SupervisorName = i.Supervisor != null ? i.Supervisor.FullName : null,
                        StartDate = i.StartDate,
                        EndDate = i.EndDate,
                        University = i.University,
                        CourseOfStudy = i.CourseOfStudy,
                        Status = i.Status.ToString(),
                        TotalAbsenceRequests = i.AbsenceRequests.Count,
                        PendingAbsenceRequests = i.AbsenceRequests.Count(ar => ar.Status == AbsenceStatus.Pending),
                        ApprovedAbsenceDays = i.AbsenceRequests
                            .Where(ar => ar.Status == AbsenceStatus.Approved)
                            .Sum(ar => ar.TotalDays)
                    })
                    .ToListAsync();

                return Ok(ApiResponse.SuccessResponse(interns));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching interns");
                return StatusCode(500, ApiResponse.Errors("An error occurred while fetching interns"));
            }
        }

        [HttpGet("me")]
        [Authorize(Roles = "Intern")]
        public async Task<ActionResult<ApiResponse<InternDto>>> GetMyInternProfile()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse.Errors("Unauthorized"));

                var intern = await _context.Interns
                    .Include(i => i.User)
                    .ThenInclude(u => u.Department)
                    .Include(i => i.Supervisor)
                    .Include(i => i.AbsenceRequests)
                    .Where(i => i.UserId == userId)
                    .Select(i => new InternDto
                    {
                        Id = i.Id,
                        UserId = i.UserId,
                        UserName = i.User.UserName,
                        Email = i.User.Email,
                        FirstName = i.User.FirstName,
                        LastName = i.User.LastName,
                        FullName = i.User.FullName,
                        DepartmentId = i.User.DepartmentId,
                        DepartmentName = i.User.Department != null ? i.User.Department.Name : null,
                        SupervisorId = i.SupervisorId,
                        SupervisorName = i.Supervisor != null ? i.Supervisor.FullName : null,
                        StartDate = i.StartDate,
                        EndDate = i.EndDate,
                        University = i.University,
                        CourseOfStudy = i.CourseOfStudy,
                        Status = i.Status.ToString(),
                        TotalAbsenceRequests = i.AbsenceRequests.Count,
                        PendingAbsenceRequests = i.AbsenceRequests.Count(ar => ar.Status == AbsenceStatus.Pending),
                        ApprovedAbsenceDays = i.AbsenceRequests
                            .Where(ar => ar.Status == AbsenceStatus.Approved)
                            .Sum(ar => ar.TotalDays)
                    })
                    .FirstOrDefaultAsync();

                if (intern == null)
                    return NotFound(ApiResponse.Errors("Intern profile not found"));

                return Ok(ApiResponse.SuccessResponse(intern));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching intern profile for user {UserId}", userId);
                return StatusCode(500, ApiResponse.Errors("An error occurred while fetching intern profile"));
            }
        }

        // ... rest of the methods remain the same, just update the return types to use ApiResponse from Common namespace
        // [HttpPost], [HttpPut], [HttpGet("supervised")] etc.
    }

    // ... DTO classes remain the same
}