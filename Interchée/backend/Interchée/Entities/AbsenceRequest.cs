using System.ComponentModel.DataAnnotations.Schema;

namespace Interchée.Entities
{
    public class AbsenceRequest
    {
        public int Id { get; set; }
        public int InternId { get; set; }
        public virtual Intern? Intern { get; set; }
        public string? Reason { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AbsenceStatus Status { get; set; } = AbsenceStatus.Pending;
        public string? RejectionReason { get; set; }
        public Guid? ApprovedById { get; set; }
        public virtual AppUser? ApprovedBy { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }

        // Computed property
        [NotMapped]
        public int TotalDays => (EndDate - StartDate).Days + 1;
    }

    public enum AbsenceStatus
    {
        Pending,
        Approved,
        Rejected
    }
}