using Interchee.Common;
using Interchée.Data;
using Interchee.Entities;
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
                        Id = u.Id,
                        UserName = u.UserName,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        FullName = u.FullName,
                        DepartmentId = u.DepartmentId,
                        DepartmentName = u.Department != null ? u.Department.Name : null,
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt
                    })
                    .ToListAsync();

                // Get roles for each user
                foreach (var user in users)
                {
                    var appUser = await _userManager.FindByIdAsync(user.Id);
                    user.Roles = await _userManager.GetRolesAsync(appUser);
                }

                return Ok(ApiResponse.Success(users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users");
                return StatusCode(500, ApiResponse.Error<List<UserDto>>("An error occurred while fetching users"));
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
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    FullName = u.FullName,
                    DepartmentId = u.DepartmentId,
                    DepartmentName = u.Department != null ? u.Department.Name : null,
                    IsActive = u.IsActive
                }).ToList();

                return Ok(ApiResponse.Success(supervisorDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching supervisors");
                return StatusCode(500, ApiResponse.Error<List<UserDto>>("An error occurred while fetching supervisors"));
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
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    FullName = u.FullName,
                    DepartmentId = u.DepartmentId,
                    DepartmentName = u.Department != null ? u.Department.Name : null,
                    IsActive = u.IsActive
                }).ToList();

                return Ok(ApiResponse.Success(internDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching interns");
                return StatusCode(500, ApiResponse.Error<List<UserDto>>("An error occurred while fetching interns"));
            }
        }

        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse.Error<UserDto>("Unauthorized"));

                var user = await _userManager.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return NotFound(ApiResponse.Error<UserDto>("User not found"));

                var userDto = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    DepartmentId = user.DepartmentId,
                    DepartmentName = user.Department != null ? user.Department.Name : null,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    Roles = await _userManager.GetRolesAsync(user)
                };

                return Ok(ApiResponse.Success(userDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching current user");
                return StatusCode(500, ApiResponse.Error<UserDto>("An error occurred while fetching user information"));
            }
        }

        [HttpPut("{id}/department")]
        [Authorize(Roles = "HR,Admin")]
        public async Task<ActionResult<ApiResponse>> UpdateUserDepartment(string id, UpdateUserDepartmentDto request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound(ApiResponse.Error("User not found"));

                var department = await _context.Departments.FindAsync(request.DepartmentId);
                if (department == null)
                    return BadRequest(ApiResponse.Error("Department not found"));

                user.DepartmentId = request.DepartmentId;
                await _userManager.UpdateAsync(user);

                return Ok(ApiResponse.Success("User department updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user department for user {UserId}", id);
                return StatusCode(500, ApiResponse.Error("An error occurred while updating user department"));
            }
        }

        [HttpPut("{id}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse>> UpdateUserRoles(string id, UpdateUserRolesDto request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound(ApiResponse.Error("User not found"));

                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                var result = await _userManager.AddToRolesAsync(user, request.Roles);
                if (!result.Succeeded)
                    return BadRequest(ApiResponse.Error("Failed to update user roles"));

                return Ok(ApiResponse.Success("User roles updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user roles for user {UserId}", id);
                return StatusCode(500, ApiResponse.Error("An error occurred while updating user roles"));
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
        public List<string> Roles { get; set; } = new();
    }
}