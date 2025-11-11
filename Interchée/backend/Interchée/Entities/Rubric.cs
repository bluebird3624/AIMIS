namespace Interchée.Entities
{
    public class Rubric
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<RubricItem> Items { get; set; } = new List<RubricItem>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}