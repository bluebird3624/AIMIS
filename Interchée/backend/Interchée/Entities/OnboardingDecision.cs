using System.ComponentModel.DataAnnotations;

namespace Interchée.Entities
{
    /// <summary>
    /// Immutable audit log for onboarding actions over time.
    /// Each action (Approved/Rejected/Reopened/Note) creates a row.
    /// </summary>
    public class OnboardingDecision
    {
        public long Id { get; set; }                        // PK

        public long RequestId { get; set; }                 // FK -> OnboardingRequest
        public OnboardingRequest Request { get; set; } = default!;

        [MaxLength(32)]
        public string Action { get; set; } = default!;      // "Approved" | "Rejected" | "Reopened" | "Note"

        [MaxLength(1000)]
        public string? Reason { get; set; }                 // optional text/reason

        public Guid ActorUserId { get; set; }               // who did it (Admin/HR)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // UTC timestamp
    }
}
