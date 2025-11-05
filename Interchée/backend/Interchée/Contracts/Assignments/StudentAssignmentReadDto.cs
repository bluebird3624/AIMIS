namespace Interchée.Contracts.Assignments
{
    public record StudentAssignmentReadDto(
        long Id,
        string Title,
        string? Description,
        int DepartmentId,
        string DepartmentName,
        DateTime? DueAt,
        string Status,
        DateTime CreatedAt,
        DateTime AssignedAt,
        bool HasSubmission,
        string SubmissionStatus, //  "InProgress", "Submitted", "Reviewed"
        DateTime? SubmittedAt,
        bool IsGraded
    );
}