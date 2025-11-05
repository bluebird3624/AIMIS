namespace Interchée.Entities
{
    public class Attachment
    {
        public long Id { get; set; }
        public string FileName { get; set; } = default!;
        public string StoredFileName { get; set; } = default!; // GUID-based filename on disk
        public string ContentType { get; set; } = default!;
        public long FileSize { get; set; } // in bytes
        public string FilePath { get; set; } = default!; // Relative path on server
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public Guid UploadedByUserId { get; set; }

        // Polymorphic relationships
        public string EntityType { get; set; } = default!; // "Assignment", "Submission", "Feedback"
        public long EntityId { get; set; } // ID of the related entity

        // Navigation properties
        public AppUser? UploadedByUser { get; set; }
    }
}
