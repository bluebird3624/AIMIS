namespace Interchée.Contracts.Assignments
{
    // Read grade
    public record GradeReadDto(
        long Id,
        long SubmissionId,
        decimal Score,
        decimal MaxScore,
        string? RubricJson,
        Guid GradedByUserId,
        DateTime GradedAt,
        string GradedByUserName
    );
}
