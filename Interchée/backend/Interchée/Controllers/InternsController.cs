using Interchée.Common;
using Interchée.Data;
using Interchée.Dtos;
using Interchée.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Controllers

{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InternsController(AppDbContext context, UserManager<AppUser> userManager, ILogger<InternsController> logger) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly ILogger<InternsController> _logger = logger;

        public string? AppUserIdString { get; private set; }

        [HttpGet]
        [Authorize(Roles = "HR,Admin,Supervisor")]
        public async Task<ActionResult<ApiResponse<List<InternDto>>>> GetInterns()
        {
            try
            {
                var interns = await _context.Interns
                    .Include(i => i.User)
                    .Include(i => i.Supervisor)
                    .Include(i => i.AbsenceRequests)
                    .Select(i => new InternDto
                    {
                        Id = i.Id,
                        UserId = i.UserId.ToString(),
                        UserName = i.User != null ? i.User.UserName ?? string.Empty : string.Empty,
                        Email = i.User != null ? i.User.Email ?? string.Empty : string.Empty,
                        FirstName = i.User != null ? i.User.FirstName ?? string.Empty : string.Empty,
                        LastName = i.User != null ? i.User.LastName ?? string.Empty : string.Empty,
                        FullName = i.User != null ? $"{(i.User.FirstName ?? string.Empty)} {(i.User.LastName ?? string.Empty)}".Trim() : "Unknown User",
                        DepartmentId = i.User != null ? i.User.DepartmentId : null,
                        DepartmentName = i.User != null && i.User.Department != null
                            ? GetDepartmentName(i.User.Department)
                            : "No Department",
                        SupervisorId = i.SupervisorId.HasValue ? i.SupervisorId.Value.ToString() : null,
                        SupervisorName = i.Supervisor != null ? $"{(i.Supervisor.FirstName ?? string.Empty)} {(i.Supervisor.LastName ?? string.Empty)}".Trim() : "No Supervisor",
                        StartDate = i.StartDate,
                        EndDate = i.EndDate,
                        University = i.University ?? "Not specified",
                        CourseOfStudy = i.CourseOfStudy ?? "Not specified",
                        Status = i.Status.ToString(),
                        TotalAbsenceRequests = i.AbsenceRequests.Count,
                        PendingAbsenceRequests = i.AbsenceRequests.Count(ar => ar.Status == AbsenceStatus.Pending),
                        ApprovedAbsenceDays = i.AbsenceRequests
                            .Where(ar => ar.Status == AbsenceStatus.Approved)
                            .Sum(ar => (ar.EndDate - ar.StartDate).Days + 1)
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<InternDto>>
                {
                    Success = true,
                    Data = interns
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching interns");
                return StatusCode(500, new ApiResponse<List<InternDto>>
                {
                    Success = false,
                    Errors = ["An error occurred while fetching interns"]
                });
            }
        }

        [HttpGet("me")]
        [Authorize(Roles = "Intern")]
        public async Task<ActionResult<ApiResponse<InternDto>>> GetMyInternProfile()
        {
            try
            {
                var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString))
                    return Unauthorized(new ApiResponse<InternDto>
                    {
                        Success = false,
                        Errors = ["Unauthorized"]
                    });

                if (!Guid.TryParse(userIdString, out Guid userId))
                    return Unauthorized(new ApiResponse<InternDto>
                    {
                        Success = false,
                        Errors = ["Invalid user ID"]
                    });

                var intern = await _context.Interns
                    .Include(i => i.User)
                    .Include(i => i.Supervisor)
                    .Include(i => i.AbsenceRequests)
                    .Where(i => i.UserId == userId)
                    .Select(i => new InternDto
                    {
                        Id = i.Id,
                        UserId = i.UserId.ToString(),
                        UserName = i.User != null ? i.User.UserName ?? string.Empty : string.Empty,
                        Email = i.User != null ? i.User.Email ?? string.Empty : string.Empty,
                        FirstName = i.User != null ? i.User.FirstName ?? string.Empty : string.Empty,
                        LastName = i.User != null ? i.User.LastName ?? string.Empty : string.Empty,
                        FullName = i.User != null ? $"{(i.User.FirstName ?? string.Empty)} {(i.User.LastName ?? string.Empty)}".Trim() : "Unknown User",
                        DepartmentId = i.User != null ? i.User.DepartmentId : null,
                        DepartmentName = i.User != null && i.User.Department != null
                            ? GetDepartmentName(i.User.Department)
                            : "No Department",
                        SupervisorId = i.SupervisorId.HasValue ? i.SupervisorId.Value.ToString() : null,
                        SupervisorName = i.Supervisor != null ? $"{(i.Supervisor.FirstName ?? string.Empty)} {(i.Supervisor.LastName ?? string.Empty)}".Trim() : "No Supervisor",
                        StartDate = i.StartDate,
                        EndDate = i.EndDate,
                        University = i.University ?? "Not specified",
                        CourseOfStudy = i.CourseOfStudy ?? "Not specified",
                        Status = i.Status.ToString(),
                        TotalAbsenceRequests = i.AbsenceRequests.Count,
                        PendingAbsenceRequests = i.AbsenceRequests.Count(ar => ar.Status == AbsenceStatus.Pending),
                        ApprovedAbsenceDays = i.AbsenceRequests
                            .Where(ar => ar.Status == AbsenceStatus.Approved)
                            .Sum(ar => (ar.EndDate - ar.StartDate).Days + 1)
                    })
                    .FirstOrDefaultAsync();

                if (intern == null)
                    return NotFound(new ApiResponse<InternDto>
                    {
                        Success = false,
                        Errors = ["Intern profile not found"]
                    });

                return Ok(new ApiResponse<InternDto>
                {
                    Success = true,
                    Data = intern
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching intern profile for user {UserId}", AppUserIdString);
                return StatusCode(500, new ApiResponse<InternDto>
                {
                    Success = false,
                    Errors = ["An error occurred while fetching intern profile"]        
                });
            }
        }

        // Add this helper method inside the InternsController class:
        private static string GetDepartmentName(object? department)
        {
            if (department == null)
                return "No Department";

            var nameProperty = department.GetType().GetProperty("Name");
            if (nameProperty != null)
            {
                var value = nameProperty.GetValue(department) as string;
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
            return "No Department";
        }

        // ... rest of the methods with similar null checks
    }
}