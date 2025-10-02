using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Departments
{
    /// <summary>Request to create a new Department.</summary>
    public record DepartmentCreateDto(
        [Required, MaxLength(128, ErrorMessage = "Name cannot exceed 128 characters.")]
        string Name,

        [MaxLength(32, ErrorMessage = "Code cannot exceed 32 characters.")]
        string? Code
    );
}
