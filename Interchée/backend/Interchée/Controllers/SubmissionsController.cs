using Interchée.DTOs;
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
        [ProducesResponseType(typeof(GradeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GradeSubmission(int submissionId, [FromBody] GradeSubmissionDto dto)
        {
            try
            {
                // ✅ Service resolves current AppUser internally; no user id needed here
                var result = await _gradeService.GradeSubmissionAsync(submissionId, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(GradeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGrade(int submissionId)
        {
            var grade = await _gradeService.GetGradeAsync(submissionId);
            return grade == null ? NotFound() : Ok(grade);
        }

        [HttpPut]
        [Authorize(Roles = "Supervisor,Admin")]
        [ProducesResponseType(typeof(GradeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateGrade(int submissionId, [FromBody] GradeSubmissionDto dto)
        {
            try
            {
                // ✅ Service resolves current AppUser internally
                var result = await _gradeService.UpdateGradeAsync(submissionId, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}
