namespace Interchée.Entities
{
    public class SubmissionCommit
    {
        public long Id { get; set; }
        public long SubmissionId { get; set; }
        public string Sha { get; set; } = default!;
        public string? Message { get; set; }
        public string? AuthorEmail { get; set; }
        public DateTime CommittedAt { get; set; }

        // Navigation properties
        public AssignmentSubmission? Submission { get; set; }
    }
}