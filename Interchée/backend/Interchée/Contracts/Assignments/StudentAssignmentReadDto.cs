using Interchée.Entities.Enums;

namespace Interchée.Contracts.Assignments
{
    public record StudentAssignmentReadDto(
        long Id,
        string Title,
        string? Description,
        int DepartmentId,
        string DepartmentName,
        DateTime? DueAt,
        AssignmentStatus Status,
        DateTime CreatedAt,
        DateTime AssignedAt,
        bool HasSubmission,
        SubmissionStatus SubmissionStatus, //  , "Submitted", "Reviewed"
        DateTime? SubmittedAt,
        bool IsGraded
    );
}