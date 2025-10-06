namespace Interchée.Entities
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Code { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<DepartmentRoleAssignment> Assignments { get; set; } = [];
    }
}
