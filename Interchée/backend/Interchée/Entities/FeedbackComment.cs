namespace Interchée.Entities
{
    public class FeedbackComment
    {
        public long Id { get; set; }
        public long SubmissionId { get; set; }
        public Guid AuthorUserId { get; set; }
        public string Comment { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public AssignmentSubmission? Submission { get; set; }
        public AppUser? AuthorUser { get; set; }
    }
}