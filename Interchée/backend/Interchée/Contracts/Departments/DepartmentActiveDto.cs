using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Departments
{
    public record DepartmentActiveDto(
        [Required]
        bool IsActive
    );
}
