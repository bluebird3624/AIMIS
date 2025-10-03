using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Absence
{
    public record AbsenceRequestStatusDto(
        [Required] string Status // Pending|Approved|Rejected|Cancelled
    );
}
