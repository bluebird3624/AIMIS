using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Onboarding
{
    // Body sent when approving an onboarding request
    public record ApproveOnboardingDto(
        [Required, MinLength(3)] string UserName,
        [Required, MinLength(8)] string TempPassword,
        [Required] string RoleName   // use string, let service validate
    );
}
