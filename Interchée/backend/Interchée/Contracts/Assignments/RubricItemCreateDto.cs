using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Assignments
{
    public record RubricItemCreateDto(
        [Required] string Criteria,
        string Description,
        [Required] decimal MaxScore,
        [Required] int Order
    );
}