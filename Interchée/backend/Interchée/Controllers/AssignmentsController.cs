using Interchée.DTOs;
using Interchée.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Interchée.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AssignmentsController(IAssignmentService assignmentService) : ControllerBase
    {
        private readonly IAssignmentService _assignmentService = assignmentService;

        [HttpPost]
        [Authorize(Roles = "Supervisor,Admin")]
        public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentDto dto)
        {
            try
            {
                var supervisorId = GetCurrentUserId();
                var result = await _assignmentService.CreateAssignmentAsync(dto, supervisorId);
                return CreatedAtAction(nameof(GetAssignment), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAssignments()
        {
            var assignments = await _assignmentService.GetAllAssignmentsAsync();
            return Ok(assignments);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAssignment(int id)
        {
            var assignment = await _assignmentService.GetAssignmentAsync(id);
            return assignment == null ? NotFound() : Ok(assignment);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Supervisor,Admin")]
        public async Task<IActionResult> UpdateAssignment(int id, [FromBody] UpdateAssignmentDto dto)
        {
            try
            {
                var result = await _assignmentService.UpdateAssignmentAsync(id, dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Supervisor,Admin")]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            try
            {
                await _assignmentService.DeleteAssignmentAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("department/{departmentId:int}")]
        public async Task<IActionResult> GetAssignmentsByDepartment(int departmentId)
        {
            var assignments = await _assignmentService.GetAssignmentsByDepartmentAsync(departmentId);
            return Ok(assignments);
        }

        [HttpGet("my-assignments")]
        [Authorize(Roles = "Intern,Attache")]
        public async Task<IActionResult> GetMyAssignments()
        {
            var internId = GetCurrentUserId();
            var assignments = await _assignmentService.GetAssignmentsForInternAsync(internId);
            return Ok(assignments);
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