using Interchée.Entities.Enums;

namespace Interchée.Entities
{
    public class Assignment
    {
        public long Id { get; set; }
        public int DepartmentId { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? DueAt { get; set; }
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Created; // Created|Assigned|Closed|
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Department? Department { get; set; }
        public AppUser? CreatedByUser { get; set; }
        public ICollection<AssignmentAssignee> Assignees { get; set; } = new List<AssignmentAssignee>();
        public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
