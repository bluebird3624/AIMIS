using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Absence
{
    public record AbsenceRequestCreateDto(
        [Required] DateOnly StartDate,
        [Required] DateOnly EndDate,
        [Required, MaxLength(500)] string Reason
    );
}
