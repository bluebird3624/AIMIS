using Interchée.Data;
using Interchée.Entities;
using Interchée.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Repositories.Implementations
{
    public class AssignmentRepository(AppDbContext context) : IAssignmentRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<Assignment?> GetByIdAsync(int id)
        {
            return await _context.Assignments
                .Include(a => a.CreatedBy)
                .Include(a => a.Department)
                .Include(a => a.Attachments)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Assignment>> GetAllAsync()
        {
            return await _context.Assignments
                .Include(a => a.CreatedBy)
                .Include(a => a.Department)
                .Include(a => a.Submissions)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Assignment>> GetByDepartmentAsync(int departmentId)
        {
            return await _context.Assignments
                .Include(a => a.CreatedBy)
                .Include(a => a.Department)
                .Where(a => a.DepartmentId == departmentId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Assignment>> GetBySupervisorAsync(Guid supervisorId)
        {
            return await _context.Assignments
                .Include(a => a.CreatedBy)
                .Include(a => a.Department)
                .Where(a => a.CreatedById == supervisorId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Assignment>> GetAssignmentsForInternAsync(Guid internId)
        {
            var internDepartment = await _context.DepartmentRoleAssignments
                .Where(dra => dra.UserId == internId && (dra.RoleName == "Intern" || dra.RoleName == "Attache"))
                .Select(dra => dra.DepartmentId)
                .FirstOrDefaultAsync();

            if (internDepartment == 0) return [];

            return await _context.Assignments
                .Include(a => a.CreatedBy)
                .Include(a => a.Department)
                .Where(a => a.DepartmentId == internDepartment)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Assignment> CreateAsync(Assignment assignment)
        {
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();
            return assignment;
        }

        public async Task UpdateAsync(Assignment assignment)
        {
            _context.Assignments.Update(assignment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var assignment = await GetByIdAsync(id);
            if (assignment != null)
            {
                _context.Assignments.Remove(assignment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Assignments.AnyAsync(a => a.Id == id);
        }
    }
}