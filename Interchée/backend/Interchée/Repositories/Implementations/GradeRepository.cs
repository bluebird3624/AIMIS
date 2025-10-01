using Interchée.Data;
using Interchée.Entities;
using Interchée.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Repositories.Implementations
{
    public class GradeRepository(AppDbContext context) : IGradeRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<Grade?> GetBySubmissionIdAsync(int submissionId)
        {
            return await _context.Grades
                .Include(g => g.GradedBy)
                .Include(g => g.Submission)
                .FirstOrDefaultAsync(g => g.SubmissionId == submissionId);
        }

        public async Task<Grade> CreateOrUpdateAsync(Grade grade)
        {
            var existingGrade = await GetBySubmissionIdAsync(grade.SubmissionId);

            if (existingGrade != null)
            {
                // Update existing grade
                existingGrade.Score = grade.Score;
                existingGrade.Comments = grade.Comments;
                existingGrade.RubricEvaluationJson = grade.RubricEvaluationJson;
                existingGrade.GradedAt = DateTime.UtcNow;
                existingGrade.GradedById = grade.GradedById;

                _context.Grades.Update(existingGrade);
            }
            else
            {
                // Create new grade
                _context.Grades.Add(grade);
            }

            await _context.SaveChangesAsync();
            return grade;
        }

        public async Task<bool> ExistsForSubmissionAsync(int submissionId)
        {
            return await _context.Grades.AnyAsync(g => g.SubmissionId == submissionId);
        }
    }
}