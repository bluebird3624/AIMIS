using Interchée.DTOs;
using Interchée.DTOs.Grades;
using Interchée.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Interchée.Controllers
{
    [ApiController]
    [Route("api/submissions/{submissionId:int}/[controller]")]
    [Authorize]
    public class GradesController(IGradeService gradeService) : ControllerBase
    {
        private readonly IGradeService _gradeService = gradeService;

        [HttpPost]
        [Authorize(Roles = "Supervisor,Admin")]
        public async Task<IActionResult> GradeSubmission(int submissionId, [FromBody] GradeSubmissionDto dto)
        {
            try
            {
                var supervisorId = GetCurrentUserId();
                var result = await _gradeService.GradeSubmissionAsync(submissionId, dto, supervisorId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetGrade(int submissionId)
        {
            var grade = await _gradeService.GetGradeAsync(submissionId);
            return grade == null ? NotFound() : Ok(grade);
        }

        [HttpPut]
        [Authorize(Roles = "Supervisor,Admin")]
        public async Task<IActionResult> UpdateGrade(int submissionId, [FromBody] GradeSubmissionDto dto)
        {
            try
            {
                var supervisorId = GetCurrentUserId();
                var result = await _gradeService.UpdateGradeAsync(submissionId, dto, supervisorId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
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