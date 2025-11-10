namespace Interchée.Entities
{
    public class Feedback
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Guid CreatedByUserId { get; set; } 
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public AppUser CreatedByUser { get; set; } = null!;
        public ICollection<FeedbackReply> Replies { get; set; } = new List<FeedbackReply>();

    }
}
