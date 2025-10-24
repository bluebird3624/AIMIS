using System.Diagnostics;

namespace Interchée.Entities
{
    public class AssignmentSubmission
    {
        public long Id { get; set; }
        public long AssignmentId { get; set; }
        public Guid UserId { get; set; }
        public string? RepoUrl { get; set; }
        public string? Branch { get; set; }
        public string? LatestCommitSha { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string Status { get; set; } = "InProgress"; // InProgress|Submitted|Reviewed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Assignment? Assignment { get; set; }
        public AppUser? User { get; set; }
        public ICollection<SubmissionCommit> Commits { get; set; } = new List<SubmissionCommit>();
        public Grade? Grade { get; set; }
        public ICollection<FeedbackComment> FeedbackComments { get; set; } = new List<FeedbackComment>();
    }
}