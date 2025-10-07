using Interchée.Contracts.Assignments;
using Interchée.Data;
using Interchée.Entities;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Services
{
    public class AssignmentStatusService(AppDbContext db)
    {
        private readonly AppDbContext _db = db;

        /// <summary>Automatically update assignment status based on deadlines and submissions</summary>
        public async Task AutoUpdateAssignmentStatus(Assignment assignment)
        {
            // Auto-close if deadline passed and all submissions are reviewed
            if (assignment.DueAt.HasValue &&
                assignment.DueAt.Value < DateTime.UtcNow &&
                assignment.Status == "Assigned")
            {
                var totalSubmissions = await _db.AssignmentSubmissions
                    .CountAsync(s => s.AssignmentId == assignment.Id);
                var reviewedCount = await _db.AssignmentSubmissions
                    .CountAsync(s => s.AssignmentId == assignment.Id && s.Status == "Reviewed");

                // If all submissions are reviewed or no submissions exist, auto-close
                if (totalSubmissions == 0 || reviewedCount >= totalSubmissions)
                {
                    assignment.Status = "Closed";
                }
            }

            // Auto-archive if closed for more than 30 days
            if (assignment.Status == "Closed" &&
                assignment.CreatedAt.AddDays(30) < DateTime.UtcNow)
            {
                assignment.Status = "Archived";
            }
        }

        /// <summary>Background service method to auto-update expired assignments</summary>
        public async Task AutoUpdateExpiredAssignments()
        {
            var expiredAssignments = await _db.Assignments
                .Where(a => a.DueAt.HasValue &&
                           a.DueAt.Value < DateTime.UtcNow &&
                           a.Status == "Assigned")
                .ToListAsync();

            foreach (var assignment in expiredAssignments)
            {
                await AutoUpdateAssignmentStatus(assignment);
            }

            await _db.SaveChangesAsync();
        }

        /// <summary>Get assignment progress summary</summary>
        public async Task<AssignmentProgressDto?> GetAssignmentProgress(long assignmentId)
        {
            var assignment = await _db.Assignments
                .Include(a => a.Assignees)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null) return null;

            var totalAssignees = assignment.Assignees.Count;
            var submittedCount = await _db.AssignmentSubmissions
                .CountAsync(s => s.AssignmentId == assignmentId && s.Status == "Submitted");
            var reviewedCount = await _db.AssignmentSubmissions
                .CountAsync(s => s.AssignmentId == assignmentId && s.Status == "Reviewed");
            var inProgressCount = await _db.AssignmentSubmissions
                .CountAsync(s => s.AssignmentId == assignmentId && s.Status == "InProgress");

            return new AssignmentProgressDto(
                TotalAssignees: totalAssignees,
                SubmittedCount: submittedCount,
                ReviewedCount: reviewedCount,
                InProgressCount: inProgressCount,
                NotStartedCount: totalAssignees - (submittedCount + reviewedCount + inProgressCount),
                SubmissionRate: totalAssignees > 0 ? (double)(submittedCount + reviewedCount) / totalAssignees * 100 : 0,
                ReviewRate: (submittedCount + reviewedCount) > 0 ? (double)reviewedCount / (submittedCount + reviewedCount) * 100 : 0
            );
        }
    }

    
}