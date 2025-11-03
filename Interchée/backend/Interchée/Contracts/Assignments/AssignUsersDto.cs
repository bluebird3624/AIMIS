using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Assignments
{
    // Assign users to assignment
    public record AssignUsersDto(
        //[Required] long AssignmentId,
        [Required] Guid[] UserIds
    );

    
}