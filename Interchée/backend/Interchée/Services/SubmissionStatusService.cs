using Interchée.Data;
using Interchée.Entities;
using Interchée.Entities.Enums; 
using Microsoft.EntityFrameworkCore;

namespace Interchée.Services
{
    public class SubmissionStatusService(AppDbContext db)
    {
        private readonly AppDbContext _db = db;

        public async Task<bool> AddCommitToSubmission(AssignmentSubmission submission, string? commitMessage, string commitSha)
        {
            // 🚫 Check if submission is locked (closed or reviewed)
            if (submission.Status == SubmissionStatus.Reviewed) 
            {
                return false; // Cannot add commits to reviewed submissions
            }

            // 🚫 Check if assignment is closed
            var assignment = await _db.Assignments
                .FirstOrDefaultAsync(a => a.Id == submission.AssignmentId);

            if (assignment?.Status == AssignmentStatus.Closed) 
            {
                return false; // Cannot add commits to submissions in closed assignments
            }

            // Check if commit already exists
            var commitExists = await _db.SubmissionCommits
                .AnyAsync(c => c.SubmissionId == submission.Id && c.Sha == commitSha);

            if (commitExists)
            {
                return true; // Commit already exists, but that's okay
            }

            // Add the commit
            var commit = new SubmissionCommit
            {
                SubmissionId = submission.Id,
                Sha = commitSha,
                Message = commitMessage?.Trim(),
                CommittedAt = DateTime.UtcNow
            };

            // Update latest commit SHA
            submission.LatestCommitSha = commitSha;

            _db.SubmissionCommits.Add(commit);
            await _db.SaveChangesAsync();

            return true;
        }

        /// <summary>Update submission status to Reviewed (after grading)</summary>
        public async Task<bool> MarkAsReviewed(long submissionId)
        {
            var submission = await _db.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null) return false;

            // Check assignment status directly
            var assignment = await _db.Assignments
                .FirstOrDefaultAsync(a => a.Id == submission.AssignmentId);

            if (assignment?.Status == AssignmentStatus.Closed) 
            {
                // Cannot change status of submission in closed assignment
                return false;
            }

            // Only allow marking as reviewed if currently submitted
            if (submission.Status == SubmissionStatus.Submitted) 
            {
                submission.Status = SubmissionStatus.Reviewed; 
                await _db.SaveChangesAsync();
                return true;
            }

            return false;
        }

        /// <summary>Check if submission is allowed for the assignment</summary>
        public async Task<bool> CanSubmitToAssignment(long assignmentId)
        {
            var assignment = await _db.Assignments
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            // Cannot submit if assignment is closed/archived or doesn't exist
            return assignment != null &&
                   assignment.Status != AssignmentStatus.Closed ; 
        }

        /// <summary>Check if commits can be added to submission</summary>
        public async Task<bool> CanAddCommitsToSubmission(long submissionId)
        {
            var submission = await _db.AssignmentSubmissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null) return false;

            // Cannot add commits if submission is reviewed or assignment is closed
            return submission.Status != SubmissionStatus.Reviewed && 
                   submission.Assignment?.Status != AssignmentStatus.Closed; 
        }
    }
}