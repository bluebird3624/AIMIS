using Interchée.Contracts.Assignments;
using Interchée.Data;
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

        /// <summary>Upload a file attachment (Auto-detects entity type based on user role)</summary>
        [HttpPost("auto-detect")]
        [RequestSizeLimit(10_485_760)] // 10MB limit
        [ProducesResponseType(typeof(AttachmentReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<AttachmentReadDto>> Upload([FromForm] AttachmentCreateDto dto)
        {
            var userId = User.GetUserId();

            try
            {
                // AUTO-DETECT ENTITY TYPE BASED ON USER ROLE
                string entityType;
                if (User.IsInRole("Intern") || User.IsInRole("Attache"))
                {
                    entityType = "Submission";
                }
                else if (User.IsInRole("Admin") || User.IsInRole("HR") || User.IsInRole("Supervisor"))
                {
                    // Supervisors can upload to assignments or submissions (for feedback)
                    // For now, default to Assignment for supervisors
                    entityType = "Assignment";
                }
                else
                {
                    return Forbid("User role not supported for file uploads");
                }

                // Validate entity exists and user has access
                var (isValid, error) = await ValidateEntityAccess(entityType, dto.EntityId, userId);
                if (!isValid)
                    return Forbid(error);

                // Save file and create attachment record
                var attachment = await _fileService.SaveFileAsync(dto.File, entityType, dto.EntityId, userId);
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
        
        /// <summary>Upload submission file (Intern/Attaché only - explicit endpoint)</summary>
        [HttpPost("submissions/{submissionId:long}")]
        [Authorize(Roles = "Intern,Attache")]
        [RequestSizeLimit(10_485_760)]
        [ProducesResponseType(typeof(AttachmentReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<AttachmentReadDto>> UploadToSubmission(long submissionId,  [FromForm] AttachmentCreateDto dto)
        {
            var userId = User.GetUserId();

            try
            {
                // Validate submission exists and user owns it
                var submission = await _db.AssignmentSubmissions
                    .Include(s => s.Assignment)
                    .FirstOrDefaultAsync(s => s.Id == submissionId && s.UserId == userId);

                if (submission == null)
                    return Forbid("Submission not found or access denied");

                // Check if assignment allows submissions
                if (submission.Assignment?.Status == "Closed" || submission.Assignment?.Status == "Archived")
                {
                    return BadRequest("Cannot upload to a closed assignment");
                }

                // Save file as submission attachment
                var attachment = await _fileService.SaveFileAsync(dto.File, "Submission", submissionId, userId);
                _db.Attachments.Add(attachment);
                await _db.SaveChangesAsync();

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

        /// <summary>Upload assignment file (Supervisors only - explicit endpoint)</summary>
        [HttpPost("assignments/{assignmentId:long}")]
        [Authorize(Roles = "Admin,HR,Supervisor")]
        [RequestSizeLimit(10_485_760)]
        [ProducesResponseType(typeof(AttachmentReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<AttachmentReadDto>> UploadToAssignment(long assignmentId,  [FromForm] AttachmentCreateDto dto)
        {
            var userId = User.GetUserId();

            try
            {
                // Validate assignment exists and user has access
                var assignment = await _db.Assignments
                    .Include(a => a.Department)
                    .FirstOrDefaultAsync(a => a.Id == assignmentId);

                if (assignment == null)
                    return NotFound("Assignment not found");

                // Check if user has access to assignment's department
                var hasAccess = await _db.DepartmentRoleAssignments
                    .AnyAsync(ra => ra.UserId == userId &&
                                   ra.DepartmentId == assignment.DepartmentId &&
                                   (ra.RoleName == "Admin" || ra.RoleName == "HR" || ra.RoleName == "Supervisor"));

                if (!hasAccess)
                    return Forbid("No access to this assignment");

                // Save file as assignment attachment
                var attachment = await _fileService.SaveFileAsync(dto.File, "Assignment", assignmentId, userId);
                _db.Attachments.Add(attachment);
                await _db.SaveChangesAsync();

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
            var (hasAccess, error) = await ValidateEntityAccess(attachment.EntityType, attachment.EntityId, userId);
            if (!hasAccess) return Forbid(error);

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
            var (hasAccess, error) = await ValidateEntityAccess(entityType, entityId, userId);
            if (!hasAccess) return BadRequest(error);

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

        private async Task<(bool isValid, string error)> ValidateEntityAccess(string entityType, long entityId, Guid userId)
        {
            return entityType.ToLower() switch
            {
                "assignment" => await ValidateAssignmentAccess(entityId, userId),
                "submission" => await ValidateSubmissionAccess(entityId, userId),
                _ => (false, "Invalid entity type")
            };
        }

        private async Task<(bool isValid, string error)> ValidateAssignmentAccess(long assignmentId, Guid userId)
        {
            var assignment = await _db.Assignments
                .Include(a => a.Department)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null) return (false, "Assignment not found");

            // Check if user has role in assignment's department
            var hasAccess = await _db.DepartmentRoleAssignments
                .AnyAsync(ra => ra.UserId == userId && ra.DepartmentId == assignment.DepartmentId);

            return (hasAccess, hasAccess ? "" : "No access to this assignment");
        }

        private async Task<(bool isValid, string error)> ValidateSubmissionAccess(long submissionId, Guid userId)
        {
            var submission = await _db.AssignmentSubmissions
                .Include(s => s.Assignment)
                .ThenInclude(a => a!.Department)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null) return (false, "Submission not found");

            // User can access if they are the submitter
            var isOwner = submission.UserId == userId;
            if (isOwner) return (true, "");

            // Check department access for supervisors
            var hasDepartmentAccess = await _db.DepartmentRoleAssignments
                .AnyAsync(ra => ra.UserId == userId &&
                               ra.DepartmentId == submission.Assignment!.DepartmentId &&
                               (ra.RoleName == "Admin" || ra.RoleName == "HR" || ra.RoleName == "Supervisor"));

            return (hasDepartmentAccess, hasDepartmentAccess ? "" : "No access to this submission");
        }
    }
}