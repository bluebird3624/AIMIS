using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Assignments
{
    // Update submission status
    public record SubmissionStatusDto([Required] string Status);
}
