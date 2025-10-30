namespace Interchée.Contracts.Assignments
{
    public record GradedSubmissionReadDto(
        long SubmissionId,
        long AssignmentId,
        string AssignmentTitle,
        Guid StudentId,
        string StudentName,
        decimal Score,
        decimal MaxScore,
        DateTime GradedAt,
        string Status // String status
    );
}