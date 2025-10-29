namespace Interchée.Contracts.Assignments
{
    // Read
    public record AssignmentReadDto(
        long Id,
        string Title,
        string? Description,
        int DepartmentId,
        Guid CreatedByUserId,
        DateTime? DueAt,
        string Status,
        DateTime CreatedAt,
        int AssigneeCount,
        int SubmissionCount
    );
}
