using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Onboarding
{
    /// <summary>Create (anonymous or HR) onboarding request.</summary>
    public record OnboardingRequestCreateDto(
        [Required, EmailAddress] string Email,
        [Required, MaxLength(64)] string FirstName,
        [Required, MaxLength(64)] string LastName,
        [MaxLength(64)] string? MiddleName,
        [Range(1, int.MaxValue)] int DepartmentId
    );
}
