using Interchée.Contracts.Assignments;
using Interchée.Data;
using Interchée.Entities;
using Interchée.Entities.Enums; 
using Microsoft.EntityFrameworkCore;

namespace Interchée.Services
{
    public class AssignmentStatusService(AppDbContext db)
    {
        private readonly AppDbContext _db = db;

        /// <summary>Automatically update assignment status based on deadlines</summary>
        public async Task AutoUpdateAssignmentStatus(Assignment assignment)
        {
            // Auto-close immediately when deadline passes, regardless of submissions
            if (assignment.DueAt.HasValue &&
                assignment.DueAt.Value < DateTime.UtcNow &&
                assignment.Status == AssignmentStatus.Assigned) // ✅ ENUM
            {
                // CHANGED: Close assignment immediately when due date passes
                assignment.Status = AssignmentStatus.Closed; // ✅ ENUM
                await _db.SaveChangesAsync();
                return;
            }

            // Auto-archive if closed for more than 30 days
            if (assignment.Status == AssignmentStatus.Closed && // ✅ ENUM
                assignment.CreatedAt.AddDays(30) < DateTime.UtcNow)
            {
                assignment.Status = AssignmentStatus.Archived; // ✅ ENUM
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>Background service method to auto-update expired assignments</summary>
        public async Task AutoUpdateExpiredAssignments()
        {
            var expiredAssignments = await _db.Assignments
                .Where(a => a.DueAt.HasValue &&
                           a.DueAt.Value < DateTime.UtcNow &&
                           a.Status == AssignmentStatus.Assigned) // ✅ ENUM
                .ToListAsync();

            foreach (var assignment in expiredAssignments)
            {
                // Close assignment immediately without checking submissions
                assignment.Status = AssignmentStatus.Closed; // ✅ ENUM
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
                .CountAsync(s => s.AssignmentId == assignmentId && s.Status == SubmissionStatus.Submitted); 
            var reviewedCount = await _db.AssignmentSubmissions
                .CountAsync(s => s.AssignmentId == assignmentId && s.Status == SubmissionStatus.Reviewed); 

            // 🚫 REMOVED: InProgressCount - no longer exists
            var notStartedCount = totalAssignees - (submittedCount + reviewedCount);

            return new AssignmentProgressDto(
                TotalAssignees: totalAssignees,
                SubmittedCount: submittedCount,
                ReviewedCount: reviewedCount,
              //  InProgressCount: 0, 
                NotStartedCount: notStartedCount,
                SubmissionRate: totalAssignees > 0 ? (double)(submittedCount + reviewedCount) / totalAssignees * 100 : 0,
                ReviewRate: (submittedCount + reviewedCount) > 0 ? (double)reviewedCount / (submittedCount + reviewedCount) * 100 : 0
            );
        }

        /// <summary>Check and close assignment if due date has passed</summary>
        public async Task<bool> CheckAndCloseIfDueDatePassed(long assignmentId)
        {
            var assignment = await _db.Assignments
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null) return false;

            if (assignment.DueAt.HasValue &&
                assignment.DueAt.Value < DateTime.UtcNow &&
                assignment.Status == AssignmentStatus.Assigned) 
            {
                assignment.Status = AssignmentStatus.Closed; 
                await _db.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}