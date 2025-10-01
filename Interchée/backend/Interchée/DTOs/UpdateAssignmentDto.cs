using System.ComponentModel.DataAnnotations;

namespace Interchée.DTOs
{
    public class UpdateAssignmentDto
    {
        [StringLength(256)]
        public string? Title { get; set; }

        [StringLength(4000)]
        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }
    }
}