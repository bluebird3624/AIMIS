namespace Interchée.Entities
{
    public class AbsenceRequest
    {
        public long Id { get; set; }

        public Guid UserId { get; set; }
        public int DepartmentId { get; set; }

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal Days { get; set; } 

        public string Reason { get; set; } = default!;
        public string Status { get; set; } = "Pending"; // Pending|Approved|Rejected|Cancelled

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public AppUser? User { get; set; }
        public Department? Department { get; set; }
        public AbsenceDecision? Decision { get; set; }
    }
}
