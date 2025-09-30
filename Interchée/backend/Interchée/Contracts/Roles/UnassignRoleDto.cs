using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Roles
{
    public record UnassignRoleDto(
    [property: Required] Guid UserId,
    [property: Range(1, int.MaxValue)] int DepartmentId,
    [property: Required, MaxLength(64)] string RoleName
);
}
