using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Assignments
{
    // Create/Update submission
    public record SubmissionCreateDto(
        [Required] long AssignmentId,  
        [Required][Url] string RepoUrl,
        string? Branch
    );
}
