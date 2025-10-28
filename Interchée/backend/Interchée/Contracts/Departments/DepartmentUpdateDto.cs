using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Departments
{
    public record DepartmentUpdateDto(
        [Required, MaxLength(128)]
        string Name,
        [MaxLength(32)]
        string? Code
    );
}
