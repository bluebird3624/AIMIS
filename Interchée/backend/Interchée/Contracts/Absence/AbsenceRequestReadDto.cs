namespace Interchée.Contracts.Absence
{
    public record AbsenceRequestReadDto(
        long Id,
        Guid UserId,
        int DepartmentId,
        DateOnly StartDate,
        DateOnly EndDate,
        decimal Days,
        string Reason,
        string Status,
        DateTime RequestedAt,
        AbsenceDecisionReadDto? Decision
    );
}
