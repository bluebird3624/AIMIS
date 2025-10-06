namespace Interchée.Contracts.Onboarding
{
    public record DecisionReadDto(
        long Id,
        string Action,           // Approved|Rejected|Reopened|Note
        string? Reason,
        Guid ActorUserId,
        DateTime CreatedAtUtc
    );

}
