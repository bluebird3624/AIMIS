using Interchée.DTOs.Submissions;
using Interchée.DTOs.Grades;
using Interchée.DTOs;

namespace Interchée.Services.Interfaces
{
    public interface IAssignmentService
    {
        // ========== ASSIGNMENT CRUD ==========
        Task<AssignmentResponseDto> CreateAssignmentAsync(CreateAssignmentDto dto, Guid supervisorId);
        Task<AssignmentResponseDto?> GetAssignmentAsync(int id);
        Task<IEnumerable<AssignmentResponseDto>> GetAllAssignmentsAsync();
        Task<IEnumerable<AssignmentResponseDto>> GetAssignmentsByDepartmentAsync(int departmentId);
        Task<IEnumerable<AssignmentResponseDto>> GetAssignmentsForInternAsync(Guid internId);
        Task<AssignmentResponseDto> UpdateAssignmentAsync(int id, UpdateAssignmentDto dto);
        Task DeleteAssignmentAsync(int id);

        // ========== SUBMISSION OPERATIONS ==========
        Task<SubmissionResponseDto> SubmitAssignmentAsync(int assignmentId, SubmitAssignmentDto dto, Guid internId);
        Task<SubmissionResponseDto?> GetSubmissionAsync(int submissionId);
        Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsByAssignmentAsync(int assignmentId);

        // ========== GRADING OPERATIONS ==========
        Task<GradeResponseDto> GradeSubmissionAsync(int submissionId, GradeSubmissionDto dto, Guid supervisorId);
        Task<GradeResponseDto?> GetGradeAsync(int submissionId);
    }
}