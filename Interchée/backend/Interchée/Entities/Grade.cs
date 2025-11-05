namespace Interchée.Entities
{
    public class Grade
    {
        public long Id { get; set; }
        public long SubmissionId { get; set; }
        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }

        // RubricId
        public int? RubricId { get; set; }
        public string? RubricScoresJson { get; set; } // Store individual criteria scores

        public Guid GradedByUserId { get; set; }
        public DateTime GradedAt { get; set; }
        public string? Comment { get; set; }

        // Navigation properties
        public AssignmentSubmission? Submission { get; set; }
        public AppUser? GradedByUser { get; set; }
        public Rubric? Rubric { get; set; }
    }
}