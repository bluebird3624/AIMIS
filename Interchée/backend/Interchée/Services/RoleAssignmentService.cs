using Interchée.Contracts.Roles;
using Interchée.Data;
using Interchée.Entities;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Services
{
    /// <summary>
    /// Manages department-scoped role assignments (User ↔ RoleName ↔ Department).
    /// Enforces uniqueness and provides simple querying.
    /// </summary>
    public class RoleAssignmentService
    {
        private readonly AppDbContext _db;

        public RoleAssignmentService(AppDbContext db) => _db = db;

        /// <summary>
        /// Assigns a role in a department to a user.
        /// Throws InvalidOperationException with a friendly message on duplicates or invalid refs.
        /// </summary>
        public async Task<UserRoleInDepartmentDto> AssignAsync(AssignRoleDto dto)
        {
            // Validate referenced entities exist (nice errors before DB constraint failures)
            var userExists = await _db.Users.AsNoTracking().AnyAsync(u => u.Id == dto.UserId);
            if (!userExists) throw new InvalidOperationException("User does not exist.");

            var deptExists = await _db.Departments.AsNoTracking().AnyAsync(d => d.Id == dto.DepartmentId);
            if (!deptExists) throw new InvalidOperationException("Department does not exist.");

            // Enforce uniqueness (also guaranteed by unique index).
            var already = await _db.DepartmentRoleAssignments.AsNoTracking()
                .AnyAsync(a => a.UserId == dto.UserId && a.DepartmentId == dto.DepartmentId && a.RoleName == dto.RoleName);
            if (already) throw new InvalidOperationException("Role is already assigned for this user in the department.");

            var entity = new DepartmentRoleAssignment
            {
                UserId = dto.UserId,
                DepartmentId = dto.DepartmentId,
                RoleName = dto.RoleName,
                AssignedAt = DateTime.UtcNow
            };

            _db.DepartmentRoleAssignments.Add(entity);
            await _db.SaveChangesAsync();

            return new UserRoleInDepartmentDto(entity.UserId, entity.DepartmentId, entity.RoleName, entity.AssignedAt);
        }

        /// <summary>
        /// Removes a role in a department from a user. Idempotent (no error if not found).
        /// </summary>
        public async Task UnassignAsync(UnassignRoleDto dto)
        {
            var entity = await _db.DepartmentRoleAssignments
                .FirstOrDefaultAsync(a => a.UserId == dto.UserId && a.DepartmentId == dto.DepartmentId && a.RoleName == dto.RoleName);

            if (entity == null) return;

            _db.DepartmentRoleAssignments.Remove(entity);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Lists all department roles held by a specific user.
        /// </summary>
        public async Task<List<UserRoleInDepartmentDto>> GetUserRolesAsync(Guid userId)
        {
            return await _db.DepartmentRoleAssignments.AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AssignedAt)
                .Select(a => new UserRoleInDepartmentDto(a.UserId, a.DepartmentId, a.RoleName, a.AssignedAt))
                .ToListAsync();
        }
    }
}
