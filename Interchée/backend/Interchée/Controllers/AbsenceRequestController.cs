// Controllers/AbsenceRequestsController.cs (updated)
using Interchee.Common;
using Interchée.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[HttpPost]
[Authorize(Roles = "Intern")]
public async Task<ActionResult<ApiResponse<AbsenceRequestDto>>> CreateAbsenceRequest(CreateAbsenceRequestDto request)
{
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
        return Unauthorized(new ApiResponse<AbsenceRequestDto>
        {
            Success = false,
            Errors = new List<string> { "Unauthorized" }
        });

    var result = await _absenceService.CreateAbsenceRequestAsync(request, userId);

    if (!result.Success)
        return BadRequest(new ApiResponse<AbsenceRequestDto>
        {
            Success = false,
            Errors = new List<string> { result.Message }
        });

    return Ok(new ApiResponse<AbsenceRequestDto>
    {
        Success = true,
        Data = result.Data,
        Message = result.Message
    });
}

[HttpGet("my-requests")]
[Authorize(Roles = "Intern")]
public async Task<ActionResult<ApiResponse<List<AbsenceRequestDto>>>> GetMyAbsenceRequests()
{
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
        return Unauthorized(new ApiResponse<List<AbsenceRequestDto>>
        {
            Success = false,
            Errors = new List<string> { "Unauthorized" }
        });

    var result = await _absenceService.GetMyAbsenceRequestsAsync(userId);

    if (!result.Success)
        return BadRequest(new ApiResponse<List<AbsenceRequestDto>>
        {
            Success = false,
            Errors = new List<string> { result.Message }
        });

    return Ok(new ApiResponse<List<AbsenceRequestDto>>
    {
        Success = true,
        Data = result.Data
    });
}