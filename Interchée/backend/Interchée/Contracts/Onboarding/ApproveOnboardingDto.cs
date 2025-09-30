using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Onboarding
{
    public record ApproveOnboardingDto(
    [property: Required, MinLength(3)] string UserName,
    [property: Required, MinLength(8)] string TempPassword
);
}
