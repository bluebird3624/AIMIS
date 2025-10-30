using Interchée.Contracts.Assignments;
using Interchée.Data;
using Interchée.Entities;
using Interchée.Extensions;
using Interchée.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Controllers
{
    [ApiController]
    [Route("attachments")]
    [Authorize]
    public class AttachmentsController(AppDbContext db, FileService fileService) : ControllerBase
    {
        private readonly AppDbContext _db = db;
        private readonly FileService _fileService = fileService;

        /// <summary>Upload a file attachment</summary>
        [HttpPost]
        [RequestSizeLimit(10_485_760)] // 10MB limit
        [ProducesResponseType(typeof(AttachmentReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<AttachmentReadDto>> Upload([FromForm] AttachmentCreateDto dto)
        {
            var userId = User.GetUserId();

            try
            {
                // Validate entity exists and user has access
                var hasAccess = await ValidateEntityAccess(dto.EntityType, dto.EntityId, userId);
                if (!hasAccess)
                    return Forbid();

                // Save file and create attachment record
                var attachment = await _fileService.SaveFileAsync(dto.File, dto.EntityType, dto.EntityId, userId);
                _db.Attachments.Add(attachment);
                await _db.SaveChangesAsync();

                // Get uploader name
                var uploadedByUserName = await _db.Users
                    .Where(u => u.Id == userId)
                    .Select(u => $"{u.FirstName} {u.LastName}")
                    .FirstOrDefaultAsync() ?? "Unknown";

                var readDto = new AttachmentReadDto(
                    attachment.Id,
                    attachment.FileName,
                    attachment.ContentType,
                    attachment.FileSize,
                    attachment.UploadedAt,
                    attachment.UploadedByUserId,
                    uploadedByUserName,
                    attachment.EntityType,
                    attachment.EntityId
                );

                return Ok(readDto);
            }
            catch (Exception ex)
            {
                return BadRequest($"File upload failed: {ex.Message}");
            }
        }

        /// <summary>Download a file attachment</summary>
        [HttpGet("{id:long}/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Download(long id)
        {
            var attachment = await _db.Attachments
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attachment == null) return NotFound();

            // Check if user has access to the entity
            var userId = User.GetUserId();
            var hasAccess = await ValidateEntityAccess(attachment.EntityType, attachment.EntityId, userId);
            if (!hasAccess) return Forbid();

            try
            {
                var (content, contentType, fileName) = await _fileService.GetFileAsync(attachment);
                return File(content, contentType, fileName);
            }
            catch (FileNotFoundException)
            {
                return NotFound("File not found on server");
            }
        }

        /// <summary>Get attachments for an entity</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AttachmentReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AttachmentReadDto>>> GetAttachments(
            [FromQuery] string entityType,
            [FromQuery] long entityId)
        {
            var userId = User.GetUserId();

            // Validate entity exists and user has access
            var hasAccess = await ValidateEntityAccess(entityType, entityId, userId);
            if (!hasAccess) return Forbid();

            var attachments = await _db.Attachments
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .Include(a => a.UploadedByUser)
                .OrderByDescending(a => a.UploadedAt)
                .Select(a => new AttachmentReadDto(
                    a.Id,
                    a.FileName,
                    a.ContentType,
                    a.FileSize,
                    a.UploadedAt,
                    a.UploadedByUserId,
                    $"{a.UploadedByUser!.FirstName} {a.UploadedByUser.LastName}",
                    a.EntityType,
                    a.EntityId
                ))
                .ToListAsync();

            return Ok(attachments);
        }

        /// <summary>Delete an attachment</summary>
        [HttpDelete("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(long id)
        {
            var attachment = await _db.Attachments
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attachment == null) return NotFound();

            // Check if user is the uploader or has admin rights
            var userId = User.GetUserId();
            var isUploader = attachment.UploadedByUserId == userId;
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("HR");

            if (!isUploader && !isAdmin)
                return Forbid();

            try
            {
                // Delete physical file
                _fileService.DeleteFile(attachment);

                // Delete database record
                _db.Attachments.Remove(attachment);
                await _db.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"File deletion failed: {ex.Message}");
            }
        }

        private async Task<bool> ValidateEntityAccess(string entityType, long entityId, Guid userId)
        {
            return entityType.ToLower() switch
            {
                "assignment" => await ValidateAssignmentAccess(entityId, userId),
                "submission" => await ValidateSubmissionAccess(entityId, userId),
                "feedback" => await ValidateFeedbackAccess(entityId, userId),
                _ => false
            };
        }

        private async Task<bool> ValidateAssignmentAccess(long assignmentId, Guid userId)
        {
            var assignment = await _db.Assignments
                .Include(a => a.Department)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null) return false;

            // Check if user has role in assignment's department
            return await _db.DepartmentRoleAssignments
                .AnyAsync(ra => ra.UserId == userId && ra.DepartmentId == assignment.DepartmentId);
        }

        private async Task<bool> ValidateSubmissionAccess(long submissionId, Guid userId)
        {
            var submission = await _db.AssignmentSubmissions
                .Include(s => s.Assignment)
                .ThenInclude(a => a!.Department)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null) return false;

            // User can access if they are the submitter or have department access
            var isOwner = submission.UserId == userId;
            if (isOwner) return true;

            // Check department access for supervisors
            return await _db.DepartmentRoleAssignments
                .AnyAsync(ra => ra.UserId == userId &&
                               ra.DepartmentId == submission.Assignment!.DepartmentId &&
                               (ra.RoleName == "Admin" || ra.RoleName == "HR" || ra.RoleName == "Supervisor"));
        }

        private async Task<bool> ValidateFeedbackAccess(long feedbackId, Guid userId)
        {
            // Implement feedback access validation as needed
            return await Task.FromResult(true);
        }
    }
}