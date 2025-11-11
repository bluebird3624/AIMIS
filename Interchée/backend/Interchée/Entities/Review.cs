namespace Interchée.Entities
{
    public class Review
    {
        public long Id { get; set; }

        public Guid UserId { get; set; } // The Attaché being reviewed
        public Guid SupervisorId { get; set; } // The Supervisor conducting review
        public int DepartmentId { get; set; }
         
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }

        public DateTime ScheduledAt { get; set; }
        public DateTime? ConductedAt { get; set; }
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled

        public decimal TeamworkScore { get; set; } // 0-5 scale
        public decimal CommunicationScore { get; set; }
        public decimal TechnicalSkillsScore { get; set; }
        public decimal InitiativeScore { get; set; }
        public decimal ProfessionalismScore { get; set; }

        public decimal OverallScore { get; set; } // Average of all scores
        public string? OverallComments { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public AppUser? User { get; set; }
        public AppUser? Supervisor { get; set; }
        public Department? Department { get; set; }
        public ICollection<ReviewFeedback> Feedbacks { get; set; } = new List<ReviewFeedback>();
    }
}

