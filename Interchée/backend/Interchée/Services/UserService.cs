using Interchée.Contracts.Users;
using Interchée.Data;
using Interchée.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Services
{
    /// <summary>
    /// User operations over ASP.NET Identity, plus department-role filtered queries.
    /// Returns enriched user summaries including structured names and a computed DisplayName.
    /// </summary>
    public class UserService
    {
        private readonly UserManager<AppUser> _users;
        private readonly AppDbContext _db;

        public UserService(UserManager<AppUser> users, AppDbContext db)
        {
            _users = users;
            _db = db;
        }

        // --- helpers ---
        private static string ComposeDisplayName(string first, string last, string? userNameFallback)
        {
            var full = $"{(first ?? string.Empty).Trim()} {(last ?? string.Empty).Trim()}".Trim();
            return string.IsNullOrWhiteSpace(full)
                ? (userNameFallback ?? string.Empty)
                : full;
        }

        /// <summary>
        /// Get one user summary by id, or null if not found.
        /// </summary>
        public async Task<UserSummaryDto?> GetAsync(Guid id)
        {
            var u = await _users.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (u is null) return null;

            var display = ComposeDisplayName(u.FirstName, u.LastName, u.UserName);
            return new UserSummaryDto(
                u.Id,
                u.UserName ?? string.Empty,
                u.Email,
                u.IsActive,
                u.FirstName,
                u.LastName,
                u.MiddleName,
                display
            );
        }

        /// <summary>
        /// List all users, or limit to those who have a specific department role.
        /// </summary>
        public async Task<List<UserSummaryDto>> GetAllAsync(int? departmentId = null, string? roleName = null)
        {
            // When departmentId+roleName are provided, join with DepartmentRoleAssignments for filtering.
            if (departmentId.HasValue && !string.IsNullOrWhiteSpace(roleName))
            {
                var q =
                    from a in _db.DepartmentRoleAssignments.AsNoTracking()
                    join u in _users.Users.AsNoTracking() on a.UserId equals u.Id
                    where a.DepartmentId == departmentId && a.RoleName == roleName
                    select new
                    {
                        u.Id,
                        u.UserName,
                        u.Email,
                        u.IsActive,
                        u.FirstName,
                        u.LastName,
                        u.MiddleName
                    };

                var rows = await q.Distinct().ToListAsync();

                return rows
                    .Select(r => new UserSummaryDto(
                        r.Id,
                        r.UserName ?? string.Empty,
                        r.Email,
                        r.IsActive,
                        r.FirstName,
                        r.LastName,
                        r.MiddleName,
                        ComposeDisplayName(r.FirstName, r.LastName, r.UserName)
                    ))
                    .ToList();
            }

            // Otherwise, return a simple projection of all users.
            var all = await _users.Users.AsNoTracking()
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.IsActive,
                    u.FirstName,
                    u.LastName,
                    u.MiddleName
                })
                .ToListAsync();

            return all
                .Select(u => new UserSummaryDto(
                    u.Id,
                    u.UserName ?? string.Empty,
                    u.Email,
                    u.IsActive,
                    u.FirstName,
                    u.LastName,
                    u.MiddleName,
                    ComposeDisplayName(u.FirstName, u.LastName, u.UserName)
                ))
                .ToList();
        }

        /// <summary>
        /// Create a new Identity user. Throws InvalidOperationException with error descriptions on failure.
        /// </summary>
        public async Task<UserSummaryDto> CreateAsync(CreateUserDto dto)
        {
            // Identity enforces unique email (RequireUniqueEmail=true).
            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = dto.UserName.Trim(),
                Email = dto.Email.Trim(),
                EmailConfirmed = true,
                IsActive = true,

                // structured names
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                MiddleName = string.IsNullOrWhiteSpace(dto.MiddleName) ? null : dto.MiddleName.Trim()
            };

            var result = await _users.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var msg = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(msg);
            }

            var display = ComposeDisplayName(user.FirstName, user.LastName, user.UserName);
            return new UserSummaryDto(
                user.Id,
                user.UserName ?? string.Empty,
                user.Email,
                user.IsActive,
                user.FirstName,
                user.LastName,
                user.MiddleName,
                display
            );
        }

        /// <summary>
        /// Toggle user IsActive. Throws KeyNotFoundException if not found.
        /// </summary>
        public async Task ToggleActiveAsync(Guid userId, bool isActive)
        {
            var u = await _users.FindByIdAsync(userId.ToString())
                ?? throw new KeyNotFoundException("User not found");

            u.IsActive = isActive;
            await _users.UpdateAsync(u);
        }
    }
}
