using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Assignments
{
    // Create comment
    public record FeedbackCreateDto([Required] string Comment);
}
