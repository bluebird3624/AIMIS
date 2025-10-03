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

        // CREATE
        [HttpPost]
        [Authorize(Roles = "Supervisor,Admin")]
        public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentDto dto)
        {
            try
            {
                // ✅ New service signature doesn't need a user id
                var result = await _assignmentService.CreateAssignmentAsync(dto);
                return CreatedAtAction(nameof(GetAssignment), new { id = result.Id }, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // READ ALL
        [HttpGet]
        public async Task<IActionResult> GetAllAssignments()
        {
            var assignments = await _assignmentService.GetAllAssignmentsAsync();
            return Ok(assignments);
        }

        // READ ONE
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAssignment(int id)
        {
            var assignment = await _assignmentService.GetAssignmentAsync(id);
            return assignment == null ? NotFound() : Ok(assignment);
        }

        // UPDATE
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

        // DELETE
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

        // BY DEPARTMENT
        [HttpGet("department/{departmentId:int}")]
        public async Task<IActionResult> GetAssignmentsByDepartment(int departmentId)
        {
            var assignments = await _assignmentService.GetAssignmentsByDepartmentAsync(departmentId);
            return Ok(assignments);
        }

        // CURRENT INTERN'S ASSIGNMENTS
        [HttpGet("my-assignments")]
        [Authorize(Roles = "Intern,Attache")]
        public async Task<IActionResult> GetMyAssignments()
        {
            var appUserId = GetCurrentUserId(); // This is AppUser.Id (Guid)
            // Ensure your service/repo expects AppUser.Id here.
            // If it expects Intern.Id, resolve it first (map AppUser.Id -> Intern.Id).
            var assignments = await _assignmentService.GetAssignmentsForInternAsync(appUserId);
            return Ok(assignments);
        }

        // ===== Helpers =====
        private Guid GetCurrentUserId()
        {
            // Prefer NameIdentifier, fallback to 'sub'
            var userIdClaim =
                User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                ?? User.FindFirst("sub");

            return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId)
                ? userId
                : throw new UnauthorizedAccessException("User ID not found in token");
        }
    }
}
