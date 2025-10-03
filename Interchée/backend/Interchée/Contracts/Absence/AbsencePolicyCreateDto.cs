using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Absence
{
    public record AbsencePolicyCreateDto(
        [Required] string Scope, // Global|Department
        int? DepartmentId, // Required if Scope=Department
        [Range(0, 999.99)] decimal MaxDaysPerTerm,
        [Range(0, 999.9)] decimal MaxDaysPerMonth,
        [Required] DateOnly EffectiveFrom,
        DateOnly? EffectiveTo
    );
}
