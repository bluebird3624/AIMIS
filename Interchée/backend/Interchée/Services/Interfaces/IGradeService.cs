using Interchée.DTOs;

namespace Interchée.Services.Interfaces
{
    public interface IGradeService
    {
        Task<GradeResponseDto> GradeSubmissionAsync(int submissionId, GradeSubmissionDto dto, Guid supervisorId);
        Task<GradeResponseDto?> GetGradeAsync(int submissionId);
        Task<GradeResponseDto> UpdateGradeAsync(int submissionId, GradeSubmissionDto dto, Guid supervisorId);
    }
}