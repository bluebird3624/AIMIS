namespace Interchée.Contracts.Assignments
{
    public record RubricReadDto(
        int Id,
        string Name,
        string Description,
        bool IsActive,
        DateTime CreatedAt,
        List<RubricItemReadDto> Items
    );
}