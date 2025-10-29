using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Assignments
{
    // Create/Update grade
    public record GradeCreateDto(
        [Required][Range(0, 999.99)] decimal Score,
        [Required][Range(0, 999.99)] decimal MaxScore,
        string? RubricJson
    );
}
