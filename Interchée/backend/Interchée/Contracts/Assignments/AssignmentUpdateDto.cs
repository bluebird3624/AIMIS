using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Assignments
{
    // Update
    public record AssignmentUpdateDto(
        [Required][MaxLength(160)] string Title,
        string? Description,
        DateTime? DueAt
    );
}
