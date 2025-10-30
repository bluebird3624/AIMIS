namespace Interchée.Contracts.Assignments
{
    public record RubricItemReadDto(
        int Id,
        string Criteria,
        string Description,
        decimal MaxScore,
        int Order
    );
}