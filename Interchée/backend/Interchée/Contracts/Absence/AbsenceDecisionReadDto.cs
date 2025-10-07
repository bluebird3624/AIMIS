namespace Interchée.Contracts.Absence
{
    public record AbsenceDecisionReadDto(
        long Id,
        Guid DecidedByUserId,
        string Decision,
        string? Comment,
        DateTime DecidedAt
    );
}
