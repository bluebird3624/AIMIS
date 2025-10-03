using System.Security.Claims;
using Interchée.Data;
using Interchée.DTOs;
using Interchée.Entities;
using Interchée.Repositories.Interfaces;
using Interchée.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Services.Implementations
{
    public class AssignmentService(
        IAssignmentRepository assignmentRepo,
        ISubmissionRepository submissionRepo,
        IGradeRepository gradeRepo,
        IGitIntegrationService gitService,
        AppDbContext db,
        UserManager<AppUser> userManager,
        IHttpContextAccessor httpContextAccessor) : IAssignmentService
    {
        private readonly IAssignmentRepository _assignmentRepo = assignmentRepo;
        private readonly ISubmissionRepository _submissionRepo = submissionRepo;
        private readonly IGradeRepository _gradeRepo = gradeRepo;
        private readonly IGitIntegrationService _gitService = gitService;
        private readonly AppDbContext _db = db;
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly IHttpContextAccessor _http = httpContextAccessor;

        // ===== ASSIGNMENT CRUD =====

        public async Task<AssignmentResponseDto> CreateAssignmentAsync(CreateAssignmentDto dto)
        {
            var user = await GetCurrentUserAsync(); // throws if unauthenticated

            var assignment = new Assignment
            {
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                DepartmentId = dto.DepartmentId,
                CreatedById = user.Id,          //  AppUser FK
                CreatedAt = DateTime.UtcNow,
                Status = AssignmentStatus.Assigned
            };

            var created = await _assignmentRepo.CreateAsync(assignment);

            // Make sure navs are available for mapping
            await _db.Entry(created).Reference(a => a.CreatedBy).LoadAsync();
            await _db.Entry(created).Reference(a => a.Department).LoadAsync();

            return new AssignmentResponseDto
            {
                Id = created.Id,
                Title = created.Title,
                Description = created.Description,
                DueDate = created.DueDate,
                CreatedAt = created.CreatedAt,
                CreatedBy = created.CreatedBy is { } u ? $"{u.FirstName} {u.LastName}" : "Unknown",
                Department = created.Department?.Name ?? "Unknown Department",
                SubmissionCount = 0
            };
        }

        public async Task<AssignmentResponseDto?> GetAssignmentAsync(int id)
        {
            var assignment = await _assignmentRepo.GetByIdAsync(id);
            if (assignment is null) return null;

            var submissions = await _submissionRepo.GetByAssignmentAsync(id);

            return new AssignmentResponseDto
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                DueDate = assignment.DueDate,
                CreatedAt = assignment.CreatedAt,
                CreatedBy = assignment.CreatedBy != null
                                ? $"{assignment.CreatedBy.FirstName} {assignment.CreatedBy.LastName}"
                                : "Unknown",
                Department = assignment.Department?.Name ?? "Unknown Department",
                SubmissionCount = submissions.Count()
            };
        }

        public async Task<IEnumerable<AssignmentResponseDto>> GetAllAssignmentsAsync()
        {
            var assignments = await _assignmentRepo.GetAllAsync();

            return assignments.Select(a => new AssignmentResponseDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                DueDate = a.DueDate,
                CreatedAt = a.CreatedAt,
                CreatedBy = a.CreatedBy != null
                                ? $"{a.CreatedBy.FirstName} {a.CreatedBy.LastName}"
                                : "Unknown",
                Department = a.Department?.Name ?? "Unknown Department",
                SubmissionCount = a.Submissions.Count
            });
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
                CreatedBy = a.CreatedBy != null
                                ? $"{a.CreatedBy.FirstName} {a.CreatedBy.LastName}"
                                : "Unknown",
                Department = a.Department?.Name ?? "Unknown Department",
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
                CreatedBy = a.CreatedBy != null
                                ? $"{a.CreatedBy.FirstName} {a.CreatedBy.LastName}"
                                : "Unknown",
                Department = a.Department?.Name ?? "Unknown Department",
                SubmissionCount = a.Submissions.Count
            });
        }

        public async Task<AssignmentResponseDto> UpdateAssignmentAsync(int id, UpdateAssignmentDto dto)
        {
            var assignment = await _assignmentRepo.GetByIdAsync(id)
                             ?? throw new ArgumentException("Assignment not found");

            if (!string.IsNullOrEmpty(dto.Title)) assignment.Title = dto.Title;
            if (!string.IsNullOrEmpty(dto.Description)) assignment.Description = dto.Description;
            if (dto.DueDate.HasValue) assignment.DueDate = dto.DueDate.Value;

            await _assignmentRepo.UpdateAsync(assignment);

            var updated = await GetAssignmentAsync(id);
            return updated ?? throw new InvalidOperationException("Updated assignment not found");
        }

        public async Task DeleteAssignmentAsync(int id)
        {
            await _assignmentRepo.DeleteAsync(id);
        }

        // ===== SUBMISSIONS =====

        public async Task<SubmissionResponseDto> SubmitAssignmentAsync(int assignmentId, SubmitAssignmentDto dto, Guid internId)
        {
            if (await _submissionRepo.HasInternSubmittedAsync(assignmentId, internId))
                throw new InvalidOperationException("Assignment already submitted");

            if (!await _gitService.ValidateGitUrlAsync(dto.GitRepositoryUrl))
                throw new ArgumentException("Invalid Git repository URL");

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
                InternName = created.Intern != null
                                    ? $"{created.Intern.FirstName} {created.Intern.LastName}"
                                    : "Unknown Intern",
                Grade = null
            };
        }

        // ===== GRADING =====

        public async Task<GradeResponseDto> GradeSubmissionAsync(int submissionId, GradeSubmissionDto dto)
        {
            var user = await GetCurrentUserAsync(); // grader = current user
            var submission = await _submissionRepo.GetByIdAsync(submissionId)
                             ?? throw new ArgumentException("Submission not found");

            var grade = new Grade
            {
                SubmissionId = submissionId,
                Score = dto.Score,
                Comments = dto.Comments,
                GradedById = user.Id,          //  AppUser FK
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

        // ===== Helpers =====

        private async Task<AppUser> GetCurrentUserAsync()
        {
            var principal = _http.HttpContext?.User;
            if (principal is null || !principal.Identity?.IsAuthenticated == true)
                throw new UnauthorizedAccessException("User is not authenticated.");

            // If you prefer to avoid DB hit, you could read Guid from NameIdentifier claim and skip GetUserAsync
            var user = await _userManager.GetUserAsync(principal);
            return user ?? throw new UnauthorizedAccessException("User not found.");
        }

        public async Task<SubmissionResponseDto?> GetSubmissionAsync(int submissionId)
        {
            var submission = await _submissionRepo.GetByIdAsync(submissionId);
            if (submission is null) return null;

            GradeResponseDto? gradeDto = null;
            if (submission.Grade is { GradedBy: not null })
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
                InternName = s.Intern != null
                                    ? $"{s.Intern.FirstName} {s.Intern.LastName}"
                                    : "Unknown Intern",
                Grade = (s.Grade != null && s.Grade.GradedBy != null)
                                    ? new GradeResponseDto
                                    {
                                        Id = s.Grade.Id,
                                        Score = s.Grade.Score,
                                        Comments = s.Grade.Comments,
                                        GradedAt = s.Grade.GradedAt,
                                        GradedBy = $"{s.Grade.GradedBy.FirstName} {s.Grade.GradedBy.LastName}"
                                    }
                                    : null
            });
        }

        public async Task<GradeResponseDto?> GetGradeAsync(int submissionId)
        {
            var grade = await _gradeRepo.GetBySubmissionIdAsync(submissionId);
            if (grade is null) return null;

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
