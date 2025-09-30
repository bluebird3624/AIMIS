namespace Interchée.Entities
{
    public class AbsenceRequest
    {
        public int Id { get; set; }
        public int InternId { get; set; }
        public AppUser? Intern { get; set; }
        public string? Reason { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AbsenceStatus Status { get; set; } = AbsenceStatus.Pending;
        public string? RejectionReason { get; set; }
        public string? ApprovedById { get; set; }
        public AppUser? ApprovedBy { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
    }
}
