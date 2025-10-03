namespace Interchée.Entities
{
    public class AbsenceLimitPolicy
    {
        public int Id { get; set; }

        public string Scope { get; set; } = default!; // Global|Department
        public int? DepartmentId { get; set; } // Required if Scope=Department

        public decimal MaxDaysPerTerm { get; set; } // (5,2)
        public decimal MaxDaysPerMonth { get; set; } // (4,1)

        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }

        // Navigation property
        public Department? Department { get; set; }
    }
}
