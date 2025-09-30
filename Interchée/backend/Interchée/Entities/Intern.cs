namespace Interchée.Entities
{
    public class Intern
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public virtual AppUser? User { get; set; }
        public Guid? SupervisorId { get; set; }
        public virtual AppUser? Supervisor { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? University { get; set; }
        public string? CourseOfStudy { get; set; }
        public InternStatus Status { get; set; } = InternStatus.Active;
        public virtual ICollection<AbsenceRequest> AbsenceRequests { get; set; } = new List<AbsenceRequest>();
    }

    public enum InternStatus
    {
        Active,
        Completed,
        Terminated
    }
}