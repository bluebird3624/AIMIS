using Interchée.Common;
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

        public string? SupervisorIdString { get; private set; }
        public string? UserIdString { get; private set; }

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
                        UserId = i.UserId.ToString(),
                        UserName = i.User.UserName,
                        Email = i.User.Email,
                        FirstName = i.User.FirstName,
                        LastName = i.User.LastName,
                        FullName = $"{i.User.FirstName} {i.User.LastName}",
                        DepartmentId = i.User.DepartmentId,
                        DepartmentName = i.User.Department != null
                            ? i.User.Department.ToString()
                            : "No Department",
                        SupervisorId = i.SupervisorId.HasValue ? i.SupervisorId.Value.ToString() : null,
                        SupervisorName = i.Supervisor != null ? $"{i.Supervisor.FirstName} {i.Supervisor.LastName}" : "No Supervisor",
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
                    Errors = new List<string> { "An error occurred while fetching interns" }
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
                        Errors = new List<string> { "Unauthorized" }
                    });

                if (!Guid.TryParse(userIdString, out Guid userId))
                    return Unauthorized(new ApiResponse<InternDto>
                    {
                        Success = false,
                        Errors = new List<string> { "Invalid user ID" }
                    });

                var intern = await _context.Interns
                    .Include(i => i.User)
                    .ThenInclude(u => u.Department)
                    .Include(i => i.Supervisor)
                    .Include(i => i.AbsenceRequests)
                    .Where(i => i.UserId == userId)
                    .Select(i => new InternDto
                    {
                        Id = i.Id,
                        UserId = i.UserId.ToString(),
                        UserName = i.User.UserName,
                        Email = i.User.Email,
                        FirstName = i.User.FirstName,
                        LastName = i.User.LastName,
                        FullName = $"{i.User.FirstName} {i.User.LastName}",
                        DepartmentId = i.User.DepartmentId,
                        DepartmentName = i.User.Department != null
                            ? i.User.Department.ToString()
                            : "No Department",
                        SupervisorId = i.SupervisorId.HasValue ? i.SupervisorId.Value.ToString() : null,
                        SupervisorName = i.Supervisor != null ? $"{i.Supervisor.FirstName} {i.Supervisor.LastName}" : "No Supervisor",
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
                        Errors = new List<string> { "Intern profile not found" }
                    });

                return Ok(new ApiResponse<InternDto>
                {
                    Success = true,
                    Data = intern
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching intern profile for user {UserId}", UserIdString);
                return StatusCode(500, new ApiResponse<InternDto>
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while fetching intern profile" }
                });
            }
        }

        [HttpGet("supervised")]
        [Authorize(Roles = "Supervisor")]
        public async Task<ActionResult<ApiResponse<List<InternDto>>>> GetSupervisedInterns()
        {
            try
            {
                var supervisorIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(supervisorIdString))
                    return Unauthorized(new ApiResponse<List<InternDto>>
                    {
                        Success = false,
                        Errors = new List<string> { "Unauthorized" }
                    });

                if (!Guid.TryParse(supervisorIdString, out Guid supervisorId))
                    return Unauthorized(new ApiResponse<List<InternDto>>
                    {
                        Success = false,
                        Errors = new List<string> { "Invalid supervisor ID" }
                    });

                var interns = await _context.Interns
                    .Include(i => i.User)
                    .ThenInclude(u => u.Department)
                    .Include(i => i.AbsenceRequests)
                    .Where(i => i.SupervisorId == supervisorId)
                    .Select(i => new InternDto
                    {
                        Id = i.Id,
                        UserId = i.UserId.ToString(),
                        UserName = i.User.UserName,
                        Email = i.User.Email,
                        FirstName = i.User.FirstName,
                        LastName = i.User.LastName,
                        FullName = $"{i.User.FirstName} {i.User.LastName}",
                        DepartmentId = i.User.DepartmentId,
                        DepartmentName = i.User.Department != null
                            ? i.User.Department.ToString()
                            : "No Department",
                        SupervisorId = i.SupervisorId.HasValue ? i.SupervisorId.Value.ToString() : null,
                        SupervisorName = i.Supervisor != null ? $"{i.Supervisor.FirstName} {i.Supervisor.LastName}" : "No Supervisor",
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
                _logger.LogError(ex, "Error fetching supervised interns for supervisor {SupervisorId}", SupervisorIdString);
                return StatusCode(500, new ApiResponse<List<InternDto>>
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while fetching supervised interns" }
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "HR,Admin")]
        public async Task<ActionResult<ApiResponse<InternDto>>> CreateIntern(CreateInternDto request)
        {
            try
            {
                // Convert string to Guid for user lookup
                if (!Guid.TryParse(request.UserId, out Guid userId))
                    return BadRequest(new ApiResponse<InternDto>
                    {
                        Success = false,
                        Errors = new List<string> { "Invalid user ID" }
                    });

                // Check if user exists and is an intern
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                    return BadRequest(new ApiResponse<InternDto>
                    {
                        Success = false,
                        Errors = new List<string> { "User not found" }
                    });

                var isIntern = await _userManager.IsInRoleAsync(user, "Intern");
                if (!isIntern)
                    return BadRequest(new ApiResponse<InternDto>
                    {
                        Success = false,
                        Errors = new List<string> { "User is not assigned as an intern" }
                    });

                // Check if intern already exists for this user
                var existingIntern = await _context.Interns
                    .FirstOrDefaultAsync(i => i.UserId == userId);

                if (existingIntern != null)
                    return BadRequest(new ApiResponse<InternDto>
                    {
                        Success = false,
                        Errors = new List<string> { "Intern already exists for this user" }
                    });

                // Validate supervisor if provided
                Guid? supervisorId = null;
                if (!string.IsNullOrEmpty(request.SupervisorId))
                {
                    if (!Guid.TryParse(request.SupervisorId, out Guid parsedSupervisorId))
                        return BadRequest(new ApiResponse<InternDto>
                        {
                            Success = false,
                            Errors = new List<string> { "Invalid supervisor ID" }
                        });

                    var supervisor = await _userManager.FindByIdAsync(request.SupervisorId);
                    if (supervisor == null || !await _userManager.IsInRoleAsync(supervisor, "Supervisor"))
                        return BadRequest(new ApiResponse<InternDto>
                        {
                            Success = false,
                            Errors = new List<string> { "Supervisor not found or not a supervisor" }
                        });

                    supervisorId = parsedSupervisorId;
                }

                var intern = new Intern
                {
                    UserId = userId,
                    SupervisorId = supervisorId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    University = request.University,
                    CourseOfStudy = request.CourseOfStudy,
                    Status = InternStatus.Active
                };

                _context.Interns.Add(intern);
                await _context.SaveChangesAsync();

                // Get the created intern with related data
                var createdIntern = await _context.Interns
                    .Include(i => i.User)
                    .ThenInclude(u => u.Department)
                    .Include(i => i.Supervisor)
                    .FirstOrDefaultAsync(i => i.Id == intern.Id);

                var internDto = await MapToInternDto(createdIntern);
                return Ok(new ApiResponse<InternDto>
                {
                    Success = true,
                    Data = internDto,
                    Message = "Intern created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating intern");
                return StatusCode(500, new ApiResponse<InternDto>
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while creating intern" }
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "HR,Admin")]
        public async Task<ActionResult<ApiResponse>> UpdateIntern(int id, UpdateInternDto request)
        {
            try
            {
                var intern = await _context.Interns
                    .Include(i => i.User)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (intern == null)
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string> { "Intern not found" }
                    });

                // Validate supervisor if provided
                Guid? supervisorId = null;
                if (!string.IsNullOrEmpty(request.SupervisorId))
                {
                    if (!Guid.TryParse(request.SupervisorId, out Guid parsedSupervisorId))
                        return BadRequest(new ApiResponse
                        {
                            Success = false,
                            Errors = new List<string> { "Invalid supervisor ID" }
                        });

                    var supervisor = await _userManager.FindByIdAsync(request.SupervisorId);
                    if (supervisor == null || !await _userManager.IsInRoleAsync(supervisor, "Supervisor"))
                        return BadRequest(new ApiResponse
                        {
                            Success = false,
                            Errors = new List<string> { "Supervisor not found or not a supervisor" }
                        });

                    supervisorId = parsedSupervisorId;
                }

                intern.SupervisorId = supervisorId;
                intern.StartDate = request.StartDate;
                intern.EndDate = request.EndDate;
                intern.University = request.University;
                intern.CourseOfStudy = request.CourseOfStudy;
                intern.Status = request.Status;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Intern updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating intern {InternId}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while updating intern" }
                });
            }
        }

        [HttpPut("{id}/supervisor")]
        [Authorize(Roles = "HR,Admin")]
        public async Task<ActionResult<ApiResponse>> UpdateInternSupervisor(int id, UpdateInternSupervisorDto request)
        {
            try
            {
                var intern = await _context.Interns.FindAsync(id);
                if (intern == null)
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string> { "Intern not found" }
                    });

                // Validate supervisor
                Guid? supervisorId = null;
                if (!string.IsNullOrEmpty(request.SupervisorId))
                {
                    if (!Guid.TryParse(request.SupervisorId, out Guid parsedSupervisorId))
                        return BadRequest(new ApiResponse
                        {
                            Success = false,
                            Errors = new List<string> { "Invalid supervisor ID" }
                        });

                    var supervisor = await _userManager.FindByIdAsync(request.SupervisorId);
                    if (supervisor == null || !await _userManager.IsInRoleAsync(supervisor, "Supervisor"))
                        return BadRequest(new ApiResponse
                        {
                            Success = false,
                            Errors = new List<string> { "Supervisor not found or not a supervisor" }
                        });

                    supervisorId = parsedSupervisorId;
                }

                intern.SupervisorId = supervisorId;
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Intern supervisor updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supervisor for intern {InternId}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while updating intern supervisor" }
                });
            }
        }

        private async Task<InternDto> MapToInternDto(Intern intern)
        {
            // Fix for CS8634: Ensure intern.User is not null before calling Entry
            if (intern.User == null)
                throw new InvalidOperationException("Intern.User is null.");

            // Fix for CS0019: Remove invalid null-coalescing operator and string fallback
            await _context.Entry(intern.User).Reference(u => u.Department).LoadAsync();

            return new InternDto
            {
                Id = intern.Id,
                UserId = intern.UserId.ToString(),
                UserName = intern.User.UserName,
                Email = intern.User.Email,
                FirstName = intern.User.FirstName,
                LastName = intern.User.LastName,
                FullName = $"{intern.User.FirstName} {intern.User.LastName}",
                DepartmentId = intern.User.DepartmentId,
                DepartmentName = intern.User.Department != null
                    ? intern.User.Department.ToString()
                    : "No Department",
                SupervisorId = intern.SupervisorId.HasValue ? intern.SupervisorId.Value.ToString() : null,
                SupervisorName = intern.Supervisor != null ? $"{intern.Supervisor.FirstName} {intern.Supervisor.LastName}" : "No Supervisor",
                StartDate = intern.StartDate,
                EndDate = intern.EndDate,
                University = intern.University ?? "Not specified",
                CourseOfStudy = intern.CourseOfStudy ?? "Not specified",
                Status = intern.Status.ToString()
            };
        }
    }
}