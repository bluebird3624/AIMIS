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
    public class AppUsersController(UserManager<AppUser> userManager, AppDbContext context, ILogger<AppUsersController> logger) : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly AppDbContext _context = context;
        private readonly ILogger<AppUsersController> _logger = logger;

        [HttpGet]
        [Authorize(Roles = "HR,Admin")]
        public async Task<ActionResult<ApiResponse<List<AppUserDto>>>> GetUsers()
        {
            try
            {
                var users = await _userManager.Users
                    .Include(u => u.Department)
                    .Select(u => new AppUserDto
                    {
                        Id = u.Id.ToString(),
                        UserName = u.UserName ?? string.Empty,
                        Email = u.Email ?? string.Empty,
                        FirstName = u.FirstName ?? string.Empty,
                        LastName = u.LastName ?? string.Empty,
                        DepartmentId = u.DepartmentId,
                        DepartmentName = u.Department != null ? u.Department.ToString() : "No Department",
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt
                    })
                    .ToListAsync();

                // Get roles for each user
                foreach (var user in users)
                {
                    if (Guid.TryParse(user.Id, out Guid userId))
                    {
                        var appUser = await _userManager.FindByIdAsync(userId.ToString());
                        if (appUser != null)
                        {
                            user.Roles = await _userManager.GetRolesAsync(appUser);
                        }
                    }
                }

                return Ok(new ApiResponse<List<AppUserDto>>
                {
                    Success = true,
                    Data = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users");
                return StatusCode(500, new ApiResponse<List<AppUserDto>>
                {
                    Success = false,
                    Errors = ["An error occurred while fetching users"]
                });
            }
        }

        [HttpGet("supervisors")]
        [Authorize(Roles = "HR,Admin,Supervisor")]
        public async Task<ActionResult<ApiResponse<List<AppUserDto>>>> GetSupervisors()
        {
            try
            {
                var supervisors = await _userManager.GetUsersInRoleAsync("Supervisor");
                var supervisorDtos = new List<AppUserDto>();

                foreach (var supervisor in supervisors)
                {
                    if (supervisor != null)
                    {
                        // Load department separately to avoid null reference
                        await _context.Entry(supervisor)
                            .Reference(u => u.Department)
                            .LoadAsync();

                        var supervisorDto = new AppUserDto
                        {
                            Id = supervisor.Id.ToString(),
                            UserName = supervisor.UserName ?? string.Empty,
                            Email = supervisor.Email ?? string.Empty,
                            FirstName = supervisor.FirstName ?? string.Empty,
                            LastName = supervisor.LastName ?? string.Empty,
                            DepartmentId = supervisor.DepartmentId,
                            DepartmentName = supervisor.Department != null ? supervisor.Department.ToString() : "No Department",
                            IsActive = supervisor.IsActive
                        };

                        supervisorDtos.Add(supervisorDto);
                    }
                }

                return Ok(new ApiResponse<List<AppUserDto>>
                {
                    Success = true,
                    Data = supervisorDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching supervisors");
                return StatusCode(500, new ApiResponse<List<AppUserDto>>
                {
                    Success = false,
                    Errors = ["An error occurred while fetching supervisors"]
                });
            }
        }

        [HttpGet("interns")]
        [Authorize(Roles = "HR,Admin,Supervisor")]
        public async Task<ActionResult<ApiResponse<List<AppUserDto>>>> GetInterns()
        {
            try
            {
                var interns = await _userManager.GetUsersInRoleAsync("Intern");
                var internDtos = new List<AppUserDto>();

                foreach (var intern in interns)
                {
                    if (intern != null)
                    {
                        // Load department separately to avoid null reference
                        await _context.Entry(intern)
                            .Reference(u => u.Department)
                            .LoadAsync();

                        var internDto = new AppUserDto
                        {
                            Id = intern.Id.ToString(),
                            UserName = intern.UserName ?? string.Empty,
                            Email = intern.Email ?? string.Empty,
                            FirstName = intern.FirstName ?? string.Empty,
                            LastName = intern.LastName ?? string.Empty,
                            DepartmentId = intern.DepartmentId,
                            DepartmentName = intern.Department != null ? intern.Department.ToString() : "No Department",
                            IsActive = intern.IsActive
                        };

                        internDtos.Add(internDto);
                    }
                }

                return Ok(new ApiResponse<List<AppUserDto>>
                {
                    Success = true,
                    Data = internDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching interns");
                return StatusCode(500, new ApiResponse<List<AppUserDto>>
                {
                    Success = false,
                    Errors = ["An error occurred while fetching interns"]
                });
            }
        }

        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<AppUserDto>>> GetCurrentUser()
        {
            try
            {
                var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString))
                    return Unauthorized(new ApiResponse<AppUserDto>
                    {
                        Success = false,
                        Errors = ["Unauthorized"]
                    });

                if (!Guid.TryParse(userIdString, out Guid userId))
                    return Unauthorized(new ApiResponse<AppUserDto>
                    {
                        Success = false,
                        Errors = ["Invalid user ID"]
                    });

                var user = await _userManager.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return NotFound(new ApiResponse<AppUserDto>
                    {
                        Success = false,
                        Errors = ["User not found"]
                    });

                var userDto = new AppUserDto
                {
                    Id = user.Id.ToString(),
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    DepartmentId = user.DepartmentId,
                    DepartmentName = user.Department != null ? user.Department.ToString() : "No Department",
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    Roles = await _userManager.GetRolesAsync(user)
                };

                return Ok(new ApiResponse<AppUserDto>
                {
                    Success = true,
                    Data = userDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching current user");
                return StatusCode(500, new ApiResponse<AppUserDto>
                {
                    Success = false,
                    Errors = ["An error occurred while fetching user information"]
                });
            }
        }

        // ... rest of the methods remain the same
    }
}