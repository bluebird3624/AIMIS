namespace Interchée.Entities
{
    public class ReviewFeedback
    {
        public long Id { get; set; }
        public long ReviewId { get; set; }

        public string Category { get; set; } = default!; // Strengths, AreasForImprovement, Goals
        public string Feedback { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Review? Review { get; set; }
    }
}
