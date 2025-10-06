namespace Interchée.Entities
{
    public class AssignmentAssignee
    {
        public long Id { get; set; }
        public long AssignmentId { get; set; }
        public Guid UserId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Assignment? Assignment { get; set; }
        public AppUser? User { get; set; }
    }
}