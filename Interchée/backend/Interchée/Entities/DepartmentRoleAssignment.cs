namespace Interchée.Entities
{
    public class DepartmentRoleAssignment
    {
        public long Id { get; set; }
        public Guid UserId { get; set; }
        public int DepartmentId { get; set; }
        public string RoleName { get; set; } = default!; // Admin | HR | Supervisor | Instructor | Intern | Attaché
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public AppUser? User { get; set; }
        public Department? Department { get; set; }
    }
}
