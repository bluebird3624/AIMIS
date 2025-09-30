using Interchée.DTOs.Assignments;
using Interchée.DTOs.Submissions;
using Interchée.DTOs.Grades;
using Interchée.Entities;
using Interchée.Repositories.Interfaces;
using Interchée.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Services.Implementations
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IAssignmentRepository _assignmentRepo;
        private readonly ISubmissionRepository _submissionRepo;
        private readonly IGradeRepository _gradeRepo;
        private readonly IGitIntegrationService _gitService;

        public AssignmentService(
            IAssignmentRepository assignmentRepo,
            ISubmissionRepository submissionRepo,
            IGradeRepository gradeRepo,
            IGitIntegrationService gitService)
        {
            _assignmentRepo = assignmentRepo;
            _submissionRepo = submissionRepo;
            _gradeRepo = gradeRepo;
            _gitService = gitService;
        }

        public async Task<AssignmentResponseDto> CreateAssignmentAsync(CreateAssignmentDto dto, Guid supervisorId)
        {
            var assignment = new Assignment
            {
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                DepartmentId = dto.DepartmentId,
                CreatedById = supervisorId,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _assignmentRepo.CreateAsync(assignment);

            return new AssignmentResponseDto
            {
                Id = created.Id,
                Title = created.Title,
                Description = created.Description,
                DueDate = created.DueDate,
                CreatedAt = created.CreatedAt,
                CreatedBy = $"{created.CreatedBy.FirstName} {created.CreatedBy.LastName}",
                Department = created.Department.Name,
                SubmissionCount = 0
            };
        }

        public async Task<AssignmentResponseDto?> GetAssignmentAsync(int id)
        {
            var assignment = await _assignmentRepo.GetByIdAsync(id);
            if (assignment == null) return null;

            var submissions = await _submissionRepo.GetByAssignmentAsync(id);

            return new AssignmentResponseDto
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                DueDate = assignment.DueDate,
                CreatedAt = assignment.CreatedAt,
                CreatedBy = $"{assignment.CreatedBy.FirstName} {assignment.CreatedBy.LastName}",
                Department = assignment.Department.Name,
                SubmissionCount = submissions.Count()
            };
        }

        public async Task<IEnumerable<AssignmentResponseDto>> GetAssignmentsByDepartmentAsync(int departmentId)
        {
            var assignments = await _assignmentRepo.GetByDepartmentAsync(departmentId);

            return assignments.Select(a => new AssignmentResponseDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                DueDate = a.DueDate,
                CreatedAt = a.CreatedAt,
                CreatedBy = $"{a.CreatedBy.FirstName} {a.CreatedBy.LastName}",
                Department = a.Department.Name,
                SubmissionCount = a.Submissions.Count
            });
        }

        public async Task<IEnumerable<AssignmentResponseDto>> GetAssignmentsForInternAsync(Guid internId)
        {
            var assignments = await _assignmentRepo.GetAssignmentsForInternAsync(internId);

            return assignments.Select(a => new AssignmentResponseDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                DueDate = a.DueDate,
                CreatedAt = a.CreatedAt,
                CreatedBy = $"{a.CreatedBy.FirstName} {a.CreatedBy.LastName}",
                Department = a.Department.Name,
                SubmissionCount = a.Submissions.Count
            });
        }

        public async Task<SubmissionResponseDto> SubmitAssignmentAsync(int assignmentId, SubmitAssignmentDto dto, Guid internId)
        {
            // Validate assignment exists
            var assignment = await _assignmentRepo.GetByIdAsync(assignmentId);
            if (assignment == null)
                throw new ArgumentException("Assignment not found");

            // Check if already submitted
            if (await _submissionRepo.HasInternSubmittedAsync(assignmentId, internId))
                throw new InvalidOperationException("Assignment already submitted");

            // Validate Git URL
            if (!await _gitService.ValidateGitUrlAsync(dto.GitRepositoryUrl))
                throw new ArgumentException("Invalid Git repository URL");

            // Get Git metadata
            var gitMetadata = await _gitService.GetRepositoryMetadataAsync(dto.GitRepositoryUrl);

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
                InternName = $"{created.Intern.FirstName} {created.Intern.LastName}",
                Grade = null
            };
        }

        public async Task<SubmissionResponseDto?> GetSubmissionAsync(int submissionId)
        {
            var submission = await _submissionRepo.GetByIdAsync(submissionId);
            if (submission == null) return null;

            GradeResponseDto? gradeDto = null;
            if (submission.Grade != null)
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
                InternName = $"{submission.Intern.FirstName} {submission.Intern.LastName}",
                Grade = gradeDto
            };
        }

        public async Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsByAssignmentAsync(int assignmentId)
        {
            var submissions = await _submissionRepo.GetByAssignmentAsync(assignmentId);

            return submissions.Select(s => new SubmissionResponseDto
            {
                Id = s.Id,
                GitRepositoryUrl = s.GitRepositoryUrl,
                SubmittedAt = s.SubmittedAt,
                Status = s.Status.ToString(),
                LastCommitHash = s.LastCommitHash,
                BranchName = s.BranchName,
                InternName = $"{s.Intern.FirstName} {s.Intern.LastName}",
                Grade = s.Grade != null ? new GradeResponseDto
                {
                    Id = s.Grade.Id,
                    Score = s.Grade.Score,
                    Comments = s.Grade.Comments,
                    GradedAt = s.Grade.GradedAt,
                    GradedBy = $"{s.Grade.GradedBy.FirstName} {s.Grade.GradedBy.LastName}"
                } : null
            });
        }

        public async Task<GradeResponseDto> GradeSubmissionAsync(int submissionId, GradeSubmissionDto dto, Guid supervisorId)
        {
            var submission = await _submissionRepo.GetByIdAsync(submissionId);
            if (submission == null)
                throw new ArgumentException("Submission not found");

            var grade = new Grade
            {
                SubmissionId = submissionId,
                Score = dto.Score,
                Comments = dto.Comments,
                GradedById = supervisorId,
                GradedAt = DateTime.UtcNow
            };

            var created = await _gradeRepo.CreateOrUpdateAsync(grade);

            // Update submission status
            submission.Status = AssignmentStatus.Graded;
            await _submissionRepo.UpdateAsync(submission);

            return new GradeResponseDto
            {
                Id = created.Id,
                Score = created.Score,
                Comments = created.Comments,
                GradedAt = created.GradedAt,
                GradedBy = $"{created.GradedBy.FirstName} {created.GradedBy.LastName}"
            };
        }

        public async Task<GradeResponseDto?> GetGradeAsync(int submissionId)
        {
            var grade = await _gradeRepo.GetBySubmissionIdAsync(submissionId);
            if (grade == null) return null;

            return new GradeResponseDto
            {
                Id = grade.Id,
                Score = grade.Score,
                Comments = grade.Comments,
                GradedAt = grade.GradedAt,
                GradedBy = $"{grade.GradedBy.FirstName} {grade.GradedBy.LastName}"
            };
        }
    }
}