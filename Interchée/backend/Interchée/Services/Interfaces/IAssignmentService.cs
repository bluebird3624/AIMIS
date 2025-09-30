using Interchée.DTOs.Assignments;
using Interchée.DTOs.Submissions;
using Interchée.DTOs.Grades;
using Interchée.Entities;

namespace Interchée.Services.Interfaces
{
    public interface IAssignmentService
    {
        Task<AssignmentResponseDto> CreateAssignmentAsync(CreateAssignmentDto dto, Guid supervisorId);
        Task<AssignmentResponseDto?> GetAssignmentAsync(int id);
        Task<IEnumerable<AssignmentResponseDto>> GetAssignmentsByDepartmentAsync(int departmentId);
        Task<IEnumerable<AssignmentResponseDto>> GetAssignmentsForInternAsync(Guid internId);

        Task<SubmissionResponseDto> SubmitAssignmentAsync(int assignmentId, SubmitAssignmentDto dto, Guid internId);
        Task<SubmissionResponseDto?> GetSubmissionAsync(int submissionId);
        Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsByAssignmentAsync(int assignmentId);

        // Grading operations
        Task<GradeResponseDto> GradeSubmissionAsync(int submissionId, GradeSubmissionDto dto, Guid supervisorId);
        Task<GradeResponseDto?> GetGradeAsync(int submissionId);
    }
}