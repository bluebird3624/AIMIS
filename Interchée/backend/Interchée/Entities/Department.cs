namespace Interchée.Entities
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = default!;
        public string? Code { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<DepartmentRoleAssignment> Assignments { get; set; } = [];
        public int? Id { get; internal set; }
        public string? Name { get; internal set; }
    }
}
