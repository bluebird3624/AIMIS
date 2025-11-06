using Interchée.Entities.Enums;

namespace Interchée.Contracts.Assignments
{
    public record StudentGradeReadDto(
        long SubmissionId,
        long AssignmentId,
        string AssignmentTitle,
        decimal Score,
        decimal MaxScore,
        List<CriteriaScoreDto>? Breakdown,
        Guid GradedByUserId,
        string GradedByUserName,
        DateTime GradedAt,
        SubmissionStatus Status,
        string? Comment
    );
}