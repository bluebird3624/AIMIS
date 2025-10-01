using System.ComponentModel.DataAnnotations;

namespace Interchée.Entities
{
    public class Assignment
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Title { get; set; } = default!;

        [MaxLength(4000)]
        public string Description { get; set; } = default!;

        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public AssignmentStatus Status { get; set; } = AssignmentStatus.Assigned;

        public Guid CreatedById { get; set; } // Supervisor who created
        public int DepartmentId { get; set; }

        public AppUser CreatedBy { get; set; } = default!;
        public Department Department { get; set; } = default!;

        public ICollection<AssignmentSubmission> Submissions { get; set; } = [];
        public ICollection<AssignmentAttachment> Attachments { get; set; } = [];
    }

    public enum AssignmentStatus
    {
        Assigned = 0,      // Assignment created but not started
        InProgress = 1,    // Intern has started working
        Submitted = 2,     // Intern submitted via Git
        Reviewed = 3,      // Supervisor reviewed
        Graded = 4         // Final grade assigned
    }
}