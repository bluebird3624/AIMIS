using Interchée.Contracts.Review;
using Interchée.Data;
using Interchée.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Interchée.Controllers
{
    [ApiController]
    [Route("reviews")]
    public class ReviewsController(AppDbContext db) : ControllerBase
    {
        private readonly AppDbContext _db = db;

        /// <summary>Schedule a review for an Attaché</summary>
        [HttpPost]
        [Authorize(Roles = "Supervisor")]
        [ProducesResponseType(typeof(ReviewReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ReviewReadDto>> ScheduleReview([FromBody] ReviewScheduleDto dto)
        {
            var supervisorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Verify user exists and is an Attaché in supervisor's department
            var userDept = await _db.DepartmentRoleAssignments
                .Where(x => x.UserId == dto.UserId && x.RoleName == "Attache")
                .Select(x => x.DepartmentId)
                .FirstOrDefaultAsync();

            if (userDept == 0) return BadRequest("User is not an Attaché or not found");

            // Verify supervisor has access to this department
            var supervisorDept = await _db.DepartmentRoleAssignments
                .Where(x => x.UserId == supervisorId &&
                           (x.RoleName == "Supervisor" || x.RoleName == "HR" || x.RoleName == "Admin"))
                .Select(x => x.DepartmentId)
                .FirstOrDefaultAsync();

            if (supervisorDept != userDept && !User.IsInRole("Admin") && !User.IsInRole("HR"))
                return BadRequest("You can only schedule reviews for Attachés in your department");

            var review = new Review
            {
                UserId = dto.UserId,
                SupervisorId = supervisorId,
                DepartmentId = userDept,
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                Location = dto.Location?.Trim(),
                ScheduledAt = dto.ScheduledAt,
                Status = "Scheduled",
                CreatedAt = DateTime.UtcNow
            };

            _db.Reviews.Add(review);
            await _db.SaveChangesAsync();

            return Ok(ToReadDto(review));
        }

        /// <summary>Schedule reviews for multiple Attachés at once</summary>
        [HttpPost("bulk")]
        [Authorize(Roles = "Supervisor,HR,Admin")]
        [ProducesResponseType(typeof(List<ReviewReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<ReviewReadDto>>> ScheduleBulkReviews([FromBody] BulkReviewScheduleDto dto)
        {
            var supervisorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Verify supervisor has access to a department
            var supervisorDept = await _db.DepartmentRoleAssignments
                .Where(x => x.UserId == supervisorId &&
                           (x.RoleName == "Supervisor" || x.RoleName == "HR" || x.RoleName == "Admin"))
                .Select(x => x.DepartmentId)
                .FirstOrDefaultAsync();

            if (supervisorDept == 0) return BadRequest("Supervisor is not assigned to a valid department");

            // Verify all users exist and are Attachés in supervisor's department
            var validUsers = await _db.DepartmentRoleAssignments
                .Where(x => dto.UserIds.Contains(x.UserId) &&
                           x.RoleName == "Attache" &&
                           (x.DepartmentId == supervisorDept || User.IsInRole("Admin") || User.IsInRole("HR")))
                .Select(x => new { x.UserId, x.DepartmentId })
                .ToListAsync();

            if (validUsers.Count != dto.UserIds.Count)
            {
                var invalidUserIds = dto.UserIds.Except(validUsers.Select(x => x.UserId)).ToList();
                return BadRequest($"The following users are not Attachés in your department: {string.Join(", ", invalidUserIds)}");
            }

            var reviews = new List<Review>();

            foreach (var user in validUsers)
            {
                var review = new Review
                {
                    UserId = user.UserId,
                    SupervisorId = supervisorId,
                    DepartmentId = user.DepartmentId,
                    Title = dto.Title.Trim(),
                    Description = dto.Description?.Trim(),
                    Location = dto.Location?.Trim(),
                    ScheduledAt = dto.ScheduledAt,
                    Status = "Scheduled",
                    CreatedAt = DateTime.UtcNow
                };

                reviews.Add(review);
            }

            _db.Reviews.AddRange(reviews);
            await _db.SaveChangesAsync();

            var readDtos = reviews.Select(r => ToReadDto(r)).ToList();
            return Ok(readDtos);
        }

        /// <summary>Submit review scores and feedback</summary>
        [HttpPost("{id:long}/submit")]
        [Authorize(Roles = "Supervisor,HR,Admin")]
        [ProducesResponseType(typeof(ReviewReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ReviewReadDto>> SubmitReview(
            long id, [FromBody] ReviewSubmitDto dto)
        {
            var review = await _db.Reviews
                .Include(r => r.Feedbacks)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null) return NotFound();
            if (review.Status != "Scheduled") return BadRequest("Review already completed or cancelled");

            // Calculate overall score (average of all categories)
            var overallScore = (dto.TeamworkScore + dto.CommunicationScore +
                              dto.TechnicalSkillsScore + dto.InitiativeScore +
                              dto.ProfessionalismScore) / 5;

            review.TeamworkScore = dto.TeamworkScore;
            review.CommunicationScore = dto.CommunicationScore;
            review.TechnicalSkillsScore = dto.TechnicalSkillsScore;
            review.InitiativeScore = dto.InitiativeScore;
            review.ProfessionalismScore = dto.ProfessionalismScore;
            review.OverallScore = overallScore;
            review.OverallComments = dto.OverallComments;
            review.ConductedAt = DateTime.UtcNow;
            review.Status = "Completed";

            // Add feedback items
            if (dto.Feedbacks != null)
            {
                foreach (var feedbackDto in dto.Feedbacks)
                {
                    review.Feedbacks.Add(new ReviewFeedback
                    {
                        Category = feedbackDto.Category,
                        Feedback = feedbackDto.Feedback,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _db.SaveChangesAsync();
            return Ok(ToReadDto(review));
        }

        /// <summary>Get reviews for an Attaché</summary>
        [HttpGet("user/{userId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<ReviewReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ReviewReadDto>>> GetUserReviews(Guid userId)
        {
            var reviews = await _db.Reviews
                .Where(r => r.UserId == userId)
                .Include(r => r.Feedbacks)
                .Include(r => r.Supervisor)
                .OrderByDescending(r => r.ScheduledAt)
                .Select(r => ToReadDto(r))
                .ToListAsync();

            return Ok(reviews);
        }
        /// <summary>Reschedule or update a review</summary>
        [HttpPut("{id:long}")]
        [Authorize(Roles = "Supervisor")]
        [ProducesResponseType(typeof(ReviewReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ReviewReadDto>> UpdateReview(
            long id, [FromBody] ReviewUpdateDto dto)
        {
            var supervisorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var review = await _db.Reviews
                .Include(r => r.Feedbacks)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null) return NotFound();

            // Verify the current user is the supervisor who created the review
            // Or is HR/Admin who can modify any review
            if (review.SupervisorId != supervisorId && !User.IsInRole("Admin") && !User.IsInRole("HR"))
                return BadRequest("You can only update reviews you created");

            // Only allow updates to scheduled reviews
            if (review.Status != "Scheduled")
                return BadRequest("Can only update scheduled reviews");

            // Update the scheduled time
            review.ScheduledAt = dto.ScheduledAt;

            await _db.SaveChangesAsync();

            return Ok(ToReadDto(review));
        }
        /// <summary>Cancel a scheduled review</summary>
        [HttpPut("{id:long}/cancel")]
        [Authorize(Roles = "Supervisor")]
        [ProducesResponseType(typeof(ReviewReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ReviewReadDto>> CancelReview(long id)
        {
            var supervisorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var review = await _db.Reviews
                .Include(r => r.Feedbacks)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null) return NotFound();

            // Verify the current user is the supervisor who created the review
           
            if (review.SupervisorId != supervisorId && !User.IsInRole("Admin") && !User.IsInRole("HR"))
                return BadRequest("You can only cancel reviews you created");

            // Only allow cancellation of scheduled reviews
            if (review.Status != "Scheduled")
                return BadRequest("Can only cancel scheduled reviews");

            review.Status = "Cancelled";

            await _db.SaveChangesAsync();

            return Ok(ToReadDto(review));
        }

        /// <summary>Get department reviews (for Supervisors/HR)</summary>
        [HttpGet("department/{departmentId:int}")]
        [Authorize(Roles = "Supervisor,HR,Admin")]
        [ProducesResponseType(typeof(IEnumerable<ReviewReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ReviewReadDto>>> GetDepartmentReviews(int departmentId)
        {
            var reviews = await _db.Reviews
                .Where(r => r.DepartmentId == departmentId)
                .Include(r => r.Feedbacks)
                .Include(r => r.User)
                .Include(r => r.Supervisor)
                .OrderByDescending(r => r.ScheduledAt)
                .Select(r => ToReadDto(r))
                .ToListAsync();

            return Ok(reviews);
        }

        private static ReviewReadDto ToReadDto(Review review)
        {
            return new ReviewReadDto(
                review.Id,
                review.UserId,
                review.SupervisorId,
                review.DepartmentId,
                review.Title,
                review.Description,
                review.Location,
                review.ScheduledAt,
                review.ConductedAt,
                review.Status,
                review.TeamworkScore,
                review.CommunicationScore,
                review.TechnicalSkillsScore,
                review.InitiativeScore,
                review.ProfessionalismScore,
                review.OverallScore,
                review.OverallComments,
                review.CreatedAt,
                review.Feedbacks?.Select(f => new ReviewFeedbackReadDto(
                    f.Id, f.Category, f.Feedback, f.CreatedAt
                )).ToList()
            );
        }
    }
}