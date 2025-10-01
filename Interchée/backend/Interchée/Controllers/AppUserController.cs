using Interchée.Common;
using Interchée.Data;
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
    public class UsersController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(UserManager<AppUser> userManager, AppDbContext context, ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "HR,Admin")]
        public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetUsers()
        {
            try
            {
                var users = await _userManager.Users
                    .Include(u => u.Department)
                    .Select(u => new UserDto
                    {
                        Id = u.Id.ToString(), // Convert Guid to string
                        UserName = u.UserName,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        FullName = $"{u.FirstName} {u.LastName}",
                        DepartmentId = u.DepartmentId,
                        DepartmentName = u.Department != null ? ((Department)u.Department).DepartmentName : "No Department",
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

                return Ok(new ApiResponse<List<UserDto>>
                {
                    Success = true,
                    Data = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users");
                return StatusCode(500, new ApiResponse<List<UserDto>>
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while fetching users" }
                });
            }
        }

        [HttpGet("supervisors")]
        [Authorize(Roles = "HR,Admin,Supervisor")]
        public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetSupervisors()
        {
            try
            {
                var supervisors = await _userManager.GetUsersInRoleAsync("Supervisor");
                var supervisorDtos = supervisors.Select(u => new UserDto
                {
                    Id = u.Id.ToString(), // Convert Guid to string
                    UserName = u.UserName,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    FullName = $"{u.FirstName} {u.LastName}",
                    DepartmentId = u.DepartmentId,
                    DepartmentName = u.Department is Department dept ? dept.DepartmentName : "No Department",
                    IsActive = u.IsActive
                }).ToList();

                return Ok(new ApiResponse<List<UserDto>>
                {
                    Success = true,
                    Data = supervisorDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching supervisors");
                return StatusCode(500, new ApiResponse<List<UserDto>>
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while fetching supervisors" }
                });
            }
        }

        [HttpGet("interns")]
        [Authorize(Roles = "HR,Admin,Supervisor")]
        public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetInterns()
        {
            try
            {
                var interns = await _userManager.GetUsersInRoleAsync("Intern");
                var internDtos = interns.Select(u => new UserDto
                {
                    Id = u.Id.ToString(), // Convert Guid to string
                    UserName = u.UserName,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    FullName = $"{u.FirstName} {u.LastName}",
                    DepartmentId = u.DepartmentId,
                    DepartmentName = u.Department is Department dept ? dept.DepartmentName : "No Department",
                    IsActive = u.IsActive
                }).ToList();

                return Ok(new ApiResponse<List<UserDto>>
                {
                    Success = true,
                    Data = internDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching interns");
                return StatusCode(500, new ApiResponse<List<UserDto>>
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while fetching interns" }
                });
            }
        }

        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
        {
            try
            {
                var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString))
                    return Unauthorized(new ApiResponse<UserDto>
                    {
                        Success = false,
                        Errors = new List<string> { "Unauthorized" }
                    });

                if (!Guid.TryParse(userIdString, out Guid userId))
                    return Unauthorized(new ApiResponse<UserDto>
                    {
                        Success = false,
                        Errors = new List<string> { "Invalid user ID" }
                    });

                var user = await _userManager.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return NotFound(new ApiResponse<UserDto>
                    {
                        Success = false,
                        Errors = new List<string> { "User not found" }
                    });

                var userDto = new UserDto
                {
                    Id = user.Id.ToString(), // Convert Guid to string
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = $"{user.FirstName} {user.LastName}",
                    DepartmentId = user.DepartmentId,
                    DepartmentName = user.Department is Department dept ? dept.DepartmentName : "No Department",
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    Roles = await _userManager.GetRolesAsync(user)
                };

                return Ok(new ApiResponse<UserDto>
                {
                    Success = true,
                    Data = userDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching current user");
                return StatusCode(500, new ApiResponse<UserDto>
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while fetching user information" }
                });
            }
        }

        [HttpPut("{id}/department")]
        [Authorize(Roles = "HR,Admin")]
        public async Task<ActionResult<ApiResponse>> UpdateUserDepartment(string id, UpdateUserDepartmentDto request)
        {
            try
            {
                if (!Guid.TryParse(id, out Guid userId))
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string> { "Invalid user ID" }
                    });

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string> { "User not found" }
                    });

                var department = await _context.Departments.FindAsync(request.DepartmentId);
                if (department == null)
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string> { "Department not found" }
                    });

                user.DepartmentId = request.DepartmentId;
                await _userManager.UpdateAsync(user);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "User department updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user department for user {UserId}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while updating user department" }
                });
            }
        }

        [HttpPut("{id}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse>> UpdateUserRoles(string id, UpdateUserRolesDto request)
        {
            try
            {
                if (!Guid.TryParse(id, out Guid userId))
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string> { "Invalid user ID" }
                    });

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string> { "User not found" }
                    });

                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                var result = await _userManager.AddToRolesAsync(user, request.Roles);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Errors = errors
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "User roles updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user roles for user {UserId}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while updating user roles" }
                });
            }
        }
    }

    public class UserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }

    public class UpdateUserDepartmentDto
    {
        public int DepartmentId { get; set; }
    }

    public class UpdateUserRolesDto
    {
        public List<string> Roles { get; set; } = new List<string>();
    }
}