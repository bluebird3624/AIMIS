using Interchée.Entities;

namespace Interchée.Repositories.Interfaces
{
    public interface ISubmissionRepository
    {
        Task<AssignmentSubmission?> GetByIdAsync(int id);
        Task<AssignmentSubmission?> GetByAssignmentAndInternAsync(int assignmentId, Guid internId);
        Task<IEnumerable<AssignmentSubmission>> GetByAssignmentAsync(int assignmentId);
        Task<IEnumerable<AssignmentSubmission>> GetByInternAsync(Guid internId);
        Task<AssignmentSubmission> CreateAsync(AssignmentSubmission submission);
        Task UpdateAsync(AssignmentSubmission submission);
        Task DeleteAsync(int id);
        Task<bool> HasInternSubmittedAsync(int assignmentId, Guid internId);
    }
}