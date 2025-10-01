// Controllers/AbsenceRequestsController.cs (updated)
using Interchée.Common;
using Interchée.Dtos;
using Interchée.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Interchée.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AbsenceRequestController(IAbsenceService absenceService) : ControllerBase
    {
        private readonly IAbsenceService _absenceService = absenceService;

        [HttpPost]
        [Authorize(Roles = "Intern")]
        public async Task<ActionResult<ApiResponse<AbsenceRequestDto>>> CreateAbsenceRequest(CreateAbsenceRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse<AbsenceRequestDto>
                {
                    Success = false,
                    Errors = ["Unauthorized"]
                });

            var result = await _absenceService.CreateAbsenceRequestAsync(request, userId);

            if (!result.Success)
                return BadRequest(new ApiResponse<AbsenceRequestDto>
                {
                    Success = false,
                    Errors = [result.Message ?? "Unknown error"]
                });

            return Ok(new ApiResponse<AbsenceRequestDto>
            {
                Success = true,
                Data = result.Data,
                Message = result.Message ?? string.Empty // Fix CS8601: ensure Message is not null
            });
        }

        [HttpGet("my-requests")]
        [Authorize(Roles = "Intern")]
        public async Task<ActionResult<ApiResponse<List<AbsenceRequestDto>>>> GetMyAbsenceRequests()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse<List<AbsenceRequestDto>>
                {
                    Success = false,
                    Errors = ["Unauthorized"]
                });

            var result = await _absenceService.GetMyAbsenceRequestsAsync(userId);

            if (!result.Success)
                return BadRequest(new ApiResponse<List<AbsenceRequestDto>>
                {
                    Success = false,
                    Errors = [result.Message ?? "Unknown error"]
                });

            return Ok(new ApiResponse<List<AbsenceRequestDto>>
            {
                Success = true,
                Data = result.Data
            });
        }
    }
}