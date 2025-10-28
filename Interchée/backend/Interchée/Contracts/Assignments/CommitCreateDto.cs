using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Assignments
{
    // Add commit to submission
    public record CommitCreateDto(
        [Required] string Sha,
        string? Message,
        string? AuthorEmail,
        [Required] DateTime CommittedAt
    );

}
