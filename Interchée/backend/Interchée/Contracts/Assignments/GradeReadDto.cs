namespace Interchée.Contracts.Assignments
{
    // Read grade
    public record GradeReadDto(
        long Id,
        long SubmissionId,
        decimal Score,
        decimal MaxScore,
         int? RubricId,           
        string? RubricName,
        string? RubricScoresJson,
        Guid GradedByUserId,
        DateTime GradedAt,
        string GradedByUserName,
        string? Comment
    );
}
