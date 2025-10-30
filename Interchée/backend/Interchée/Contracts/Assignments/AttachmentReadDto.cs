namespace Interchée.Contracts.Assignments
{
    public record AttachmentReadDto(
        long Id,
        string FileName,
        string ContentType,
        long FileSize,
        DateTime UploadedAt,
        Guid UploadedByUserId,
        string UploadedByUserName,
        string EntityType,
        long EntityId
    );
}
