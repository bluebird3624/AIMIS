using Interchée.DTOs;

namespace Interchée.Services.Interfaces
{
    public interface ISubmissionService
    {
        Task<SubmissionResponseDto> SubmitAssignmentAsync(int assignmentId, SubmitAssignmentDto dto, Guid internId);
        Task<SubmissionResponseDto?> GetSubmissionByAssignmentAndInternAsync(int assignmentId, Guid internId);
        Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsByAssignmentAsync(int assignmentId);
        Task<SubmissionResponseDto> UpdateSubmissionAsync(int submissionId, UpdateSubmissionDto dto, Guid internId);
        Task DeleteSubmissionAsync(int submissionId, Guid currentUserId);
    }
}