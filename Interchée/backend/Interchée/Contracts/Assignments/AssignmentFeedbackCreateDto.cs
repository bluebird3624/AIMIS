using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Assignments
{
    // Create comment
    public record AssignmentFeedbackCreateDto([Required] string Comment);
}
