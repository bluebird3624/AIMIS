// Controllers/FeedbackController.cs
using Interchée.Contracts.Assignments;
using Interchée.Data;
using Interchée.Entities;
using Interchée.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Controllers
{
    [ApiController]
    [Route("feedback")]
    [Authorize]
    public class FeedbackController(AppDbContext db) : ControllerBase
    {
        private readonly AppDbContext _db = db;

        /// <summary>Submit feedback (Intern/Attaché only)</summary>
        [HttpPost]
        [Authorize(Roles = "Intern,Attache")]
        [ProducesResponseType(typeof(FeedbackReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<FeedbackReadDto>> SubmitFeedback([FromBody] FeedbackCreateDto dto)
        {
            var userId = User.GetUserId();

            var feedback = new Feedback
            {
                Title = dto.Title.Trim(),
                Message = dto.Message.Trim(),
                CreatedByUserId = userId,  // ✅ GUID conversion handled by EF
                CreatedAt = DateTime.UtcNow
            };

            _db.Feedbacks.Add(feedback);
            await _db.SaveChangesAsync();

            var createdByUserName = await _db.Users
                .Where(u => u.Id == userId)
                .Select(u => $"{u.FirstName} {u.LastName}")
                .FirstOrDefaultAsync() ?? "Unknown";

            var readDto = new FeedbackReadDto(
                feedback.Id,
                feedback.Title,
                feedback.Message,
                feedback.CreatedByUserId,
                createdByUserName,
                feedback.CreatedAt,
                new List<FeedbackReplyReadDto>()
            );

            return Ok(readDto);
        }

        /// <summary>Get all feedback (Supervisors/Admin/HR only)</summary>
        [HttpGet]
        [Authorize(Roles = "Admin,HR,Supervisor")]
        [ProducesResponseType(typeof(IEnumerable<FeedbackReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FeedbackReadDto>>> GetAllFeedback()
        {
            var feedback = await _db.Feedbacks
                .OrderByDescending(f => f.CreatedAt)
                .Include(f => f.CreatedByUser)
                .Include(f => f.Replies)
                    .ThenInclude(r => r.CreatedByUser)
                .Select(f => new FeedbackReadDto(
                    f.Id,
                    f.Title,
                    f.Message,
                    f.CreatedByUserId,
                    $"{f.CreatedByUser.FirstName} {f.CreatedByUser.LastName}",
                    f.CreatedAt,
                    f.Replies
                        .OrderBy(r => r.CreatedAt)
                        .Select(r => new FeedbackReplyReadDto(
                            r.Id,
                            r.FeedbackId,
                            r.Message,
                            r.CreatedByUserId,
                            $"{r.CreatedByUser.FirstName} {r.CreatedByUser.LastName}",
                            r.CreatedAt
                        ))
                        .ToList()
                ))
                .ToListAsync();

            return Ok(feedback);
        }

        /// <summary>Get my submitted feedback (Intern/Attaché only)</summary>
        [HttpGet("my-feedback")]
        [Authorize(Roles = "Intern,Attache")]
        [ProducesResponseType(typeof(IEnumerable<FeedbackReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FeedbackReadDto>>> GetMyFeedback()
        {
            var userId = User.GetUserId();

            var feedback = await _db.Feedbacks
                .Where(f => f.CreatedByUserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Include(f => f.CreatedByUser)
                .Include(f => f.Replies)
                    .ThenInclude(r => r.CreatedByUser)
                .Select(f => new FeedbackReadDto(
                    f.Id,
                    f.Title,
                    f.Message,
                    f.CreatedByUserId,
                    $"{f.CreatedByUser.FirstName} {f.CreatedByUser.LastName}",
                    f.CreatedAt,
                    f.Replies
                        .OrderBy(r => r.CreatedAt)
                        .Select(r => new FeedbackReplyReadDto(
                            r.Id,
                            r.FeedbackId,
                            r.Message,
                            r.CreatedByUserId,
                            $"{r.CreatedByUser.FirstName} {r.CreatedByUser.LastName}",
                            r.CreatedAt
                        ))
                        .ToList()
                ))
                .ToListAsync();

            return Ok(feedback);
        }

        /// <summary>Get feedback details by ID</summary>
        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(FeedbackReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FeedbackReadDto>> GetFeedbackById(long id)
        {
            var userId = User.GetUserId();
            var userRoles = User.GetRoles();

            var feedback = await _db.Feedbacks
                .Include(f => f.CreatedByUser)
                .Include(f => f.Replies)
                    .ThenInclude(r => r.CreatedByUser)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (feedback == null) return NotFound();

            // Authorization: User can only see their own feedback or supervisors can see any
            var isOwner = feedback.CreatedByUserId == userId;
            var isSupervisor = userRoles.Contains("Admin") || userRoles.Contains("HR") || userRoles.Contains("Supervisor");

            if (!isOwner && !isSupervisor)
                return Forbid();

            var replies = feedback.Replies
                .OrderBy(r => r.CreatedAt)
                .Select(r => new FeedbackReplyReadDto(
                    r.Id,
                    r.FeedbackId,
                    r.Message,
                    r.CreatedByUserId,
                    $"{r.CreatedByUser.FirstName} {r.CreatedByUser.LastName}",
                    r.CreatedAt
                ))
                .ToList();

            var readDto = new FeedbackReadDto(
                feedback.Id,
                feedback.Title,
                feedback.Message,
                feedback.CreatedByUserId,
                $"{feedback.CreatedByUser.FirstName} {feedback.CreatedByUser.LastName}",
                feedback.CreatedAt,
                replies
            );

            return Ok(readDto);
        }

        /// <summary>Reply to feedback (Supervisors/Admin/HR only)</summary>
        [HttpPost("{feedbackId:long}/reply")]
        [Authorize(Roles = "Admin,HR,Supervisor")]
        [ProducesResponseType(typeof(FeedbackReplyReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<FeedbackReplyReadDto>> ReplyToFeedback(long feedbackId, [FromBody] FeedbackReplyCreateDto dto)
        {
            var userId = User.GetUserId();

            var feedback = await _db.Feedbacks
                .FirstOrDefaultAsync(f => f.Id == feedbackId);

            if (feedback == null) return NotFound("Feedback not found");

            var reply = new FeedbackReply
            {
                FeedbackId = feedbackId,
                Message = dto.Message.Trim(),
                CreatedByUserId = userId,  
                CreatedAt = DateTime.UtcNow
            };

            _db.FeedbackReplies.Add(reply);
            await _db.SaveChangesAsync();

            var createdByUserName = await _db.Users
                .Where(u => u.Id == userId)
                .Select(u => $"{u.FirstName} {u.LastName}")
                .FirstOrDefaultAsync() ?? "Unknown";

            var readDto = new FeedbackReplyReadDto(
                reply.Id,
                reply.FeedbackId,
                reply.Message,
                reply.CreatedByUserId,
                createdByUserName,
                reply.CreatedAt
            );

            return Ok(readDto);
        }

        /// <summary>Delete my feedback (Intern/Attaché only)</summary>
        [HttpDelete("{feedbackId:long}")]
        [Authorize(Roles = "Intern,Attache")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteMyFeedback(long feedbackId)
        {
            var userId = User.GetUserId();

            var feedback = await _db.Feedbacks
                .FirstOrDefaultAsync(f => f.Id == feedbackId && f.CreatedByUserId == userId);

            if (feedback == null) return NotFound();

            // Can only delete if no replies yet
            if (await _db.FeedbackReplies.AnyAsync(r => r.FeedbackId == feedbackId))
            {
                return BadRequest("Cannot delete feedback that has replies.");
            }

            _db.Feedbacks.Remove(feedback);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}