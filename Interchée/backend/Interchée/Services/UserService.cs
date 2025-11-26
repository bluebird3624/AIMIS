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
    public class UserService(UserManager<AppUser> users, AppDbContext db)
    {
        private readonly UserManager<AppUser> _users = users;
        private readonly AppDbContext _db = db;

        // --- helpers ---
        private static string ComposeDisplayName(string first, string last, string? userNameFallback)
        {
            var full = $"{(first ?? string.Empty).Trim()}  {(last ?? string.Empty).Trim()}".Trim();
            return string.IsNullOrWhiteSpace(full)
                ? userNameFallback ?? string.Empty
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
                u.roleName,
                u.departmentName,
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
                        u.MiddleName,
                        u.roleName,
                        u.departmentName
                    };

                var rows = await q.Distinct().ToListAsync();

                
            }

            // Otherwise, return a simple projection of all users.
            var userWithAssignmentQuery =
                 from u in _users.Users.AsNoTracking()
                 join a in _db.DepartmentRoleAssignments.AsNoTracking() on u.Id equals a.UserId into gj
                 from a in gj.DefaultIfEmpty()
                 select new
                 {
                     u.Id,
                     u.UserName,
                     u.Email,
                     u.IsActive,
                     u.FirstName,
                     u.LastName,
                     u.MiddleName,
                     DepartmentName = a == null ? null : a.Department != null ? a.Department.Name : null,
                     RoleName = a == null ? null : a.RoleName
                 };

            var rows1 = await userWithAssignmentQuery.ToListAsync();
            var userIds = rows1.Select(r => r.Id).Distinct().ToList();
            var roleLookup = await _db.Set<IdentityUserRole<Guid>>()
            .AsNoTracking()
            .Where(ur => userIds.Contains(ur.UserId))
            .Join(_db.Roles.AsNoTracking(), ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, RoleName = r.Name })
            .GroupBy(x => x.UserId)
            .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.RoleName!).Distinct().ToList());


            return rows1
                .Select(r => new UserSummaryDto(
                    r.Id,
                    r.UserName ?? string.Empty,
                    r.Email,
                    r.IsActive,
                    r.DepartmentName,
                    r.RoleName,
                    r.FirstName,
                    r.LastName,
                    r.MiddleName,
                    ComposeDisplayName(r.FirstName, r.LastName, r.UserName)
                    
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
                user.roleName ?? string.Empty,
                user.departmentName ?? string.Empty,
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
