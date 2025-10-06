using Interchée.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Interchée.Auth
{
    /// <summary>
    /// Checks if the current user holds RoleName in the provided departmentId (resource).
    /// Usage: await _auth.AuthorizeAsync(User, departmentId, new DepartmentRoleRequirement("Instructor"))
    /// </summary>
    public class DepartmentRoleHandler : AuthorizationHandler<DepartmentRoleRequirement, int>
    {
        private readonly AppDbContext _db;
        public DepartmentRoleHandler(AppDbContext db) => _db = db;

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            DepartmentRoleRequirement requirement,
            int departmentId)
        {
            // Must be authenticated and have a user id
            var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr)) return;

            if (!Guid.TryParse(userIdStr, out var userId)) return;

            var hasRole = await _db.DepartmentRoleAssignments.AsNoTracking()
                .AnyAsync(a =>
                    a.UserId == userId &&
                    a.DepartmentId == departmentId &&
                    a.RoleName == requirement.RoleName);

            if (hasRole)
                context.Succeed(requirement);
        }
    }
}
