using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Roles
{
    /// <summary>
    /// Remove a department-scoped role from a user.
    /// </summary>
    public record UnassignRoleDto(
        [Required] Guid UserId,
        [Range(1, int.MaxValue, ErrorMessage = "DepartmentId must be a positive integer.")] int DepartmentId,
        [Required, MaxLength(64, ErrorMessage = "RoleName max length is 64.")] string RoleName
    );
}
