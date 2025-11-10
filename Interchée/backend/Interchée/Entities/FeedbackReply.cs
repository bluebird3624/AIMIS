namespace Interchée.Entities
{
    public class FeedbackReply
    {
        public long Id { get; set; }
        public long FeedbackId { get; set; } 
        public string Message { get; set; } = string.Empty;
        public Guid CreatedByUserId { get; set; } 
        public DateTime CreatedAt { get; set; }

        //Navigation Properties

        public Feedback Feedback { get; set; } = null!;
        public AppUser CreatedByUser { get; set; } = null!;
    }
}
