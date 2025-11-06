using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Assignments
{
    public record GradeUpdateDto(
        [Required]
        [Range(0, 99.99)]
        decimal Score,

        [Required]
        [Range(1, 100.00)]
        decimal MaxScore,

         int? RubricId, // Use rubric instead of manual JSON
        Dictionary<string, decimal>? CriteriaScores, // Individual criteria scores
        string? Comment
    );
}