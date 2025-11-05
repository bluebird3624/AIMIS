using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Assignments
{
    // Create/Update grade
    public record GradeCreateDto(
        [Required][Range(0, 99.99)] decimal Score,
        [Required][Range(0, 100.00)] decimal MaxScore,

        int? RubricId, // Use rubric instead of manual JSON
        Dictionary<string, decimal>? CriteriaScores, // Individual criteria scores
        string? Comment
    );
}
