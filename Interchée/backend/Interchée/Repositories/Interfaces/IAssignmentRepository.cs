using Interchée.Entities;

namespace Interchée.Repositories.Interfaces
{
    public interface IAssignmentRepository
    {
        Task<Assignment?> GetByIdAsync(int id);
        Task<IEnumerable<Assignment>> GetAllAsync();
        Task<IEnumerable<Assignment>> GetByDepartmentAsync(int departmentId);
        Task<IEnumerable<Assignment>> GetBySupervisorAsync(Guid supervisorId);
        Task<IEnumerable<Assignment>> GetAssignmentsForInternAsync(Guid internId);
        Task<Assignment> CreateAsync(Assignment assignment);
        Task UpdateAsync(Assignment assignment);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}