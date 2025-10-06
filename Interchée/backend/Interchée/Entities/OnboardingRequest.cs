namespace Interchée.Entities
{
    public class OnboardingRequest
    {
        public long Id { get; set; }

        public string Email { get; set; } = default!;

        // Structured names (PreferredName omitted)
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string? MiddleName { get; set; }

        // Convenience for reads/search
        public string FullName { get; set; } = default!;

        public int DepartmentId { get; set; }
        public string Status { get; set; } = "Pending"; // Pending|Approved|Rejected
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public Department? Department { get; set; }

        public ICollection<OnboardingDecision> Decisions { get; set; } = new List<OnboardingDecision>();
    }
}

