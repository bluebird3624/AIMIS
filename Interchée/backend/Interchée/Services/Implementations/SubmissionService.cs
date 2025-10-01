using Interchée.DTOs;
using Interchée.Entities;
using Interchée.Repositories.Interfaces;
using Interchée.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Services.Implementations
{
    public class SubmissionService(
        ISubmissionRepository submissionRepo,
        IAssignmentRepository assignmentRepo,
        IGradeRepository gradeRepo,
        IGitIntegrationService gitService) : ISubmissionService
    {
        private readonly ISubmissionRepository _submissionRepo = submissionRepo;
        private readonly IAssignmentRepository _assignmentRepo = assignmentRepo;
        private readonly IGradeRepository _gradeRepo = gradeRepo;
        private readonly IGitIntegrationService _gitService = gitService;

        public async Task<SubmissionResponseDto> SubmitAssignmentAsync(int assignmentId, SubmitAssignmentDto dto, Guid internId)
        {
            // Get assignment and check if it exists
            if (!await _assignmentRepo.ExistsAsync(assignmentId))
                throw new ArgumentException("Assignment not found");

            // Check if intern already submitted
            if (await _submissionRepo.HasInternSubmittedAsync(assignmentId, internId))
                throw new InvalidOperationException("Assignment already submitted");

            // Validate Git URL
            if (!await _gitService.ValidateGitUrlAsync(dto.GitRepositoryUrl))
                throw new ArgumentException("Invalid Git repository URL");

            // Get Git metadata
            var gitMetadata = await _gitService.GetRepositoryMetadataAsync(dto.GitRepositoryUrl);

            // Create submission
            var submission = new AssignmentSubmission
            {
                AssignmentId = assignmentId,
                InternId = internId,
                GitRepositoryUrl = dto.GitRepositoryUrl,
                Status = AssignmentStatus.Submitted,
                SubmittedAt = DateTime.UtcNow,
                LastCommitHash = gitMetadata.LastCommitHash,
                CommitHistoryJson = gitMetadata.CommitHistoryJson,
                BranchName = gitMetadata.DefaultBranch
            };

            var created = await _submissionRepo.CreateAsync(submission);

            return new SubmissionResponseDto
            {
                Id = created.Id,
                GitRepositoryUrl = created.GitRepositoryUrl,
                SubmittedAt = created.SubmittedAt,
                Status = created.Status.ToString(),
                LastCommitHash = created.LastCommitHash,
                BranchName = created.BranchName,
                InternName = created.Intern != null
                    ? $"{created.Intern.FirstName} {created.Intern.LastName}"
                    : "Unknown Intern",
                Grade = null
            };
        }

        public async Task<SubmissionResponseDto?> GetSubmissionByAssignmentAndInternAsync(int assignmentId, Guid internId)
        {
            var submission = await _submissionRepo.GetByAssignmentAndInternAsync(assignmentId, internId);
            if (submission == null) return null;

            return MapToSubmissionResponseDto(submission);
        }

        public async Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsByAssignmentAsync(int assignmentId)
        {
            var submissions = await _submissionRepo.GetByAssignmentAsync(assignmentId);
            return submissions.Select(MapToSubmissionResponseDto);
        }

        public async Task<SubmissionResponseDto> UpdateSubmissionAsync(int submissionId, UpdateSubmissionDto dto, Guid internId)
        {
            var submission = await _submissionRepo.GetByIdAsync(submissionId) ?? throw new ArgumentException("Submission not found");
            if (submission.InternId != internId)
                throw new UnauthorizedAccessException("You can only update your own submissions");

            if (!await _gitService.ValidateGitUrlAsync(dto.GitRepositoryUrl))
                throw new ArgumentException("Invalid Git repository URL");

            var gitMetadata = await _gitService.GetRepositoryMetadataAsync(dto.GitRepositoryUrl);

            submission.GitRepositoryUrl = dto.GitRepositoryUrl;
            submission.LastCommitHash = gitMetadata.LastCommitHash;
            submission.CommitHistoryJson = gitMetadata.CommitHistoryJson;
            submission.BranchName = gitMetadata.DefaultBranch;
            submission.SubmittedAt = DateTime.UtcNow;

            await _submissionRepo.UpdateAsync(submission);
            return MapToSubmissionResponseDto(submission);
        }

        public async Task DeleteSubmissionAsync(int submissionId, Guid currentUserId)
        {
            var submission = await _submissionRepo.GetByIdAsync(submissionId) ?? throw new ArgumentException("Submission not found");
            if (submission.InternId != currentUserId)
                throw new UnauthorizedAccessException("You can only delete your own submissions");

            await _submissionRepo.DeleteAsync(submissionId);
        }

        private static SubmissionResponseDto MapToSubmissionResponseDto(AssignmentSubmission submission)
        {
            GradeResponseDto? gradeDto = null;
            if (submission.Grade != null && submission.Grade.GradedBy != null)
            {
                gradeDto = new GradeResponseDto
                {
                    Id = submission.Grade.Id,
                    Score = submission.Grade.Score,
                    Comments = submission.Grade.Comments,
                    GradedAt = submission.Grade.GradedAt,
                    GradedBy = $"{submission.Grade.GradedBy.FirstName} {submission.Grade.GradedBy.LastName}"
                };
            }

            return new SubmissionResponseDto
            {
                Id = submission.Id,
                GitRepositoryUrl = submission.GitRepositoryUrl,
                SubmittedAt = submission.SubmittedAt,
                Status = submission.Status.ToString(),
                LastCommitHash = submission.LastCommitHash,
                BranchName = submission.BranchName,
                InternName = submission.Intern != null
                    ? $"{submission.Intern.FirstName} {submission.Intern.LastName}"
                    : "Unknown Intern",
                Grade = gradeDto
            };
        }
    }
}