namespace Interchée.Contracts.Onboarding
{
    /// <summary>Read model for onboarding requests (Admin/HR).</summary>
    public record OnboardingRequestReadDto(
        long Id,
        string Email,
        string FullName,
        string FirstName,
        string LastName,
        string? MiddleName,
        string Display,
        int DepartmentId,
        string Status,
        DateTime RequestedAt,
        Guid? ApprovedByUserId,
        DateTime? ApprovedAt,
        string ProposedUserName,
        List<DecisionReadDto>? Decisions);
        
}
