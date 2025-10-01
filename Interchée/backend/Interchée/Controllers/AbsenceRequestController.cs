using Interchée.Common;
using Interchée.Dtos;
using Interchée.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Interchée.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AbsenceRequestsController : ControllerBase
    {
        private readonly IAbsenceService _absenceService;
        private readonly ILogger<AbsenceRequestsController> _logger;

        public AbsenceRequestsController(IAbsenceService absenceService, ILogger<AbsenceRequestsController> logger)
        {
            _absenceService = absenceService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Intern")]
        public async Task<ActionResult<ApiResponse<AbsenceRequestDto>>> CreateAbsenceRequest(CreateAbsenceRequestDto request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse.Error("Unauthorized"));

            var result = await _absenceService.CreateAbsenceRequestAsync(request, userId);

            if (!result.Success)
                return BadRequest(ApiResponse.Error(result.Message));

            return Ok(ApiResponse.Success(result.Data, result.Message));
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Supervisor,HR,Admin")]
        public async Task<ActionResult<ApiResponse>> ApproveAbsenceRequest(int id, ApproveAbsenceRequestDto request)
        {
            var approverId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(approverId))
                return Unauthorized(ApiResponse.Error("Unauthorized"));

            var result = await _absenceService.ApproveAbsenceRequestAsync(id, request, approverId);

            if (!result.Success)
                return BadRequest(ApiResponse.Error(result.Message));

            return Ok(ApiResponse.Success(result.Message));
        }

        [HttpGet("my-requests")]
        [Authorize(Roles = "Intern")]
        public async Task<ActionResult<ApiResponse<List<AbsenceRequestDto>>>> GetMyAbsenceRequests()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse.Error<List<AbsenceRequestDto>>("Unauthorized"));

            var result = await _absenceService.GetMyAbsenceRequestsAsync(userId);

            if (!result.Success)
                return BadRequest(ApiResponse.Error<List<AbsenceRequestDto>>(result.Message));

            return Ok(ApiResponse.Success(result.Data));
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Supervisor,HR,Admin")]
        public async Task<ActionResult<ApiResponse<List<AbsenceRequestDto>>>> GetPendingRequests()
        {
            var supervisorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(supervisorId))
                return Unauthorized(ApiResponse.Error<List<AbsenceRequestDto>>("Unauthorized"));

            var result = await _absenceService.GetPendingRequestsAsync(supervisorId);

            if (!result.Success)
                return BadRequest(ApiResponse.Error<List<AbsenceRequestDto>>(result.Message));

            return Ok(ApiResponse.Success(result.Data));
        }

        [HttpGet("summary/{internId}")]
        [Authorize(Roles = "Supervisor,HR,Admin")]
        public async Task<ActionResult<ApiResponse<AbsenceSummaryDto>>> GetAbsenceSummary(int internId)
        {
            var result = await _absenceService.GetAbsenceSummaryAsync(internId);

            if (!result.Success)
                return BadRequest(ApiResponse.Error<AbsenceSummaryDto>(result.Message));

            return Ok(ApiResponse.Success(result.Data));
        }

        [HttpGet("department/{departmentId}")]
        [Authorize(Roles = "HR,Admin")]
        public async Task<ActionResult<ApiResponse<List<AbsenceRequestDto>>>> GetDepartmentAbsences(int departmentId)
        {
            var result = await _absenceService.GetDepartmentAbsencesAsync(departmentId);

            if (!result.Success)
                return BadRequest(ApiResponse.Error<List<AbsenceRequestDto>>(result.Message));

            return Ok(ApiResponse.Success(result.Data));
        }

        [HttpGet("check-limits")]
        [Authorize(Roles = "Intern")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckAbsenceLimits([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse.Error<bool>("Unauthorized"));

            // Note: You'll need to implement this method in your service
            // For now, returning a placeholder response
            return Ok(ApiResponse.Success(true, "Within absence limits"));
        }
    }
}