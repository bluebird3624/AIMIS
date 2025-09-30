using Interchée.Entities;

namespace Interchée.Repositories.Interfaces
{
    public interface IGradeRepository
    {
        Task<Grade?> GetBySubmissionIdAsync(int submissionId);
        Task<Grade> CreateOrUpdateAsync(Grade grade);
        Task<bool> ExistsForSubmissionAsync(int submissionId);
    }
}