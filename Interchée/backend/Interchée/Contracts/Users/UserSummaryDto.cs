namespace Interchée.Contracts.Users
{
    /// <summary>
    /// Minimal read model for user listings.
    /// </summary>
    public record UserSummaryDto(
        Guid Id,
        string UserName,
        string? Email,
        bool IsActive
,
        string firstName,
        string lastName,
        string? middleName,
        string display);
}
