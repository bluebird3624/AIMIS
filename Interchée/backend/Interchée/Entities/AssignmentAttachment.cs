using System.ComponentModel.DataAnnotations;

namespace Interchée.Entities
{
    public class AssignmentAttachment
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(512)]
        public string FileName { get; set; } = default!;

        [Required]
        [MaxLength(1024)]
        public string FilePath { get; set; } = default!;

        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public int AssignmentId { get; set; }
        public Assignment Assignment { get; set; } = default!;
    }
}