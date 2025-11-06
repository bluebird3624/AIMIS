using Interchée.Entities.Enums;

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
        AssignmentStatus Status,
        DateTime CreatedAt,
        int AssigneeCount,
        int SubmissionCount
    );
}
