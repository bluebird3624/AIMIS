using System.ComponentModel.DataAnnotations;

namespace Interchée.Entities
{
    public class Feedback
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Comment { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public int SubmissionId { get; set; }
        public Guid SupervisorId { get; set; }

        // Navigation properties
        public AssignmentSubmission Submission { get; set; } = default!;
        public AppUser Supervisor { get; set; } = default!;
    }
}