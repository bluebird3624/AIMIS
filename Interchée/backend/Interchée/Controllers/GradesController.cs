using Interchée.DTOs;
using Interchée.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Interchée.Controllers
{
    [ApiController]
    [Route("api/assignments/{assignmentId:int}/[controller]")]
    [Authorize]
    public class SubmissionsController(ISubmissionService submissionService) : ControllerBase
    {
        private readonly ISubmissionService _submissionService = submissionService;

        [HttpPost]
        [Authorize(Roles = "Intern,Attache")]
        public async Task<IActionResult> SubmitAssignment(int assignmentId, [FromBody] SubmitAssignmentDto dto)
        {
            try
            {
                var internId = GetCurrentUserId();
                var result = await _submissionService.SubmitAssignmentAsync(assignmentId, dto, internId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Supervisor,Admin")]
        public async Task<IActionResult> GetSubmissions(int assignmentId)
        {
            var submissions = await _submissionService.GetSubmissionsByAssignmentAsync(assignmentId);
            return Ok(submissions);
        }

        [HttpGet("my-submission")]
        [Authorize(Roles = "Intern,Attache")]
        public async Task<IActionResult> GetMySubmission(int assignmentId)
        {
            var internId = GetCurrentUserId();
            var submission = await _submissionService.GetSubmissionByAssignmentAndInternAsync(assignmentId, internId);
            return submission == null ? NotFound() : Ok(submission);
        }

        [HttpPut("{submissionId:int}")]
        [Authorize(Roles = "Intern,Attache")]
        public async Task<IActionResult> UpdateSubmission(int submissionId, [FromBody] UpdateSubmissionDto dto)
        {
            try
            {
                var internId = GetCurrentUserId();
                var result = await _submissionService.UpdateSubmissionAsync(submissionId, dto, internId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpDelete("{submissionId:int}")]
        [Authorize(Roles = "Intern,Attache,Admin")]
        public async Task<IActionResult> DeleteSubmission(int submissionId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                await _submissionService.DeleteSubmissionAsync(submissionId, currentUserId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId)
                ? userId
                : throw new UnauthorizedAccessException("User ID not found in token");
        }
    }
}