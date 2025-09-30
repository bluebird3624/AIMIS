using System.ComponentModel.DataAnnotations;

namespace Interchée.DTOs.Grades
{
    public class GradeSubmissionDto
    {
        [Required]
        [Range(0, 100)]
        public decimal Score { get; set; }

        [StringLength(2000)]
        public string? Comments { get; set; }
    }

    public class GradeResponseDto
    {
        public int Id { get; set; }
        public decimal Score { get; set; }
        public string? Comments { get; set; }
        public DateTime GradedAt { get; set; }
        public string GradedBy { get; set; } = default!;
    }
}