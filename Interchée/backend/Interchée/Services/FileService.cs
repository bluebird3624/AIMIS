using Interchée.Entities;
using Microsoft.AspNetCore.StaticFiles;

namespace Interchée.Services
{
    public class FileService
    {
        private readonly string _uploadsPath;
        private readonly FileExtensionContentTypeProvider _contentTypeProvider;

        public FileService(IConfiguration configuration)
        {
            _uploadsPath = configuration["FileUploads:Path"] ?? "Uploads";
            _contentTypeProvider = new FileExtensionContentTypeProvider();

            // Ensure uploads directory exists
            if (!Directory.Exists(_uploadsPath))
            {
                Directory.CreateDirectory(_uploadsPath);
            }
        }

        public async Task<Attachment> SaveFileAsync(IFormFile file, string entityType, long entityId, Guid userId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            // Validate file size (10MB max)
            if (file.Length > 10 * 1024 * 1024)
                throw new InvalidOperationException("File size cannot exceed 10MB");

            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName);
            var storedFileName = $"{Guid.NewGuid()}{fileExtension}";
            var relativePath = Path.Combine(entityType, storedFileName);
            var fullPath = Path.Combine(_uploadsPath, relativePath);

            // Ensure entity type directory exists
            var entityDir = Path.Combine(_uploadsPath, entityType);
            if (!Directory.Exists(entityDir))
            {
                Directory.CreateDirectory(entityDir);
            }

            // Save file to disk
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Get content type
            if (!_contentTypeProvider.TryGetContentType(file.FileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return new Attachment
            {
                FileName = file.FileName,
                StoredFileName = storedFileName,
                ContentType = contentType,
                FileSize = file.Length,
                FilePath = relativePath,
                EntityType = entityType,
                EntityId = entityId,
                UploadedByUserId = userId,
                UploadedAt = DateTime.UtcNow
            };
        }

        public async Task<(byte[] content, string contentType, string fileName)> GetFileAsync(Attachment attachment)
        {
            var fullPath = Path.Combine(_uploadsPath, attachment.FilePath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException("File not found");

            var content = await File.ReadAllBytesAsync(fullPath);
            return (content, attachment.ContentType, attachment.FileName);
        }

        public bool DeleteFile(Attachment attachment)
        {
            var fullPath = Path.Combine(_uploadsPath, attachment.FilePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
            return false;
        }

        public string GetContentType(string fileName)
        {
            if (!_contentTypeProvider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }


    }
}
