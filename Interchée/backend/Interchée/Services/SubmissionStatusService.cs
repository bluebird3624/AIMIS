using Interchée.Data;
using Interchée.Entities;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Services
{
    public class SubmissionStatusService(AppDbContext db)
    {
        private readonly AppDbContext _db = db;

        /// <summary>Automatically update submission status based on commit message</summary>
        public async Task UpdateSubmissionStatusFromCommit(AssignmentSubmission submission, string? commitMessage)
        {
            if (string.IsNullOrEmpty(commitMessage)) return;

            var message = commitMessage.ToLowerInvariant();

            // Automatic status detection from commit messages
            if (message.Contains("final") || message.Contains("submit") || message.Contains("complete"))
            {
                submission.Status = "Submitted";
                submission.SubmittedAt = DateTime.UtcNow;
            }
            else if (submission.Status == "InProgress" && HasSignificantActivity(message))
            {
                // Keep as InProgress for normal development commits
                submission.Status = "InProgress";
            }

            await _db.SaveChangesAsync();
        }

        /// <summary>Check if commit represents significant work</summary>
        private static bool HasSignificantActivity(string message)
        {
            var insignificantKeywords = new[] { "merge", "update readme", "typo", "fix typo", "minor" };
            return !insignificantKeywords.Any(keyword => message.Contains(keyword));
        }

        /// <summary>Auto-close assignment if all students have submitted</summary>
        public async Task AutoCloseAssignmentIfAllSubmitted(long assignmentId)
        {
            var assignment = await _db.Assignments
                .Include(a => a.Assignees)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null || assignment.Status != "Assigned") return;

            var totalAssignees = assignment.Assignees.Count;
            var submittedCount = await _db.AssignmentSubmissions
                .CountAsync(s => s.AssignmentId == assignmentId && s.Status == "Submitted");

            // If all assigned students have submitted, auto-close
            if (totalAssignees > 0 && submittedCount >= totalAssignees)
            {
                assignment.Status = "Closed";
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>Auto-close assignment if all submissions are reviewed</summary>
        public async Task AutoCloseAssignmentIfAllReviewed(long assignmentId)
        {
            var assignment = await _db.Assignments
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null || assignment.Status != "Assigned") return;

            var totalSubmissions = await _db.AssignmentSubmissions
                .CountAsync(s => s.AssignmentId == assignmentId);
            var reviewedCount = await _db.AssignmentSubmissions
                .CountAsync(s => s.AssignmentId == assignmentId && s.Status == "Reviewed");

            // If all submissions are reviewed, auto-close
            if (totalSubmissions > 0 && reviewedCount >= totalSubmissions)
            {
                assignment.Status = "Closed";
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>Update submission status to Submitted automatically</summary>
        public async Task MarkAsSubmitted(long submissionId)
        {
            var submission = await _db.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission != null)
            {
                submission.Status = "Submitted";
                submission.SubmittedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>Update submission status to Reviewed automatically</summary>
        public async Task MarkAsReviewed(long submissionId)
        {
            var submission = await _db.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission != null)
            {
                submission.Status = "Reviewed";
                await _db.SaveChangesAsync();
            }
        }
    }
}