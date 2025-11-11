using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Interchée.Contracts.Assignments
{
    public record FileSubmissionCreateDto(
        [Required] long AssignmentId,
        [Required] IFormFile File
    );
}