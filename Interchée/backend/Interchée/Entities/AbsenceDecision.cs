namespace Interchée.Entities
{
    public class AbsenceDecision
    {
        public long Id { get; set; }

        public long RequestId { get; set; }
        public Guid DecidedByUserId { get; set; }

        public string Decision { get; set; } = default!; // Approved|Rejected
        public string? Comment { get; set; }

        public DateTime DecidedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public AbsenceRequest? Request { get; set; }
        public AppUser? DecidedByUser { get; set; }
    }
}
