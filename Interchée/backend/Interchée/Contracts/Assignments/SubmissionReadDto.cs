namespace Interchée.Contracts.Assignments
{
    // Read submission
    public record SubmissionReadDto(
        long Id,
        long AssignmentId,
        Guid UserId,
        string? RepoUrl,
        string? Branch,
        string? LatestCommitSha,
        DateTime? SubmittedAt,
        string Status,
        DateTime CreatedAt,
        GradeReadDto? Grade,
        int CommitCount,
        int FeedbackCount
    );
}
