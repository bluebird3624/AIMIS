namespace Interchée.Contracts.Assignments
{
    public record GradeSummaryDto(
        int TotalSubmissions,
        int GradedSubmissions,
        double AverageScore,
        double HighestScore,
        double LowestScore,
        int PassedCount,
        int FailedCount
    );
}