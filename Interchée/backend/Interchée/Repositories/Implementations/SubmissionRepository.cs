using Interchée.Data;
using Interchée.Entities;
using Interchée.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Repositories.Implementations
{
    public class SubmissionRepository(AppDbContext context) : ISubmissionRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<AssignmentSubmission?> GetByIdAsync(int id)
        {
            return await _context.AssignmentSubmissions
                .Include(s => s.Assignment)
                .Include(s => s.Intern)
                .Include(s => s.Grade)
                .Include(s => s.Feedbacks)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<AssignmentSubmission?> GetByAssignmentAndInternAsync(int assignmentId, Guid internId)
        {
            return await _context.AssignmentSubmissions
                .Include(s => s.Assignment)
                .Include(s => s.Intern)
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.InternId == internId);
        }

        public async Task<IEnumerable<AssignmentSubmission>> GetByAssignmentAsync(int assignmentId)
        {
            return await _context.AssignmentSubmissions
                .Include(s => s.Intern)
                .Include(s => s.Grade)
                .Where(s => s.AssignmentId == assignmentId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<AssignmentSubmission>> GetByInternAsync(Guid internId)
        {
            return await _context.AssignmentSubmissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Department)
                .Include(s => s.Grade)
                .Where(s => s.InternId == internId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }

        public async Task<AssignmentSubmission> CreateAsync(AssignmentSubmission submission)
        {
            _context.AssignmentSubmissions.Add(submission);
            await _context.SaveChangesAsync();
            return submission;
        }

        public async Task UpdateAsync(AssignmentSubmission submission)
        {
            _context.AssignmentSubmissions.Update(submission);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var submission = await GetByIdAsync(id);
            if (submission != null)
            {
                _context.AssignmentSubmissions.Remove(submission);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasInternSubmittedAsync(int assignmentId, Guid internId)
        {
            return await _context.AssignmentSubmissions
                .AnyAsync(s => s.AssignmentId == assignmentId && s.InternId == internId);
        }
    }
}