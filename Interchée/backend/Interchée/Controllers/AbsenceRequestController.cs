using Interchée.Controllers;
using Interchée.Dtos;
using Interchée.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Interchee.Controllers
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
                return Unauthorized();

            var result = await _absenceService.CreateAbsenceRequestAsync(request, userId);

            if (!result.Success)
                return BadRequest(ApiResponse<AbsenceRequestDto>.Error(result.Message));

            return Ok(ApiResponse<AbsenceRequestDto>.Success(result.Data, result.Message));
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Supervisor,HR,Admin")]
        public async Task<ActionResult<ApiResponse>> ApproveAbsenceRequest(int id, ApproveAbsenceRequestDto request)
        {
            var approverId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(approverId))
                return Unauthorized();

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
                return Unauthorized();

            var result = await _absenceService.GetMyAbsenceRequestsAsync(userId);

            if (!result.Success)
                return BadRequest(ApiResponse<List<AbsenceRequestDto>>.Error(result.Message));

            return Ok(ApiResponse<List<AbsenceRequestDto>>.Success(result.Data));
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Supervisor,HR,Admin")]
        public async Task<ActionResult<ApiResponse<List<AbsenceRequestDto>>>> GetPendingRequests()
        {
            var supervisorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(supervisorId))
                return Unauthorized();

            var result = await _absenceService.GetPendingRequestsAsync(supervisorId);

            if (!result.Success)
                return BadRequest(ApiResponse<List<AbsenceRequestDto>>.Error(result.Message));

            return Ok(ApiResponse<List<AbsenceRequestDto>>.Success(result.Data));
        }

        [HttpGet("summary/{internId}")]
        [Authorize(Roles = "Supervisor,HR,Admin")]
        public async Task<ActionResult<ApiResponse<AbsenceSummaryDto>>> GetAbsenceSummary(int internId)
        {
            var result = await _absenceService.GetAbsenceSummaryAsync(internId);

            if (!result.Success)
                return BadRequest(ApiResponse<AbsenceSummaryDto>.Error(result.Message));

            return Ok(ApiResponse<AbsenceSummaryDto>.Success(result.Data));
        }

        [HttpGet("department/{departmentId}")]
        [Authorize(Roles = "HR,Admin")]
        public async Task<ActionResult<ApiResponse<List<AbsenceRequestDto>>>> GetDepartmentAbsences(int departmentId)
        {
            var result = await _absenceService.GetDepartmentAbsencesAsync(departmentId);

            if (!result.Success)
                return BadRequest(ApiResponse<List<AbsenceRequestDto>>.Error(result.Message));

            return Ok(ApiResponse<List<AbsenceRequestDto>>.Success(result.Data));
        }

        [HttpGet("check-limits")]
        [Authorize(Roles = "Intern")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckAbsenceLimits([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Get intern ID from user ID
            var intern = await _absenceService.GetMyAbsenceRequestsAsync(userId);
            if (!intern.Success)
                return BadRequest(ApiResponse<bool>.Error("Intern not found"));

            var result = await _absenceService.CheckAbsenceLimitAsync(1, startDate, endDate); // You'll need to adjust this

            if (!result.Success)
                return Ok(ApiResponse<bool>.Success(false, result.Message));

            return Ok(ApiResponse<bool>.Success(true, "Within absence limits"));
        }
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public static ApiResponse Success(string message = "")
        {
            return new() { Success = true, Message = message };
        }

        public static ApiResponse Error(string message) => new() { Success = false, Message = message };
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T Data { get; set; }

        public static ApiResponse<T> Success(T data, string message = "") => new() { Success = true, Data = data, Message = message };
        public static new ApiResponse<T> Error(string message) => new() { Success = false, Message = message };
    }
}