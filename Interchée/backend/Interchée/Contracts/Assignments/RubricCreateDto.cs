using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Assignments
{
    public record RubricCreateDto(
        [Required] string Name,
        string Description,
        [Required] List<RubricItemCreateDto> Items
    );
}