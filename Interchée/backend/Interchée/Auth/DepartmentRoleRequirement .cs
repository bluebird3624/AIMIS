using Microsoft.AspNetCore.Authorization;

namespace Interchée.Auth
{
    /// <summary>
    /// "User must have RoleName in a specific Department (the resource = departmentId)"
    /// </summary>
    public class DepartmentRoleRequirement : IAuthorizationRequirement
    {
        public string RoleName { get; }
        public DepartmentRoleRequirement(string roleName) => RoleName = roleName;
    }
}
