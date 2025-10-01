using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Interchée.Entities
{
    public class AssignmentSubmission
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(2048)]
        public string GitRepositoryUrl { get; set; } = default!;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Submitted;

        // Git metadata
        [MaxLength(64)]
        public string? LastCommitHash { get; set; }

        [MaxLength(8000)]
        public string? CommitHistoryJson { get; set; } // Store commit history as JSON

        [MaxLength(256)]
        public string? BranchName { get; set; }

        // Foreign keys
        public int AssignmentId { get; set; }
        public Guid InternId { get; set; }

        // Navigation properties
        public Assignment Assignment { get; set; } = default!;
        public AppUser Intern { get; set; } = default!;
        public Grade? Grade { get; set; }
        public ICollection<Feedback> Feedbacks { get; set; } = [];
    }
}