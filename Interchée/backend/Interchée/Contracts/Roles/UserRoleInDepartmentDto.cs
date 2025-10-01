namespace Interchée.Contracts.Roles
{
    /// <summary>
    /// Read model showing a user's role in a department.
    /// </summary>
    public record UserRoleInDepartmentDto(
        Guid UserId,
        int DepartmentId,
        string RoleName,
        DateTime AssignedAt
    );
}
