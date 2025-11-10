namespace Interchée.Contracts.Assignments
{
    // Read comment
    public record AssignmentFeedbackReadDto(
        long Id,
        long SubmissionId,
        Guid AuthorUserId,
        string Comment,
        DateTime CreatedAt,
        string AuthorUserName,
        string AuthorDisplayName
    );
}
