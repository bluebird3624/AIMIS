using System.ComponentModel.DataAnnotations;

namespace Interchée.Entities
{
    public class Grade
    {
        public int Id { get; set; }

        [Range(0, 100)]
        public decimal Score { get; set; }

        [MaxLength(2000)]
        public string? Comments { get; set; }

        [MaxLength(4000)]
        public string? RubricEvaluationJson { get; set; }

        public DateTime GradedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public int SubmissionId { get; set; }
        public Guid GradedById { get; set; } // Supervisor who graded

        // Navigation properties
        public AssignmentSubmission Submission { get; set; } = default!;
        public AppUser GradedBy { get; set; } = default!;
    }
}