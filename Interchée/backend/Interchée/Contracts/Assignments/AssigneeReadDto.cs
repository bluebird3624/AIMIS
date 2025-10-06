namespace Interchée.Contracts.Assignments
{
    // Read assignee info
    public record AssigneeReadDto(
        long Id,
        long AssignmentId,
        Guid UserId,
        DateTime AssignedAt,
        string UserName,
        string UserDisplayName
    );
}
