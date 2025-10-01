using System.ComponentModel.DataAnnotations;

namespace Interchée.DTOs
{
    public class UpdateSubmissionDto
    {
        [Required]
        [Url]
        [StringLength(2048)]
        public string GitRepositoryUrl { get; set; } = default!;
    }
}