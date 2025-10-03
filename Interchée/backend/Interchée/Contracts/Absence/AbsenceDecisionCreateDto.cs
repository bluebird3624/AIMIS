using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Absence
{
    public record AbsenceDecisionCreateDto(
        [Required] string Decision, // Approved|Rejected
        [MaxLength(500)] string? Comment
    );
}
