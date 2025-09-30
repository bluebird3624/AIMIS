using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Departments
{
    public record DepartmentCreateDto(
    [property: Required, MaxLength(128)] string Name,
    [property: MaxLength(32)] string? Code
);
}
