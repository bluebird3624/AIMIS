namespace Interchée.Contracts.Assignments
{
    public record AssignmentAssigneeReadDto(
     Guid UserId,
     string FirstName,
     string LastName,
     string Email,
     string RoleName,
     DateTime AssignedAt
 );
}
