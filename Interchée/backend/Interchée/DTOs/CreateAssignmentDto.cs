using System.ComponentModel.DataAnnotations;

namespace Interchée.DTOs
{
    public class CreateAssignmentDto
    {
        [Required]
        [StringLength(256)]
        public string Title { get; set; } = default!;

        [StringLength(4000)]
        public string Description { get; set; } = default!;

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public int DepartmentId { get; set; }
    }

    public class AssignmentResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = default!;
        public string Department { get; set; } = default!;
        public int SubmissionCount { get; set; }
    }
}