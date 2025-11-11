namespace Interchée.Entities
{
    public class RubricItem
    {
        public int Id { get; set; }
        public int RubricId { get; set; }
        public string Criteria { get; set; } = default!;
        public string Description { get; set; } = string.Empty;
        public decimal MaxScore { get; set; }
        public int Order { get; set; }

        // Navigation properties
        public Rubric? Rubric { get; set; }
    }
}