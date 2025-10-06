namespace Interchée.Contracts.Absence
{
    public record AbsencePolicyReadDto(
        int Id,
        string Scope,
        int? DepartmentId,
        decimal MaxDaysPerTerm,
        decimal MaxDaysPerMonth,
        DateOnly EffectiveFrom,
        DateOnly? EffectiveTo
    );
}
