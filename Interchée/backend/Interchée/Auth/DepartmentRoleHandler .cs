using Interchée.Auth;
using Interchée.Config; // where Roles.Admin lives
using Interchée.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InternAttache.Api.Auth
    /// <summary>
    /// Checks if the current user holds RoleName in the provided departmentId (resource).
    /// Usage: await _auth.AuthorizeAsync(User, departmentId, new DepartmentRoleRequirement("Instructor"))
    /// </summary>
    public sealed class DepartmentRoleHandler
        : AuthorizationHandler<DepartmentRoleRequirement, int>
    {
        private readonly AppDbContext _db;
        public DepartmentRoleHandler(AppDbContext db) => _db = db;

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            DepartmentRoleRequirement requirement,
            int departmentId)
        {
            // 0) If caller is a **global Admin**, grant access immediately.
            if (context.User.IsInRole(Roles.Admin))
            {
                context.Succeed(requirement);
                return;
            }

            // 1) Must be authenticated
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                context.Fail();
                return;
            }

            // 2) Check department-scoped role in DB
            var hasRole = await _db.DepartmentRoleAssignments.AsNoTracking()
                .AnyAsync(a => a.UserId.ToString() == userId
                            && a.DepartmentId == departmentId
                            && a.RoleName == requirement.RoleName);

            if (hasRole) context.Succeed(requirement);
            else context.Fail();
        }
    }
}
