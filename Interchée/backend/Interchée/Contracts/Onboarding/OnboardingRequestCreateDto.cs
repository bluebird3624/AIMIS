using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Onboarding
{
    public record OnboardingRequestCreateDto(
        [property: Required, EmailAddress] string Email,
        [property: Required, MaxLength(64)] string FirstName,
        [property: Required, MaxLength(64)] string LastName,
        [property: MaxLength(64)] string? MiddleName,
        [property: Range(1, int.MaxValue)] int DepartmentId
    );
}
