using System.ComponentModel.DataAnnotations;
using Interchée.DTOs.Grades;

namespace Interchée.DTOs.Submissions
{
    public class SubmitAssignmentDto
    {
        [Required]
        [Url]
        [StringLength(2048)]
        public string GitRepositoryUrl { get; set; } = default!;
    }

    public class SubmissionResponseDto
    {
        public int Id { get; set; }
        public string GitRepositoryUrl { get; set; } = default!;
        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; } = default!;
        public string? LastCommitHash { get; set; }
        public string? BranchName { get; set; }
        public string InternName { get; set; } = default!;
        public GradeResponseDto? Grade { get; set; }
    }
}