namespace Interchée.Contracts.Assignments
{
    // Read commit
    public record CommitReadDto(
        long Id,
        long SubmissionId,
        string Sha,
        string? Message,
        string? AuthorEmail,
        DateTime CommittedAt
    );
}
