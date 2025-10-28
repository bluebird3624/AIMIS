namespace Interchée.Contracts.Departments
{
    /// <summary>
    /// Read model returned to clients.
    /// </summary>
    public record DepartmentReadDto(
        int Id,
        string Name,
        string? Code,
        bool IsActive
    );
}
