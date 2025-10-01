namespace Interchée.Entities
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; } = default!;
        public string? Code { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<DepartmentRoleAssignment> Assignments { get; set; } = new List<DepartmentRoleAssignment>();
        public object? Id { get; internal set; }
    }
}
