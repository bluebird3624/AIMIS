using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Assignments
{
    // Create
    public record AssignmentCreateDto(
        [Required][MaxLength(160)] string Title,
        string? Description,
        int DepartmentId,
        DateTime? DueAt
    );

}