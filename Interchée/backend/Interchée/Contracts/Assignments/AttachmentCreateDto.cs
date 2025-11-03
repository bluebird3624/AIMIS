using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Assignments
{
    public record AttachmentCreateDto(
       [Required] IFormFile File,
       [Required] string EntityType, // "Assignment", "Submission", "Feedback"
       [Required] long EntityId
   );
}
