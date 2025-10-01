using Interchée.DTOs;
using Interchée.Entities;
using Interchée.Repositories.Interfaces;
using Interchée.Services.Interfaces;

namespace Interchée.Services.Implementations
{
    public class GradeService(IGradeRepository gradeRepo, ISubmissionRepository submissionRepo) : IGradeService
    {
        private readonly IGradeRepository _gradeRepo = gradeRepo;
        private readonly ISubmissionRepository _submissionRepo = submissionRepo;

        public async Task<GradeResponseDto> GradeSubmissionAsync(int submissionId, GradeSubmissionDto dto, Guid supervisorId)
        {
            var submission = await _submissionRepo.GetByIdAsync(submissionId)
                ?? throw new ArgumentException("Submission not found");

            var grade = new Grade
            {
                SubmissionId = submissionId,
                Score = dto.Score,
                Comments = dto.Comments,
                GradedById = supervisorId,
                GradedAt = DateTime.UtcNow
            };

            var created = await _gradeRepo.CreateOrUpdateAsync(grade);

            submission.Status = AssignmentStatus.Graded;
            await _submissionRepo.UpdateAsync(submission);

            return new GradeResponseDto
            {
                Id = created.Id,
                Score = created.Score,
                Comments = created.Comments,
                GradedAt = created.GradedAt,
                GradedBy = created.GradedBy != null
                    ? $"{created.GradedBy.FirstName} {created.GradedBy.LastName}"
                    : "Unknown Grader"
            };
        }

        public async Task<GradeResponseDto?> GetGradeAsync(int submissionId)
        {
            var grade = await _gradeRepo.GetBySubmissionIdAsync(submissionId);
            return grade == null ? null : new GradeResponseDto
            {
                Id = grade.Id,
                Score = grade.Score,
                Comments = grade.Comments,
                GradedAt = grade.GradedAt,
                GradedBy = grade.GradedBy != null
                    ? $"{grade.GradedBy.FirstName} {grade.GradedBy.LastName}"
                    : "Unknown Grader"
            };
        }

        public async Task<GradeResponseDto> UpdateGradeAsync(int submissionId, GradeSubmissionDto dto, Guid supervisorId)
        {
            var grade = await _gradeRepo.GetBySubmissionIdAsync(submissionId)
                ?? throw new ArgumentException("Grade not found");

            grade.Score = dto.Score;
            grade.Comments = dto.Comments;
            grade.GradedById = supervisorId;
            grade.GradedAt = DateTime.UtcNow;

            await _gradeRepo.CreateOrUpdateAsync(grade);

            return new GradeResponseDto
            {
                Id = grade.Id,
                Score = grade.Score,
                Comments = grade.Comments,
                GradedAt = grade.GradedAt,
                GradedBy = grade.GradedBy != null
                    ? $"{grade.GradedBy.FirstName} {grade.GradedBy.LastName}"
                    : "Unknown Grader"
            };
        }
    }
}