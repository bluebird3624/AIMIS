namespace Interchée.Entities
{
    public class Grade
    {
        public long Id { get; set; }
        public long SubmissionId { get; set; }
        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }
        public string? RubricJson { get; set; }
        public Guid GradedByUserId { get; set; }
        public DateTime GradedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public AssignmentSubmission? Submission { get; set; }
        public AppUser? GradedByUser { get; set; }
    }
}