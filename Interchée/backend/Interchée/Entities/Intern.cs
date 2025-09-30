using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Interchée.Entities
{
    public class Intern
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual AppUser? User { get; set; }

        public Guid? SupervisorId { get; set; }

        [ForeignKey(nameof(SupervisorId))]
        public virtual AppUser? Supervisor { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [MaxLength(200)]
        public string? University { get; set; }

        [MaxLength(200)]
        public string? CourseOfStudy { get; set; }

        [Required]
        public InternStatus Status { get; set; } = InternStatus.Active;

        // Navigation properties
        public virtual ICollection<AbsenceRequest> AbsenceRequests { get; set; } = new List<AbsenceRequest>();
    }

    public enum InternStatus
    {
        Active = 0,
        Completed = 1,
        Terminated = 2
    }
}