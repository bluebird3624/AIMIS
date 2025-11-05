using Interchée.Entities.Enums;

namespace Interchée.Contracts.Assignments
{
    // Read submission
    public record SubmissionReadDto(
        long Id,
        long AssignmentId,
        Guid UserId,
        SubmissionType SubmissionType, // "GitHub" or "File"
        string? RepoUrl,
        string? Branch,
        string? LatestCommitSha,
        DateTime? SubmittedAt,
        SubmissionStatus SubmissionStatus,
        DateTime CreatedAt,
        GradeReadDto? Grade,
        int CommitCount,
        int FeedbackCount,
        List<AttachmentReadDto> FileAttachments // Only for File submissions
    );
}
