using Interchée.Data;
using Interchée.Dtos;
using Interchée.Entities;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Services
{
    public class AbsenceService(AppDbContext context, ILogger<AbsenceService> logger) : IAbsenceService
    {
        public Guid? SupervisorId { get; private set; }
        public Guid UserId { get; private set; }

        public async Task<ServiceResult<AbsenceRequestDto>> CreateAbsenceRequestAsync(CreateAbsenceRequestDto request, string userId)
        {
            try
            {
                logger.LogInformation("Creating absence request for user {UserId}", userId);

                // Get intern associated with user
                var intern = await context.Interns
                    .Include(i => i.User)
                    .FirstOrDefaultAsync(i => i.UserId.ToString() == userId);

                if (intern == null)
                    return ServiceResult<AbsenceRequestDto>.Fail("Intern not found");

                // Validate dates
                if (request.StartDate > request.EndDate)
                    return ServiceResult<AbsenceRequestDto>.Fail("Start date cannot be after end date");

                if (request.StartDate < DateTime.Today)
                    return ServiceResult<AbsenceRequestDto>.Fail("Cannot request absence for past dates");

                // Check absence limits
                var limitCheck = await CheckAbsenceLimitAsync(intern.Id, request.StartDate, request.EndDate);
                if (!limitCheck.Success)
                    return ServiceResult<AbsenceRequestDto>.Fail(limitCheck.Message ?? "Absence limit check failed");

                // Create absence request
                var absenceRequest = new AbsenceRequest
                {
                    InternId = intern.Id,
                    Reason = request.Reason,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Status = AbsenceStatus.Pending,
                    RequestedAt = DateTime.UtcNow
                };

                context.AbsenceRequests.Add(absenceRequest);
                await context.SaveChangesAsync();

                var dto = MapToAbsenceRequestDto(absenceRequest);
                return ServiceResult<AbsenceRequestDto>.Ok(dto, "Absence request created successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating absence request for user {UserId}", userId);
                return ServiceResult<AbsenceRequestDto>.Fail("An error occurred while creating absence request");
            }
        }

        public async Task<ServiceResult> ApproveAbsenceRequestAsync(int requestId, ApproveAbsenceRequestDto request, string ApproverId)
        {
            try
            {
                logger.LogInformation("Processing absence request {RequestId} by approver {ApproverId}", requestId, ApproverId);

                var absenceRequest = await context.AbsenceRequests
                    .Include(ar => ar.Intern)
                    .ThenInclude(i => i.User)
                    .FirstOrDefaultAsync(ar => ar.Id == requestId);

                if (absenceRequest == null)
                    return ServiceResult.Fail("Absence request not found");

                if (absenceRequest.Status != AbsenceStatus.Pending)
                    return ServiceResult.Fail("Absence request has already been processed");

                // Update request
                absenceRequest.Status = request.IsApproved ? AbsenceStatus.Approved : AbsenceStatus.Rejected;
                absenceRequest.ApprovedById = Guid.TryParse(ApproverId, out var approverGuid) ? approverGuid : (Guid?)null;
                absenceRequest.ReviewedAt = DateTime.UtcNow;

                if (!request.IsApproved)
                    absenceRequest.RejectionReason = request.RejectionReason;

                await context.SaveChangesAsync();

                var action = request.IsApproved ? "approved" : "rejected";
                logger.LogInformation("Absence request {RequestId} {Action}", requestId, action);

                return ServiceResult.Ok($"Absence request {action} successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing absence request {RequestId}", requestId);
                return ServiceResult.Fail("An error occurred while processing the absence request");
            }
        }

        public async Task<ServiceResult<List<AbsenceRequestDto>>> GetMyAbsenceRequestsAsync(string userId)
        {
            try
            {
                var intern = await context.Interns.FirstOrDefaultAsync(i => i.UserId == UserId);
                if (intern == null)
                    return ServiceResult<List<AbsenceRequestDto>>.Fail("Intern not found");

                var requests = await context.AbsenceRequests
                    .Where(ar => ar.InternId == intern.Id)
                    .Include(ar => ar.Intern)
                    .ThenInclude(i => i.User)
                    .Include(ar => ar.ApprovedBy)
                    .OrderByDescending(ar => ar.RequestedAt)
                    .Select(ar => MapToAbsenceRequestDto(ar))
                    .ToListAsync();

                // Ensure that Intern and User are not null before accessing their properties
                requests = requests.Where(dto => dto.InternName != null && dto.InternEmail != null).ToList();

                return ServiceResult<List<AbsenceRequestDto>>.Ok(requests);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching absence requests for user {UserId}", userId);
                return ServiceResult<List<AbsenceRequestDto>>.Fail("An error occurred while fetching absence requests");
            }
        }

        public async Task<ServiceResult<List<AbsenceRequestDto>>> GetPendingRequestsAsync(string supervisorId)
        {
            try
            {
                var supervisedInternIds = await context.Interns
                    .Where(i => i.SupervisorId == SupervisorId)
                    .Select(i => i.Id)
                    .ToListAsync();

                var requests = await context.AbsenceRequests
                    .Where(ar => supervisedInternIds.Contains(ar.InternId) && ar.Status == AbsenceStatus.Pending)
                    .Include(ar => ar.Intern)
                    .ThenInclude(i => i.User)
                    .Include(ar => ar.ApprovedBy)
                    .OrderBy(ar => ar.StartDate)
                    .Select(ar => MapToAbsenceRequestDto(ar))
                    .ToListAsync();

                return ServiceResult<List<AbsenceRequestDto>>.Ok(requests);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching pending absence requests for supervisor {SupervisorId}", supervisorId);
                return ServiceResult<List<AbsenceRequestDto>>.Fail("An error occurred while fetching pending requests");
            }
        }

        public async Task<ServiceResult<AbsenceSummaryDto>> GetAbsenceSummaryAsync(int internId)
        {
            try
            {
                var intern = await context.Interns
                    .Include(i => i.User)
                    .FirstOrDefaultAsync(i => i.Id == internId);

                if (intern == null)
                    return ServiceResult<AbsenceSummaryDto>.Fail("Intern not found");

                var requests = await context.AbsenceRequests
                    .Where(ar => ar.InternId == internId)
                    .ToListAsync();

                var totalApprovedDays = requests
                    .Where(r => r.Status == AbsenceStatus.Approved)
                    .Sum(r => r.TotalDays);

                var summary = new AbsenceSummaryDto
                {
                    InternId = internId,
                    InternName = $"{intern.User.FirstName} {intern.User.LastName}",
                    TotalAbsenceDays = totalApprovedDays,
                    PendingRequests = requests.Count(r => r.Status == AbsenceStatus.Pending),
                    ApprovedRequests = requests.Count(r => r.Status == AbsenceStatus.Approved),
                    RejectedRequests = requests.Count(r => r.Status == AbsenceStatus.Rejected),
                    RemainingAbsenceDays = Math.Max(0, 20 - totalApprovedDays) // 20 days max limit
                };

                return ServiceResult<AbsenceSummaryDto>.Ok(summary);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching absence summary for intern {InternId}", internId);
                return ServiceResult<AbsenceSummaryDto>.Fail("An error occurred while fetching absence summary");
            }
        }

        public async Task<ServiceResult<bool>> CheckAbsenceLimitAsync(int internId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var requestedDays = (endDate - startDate).Days + 1;

                // Check monthly limit (5 days per month)
                var currentMonthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);

                var monthlyAbsenceDays = await context.AbsenceRequests
                    .Where(ar => ar.InternId == internId &&
                                ar.Status == AbsenceStatus.Approved &&
                                ar.StartDate >= currentMonthStart &&
                                ar.EndDate <= currentMonthEnd)
                    .SumAsync(ar => ar.TotalDays);

                if (monthlyAbsenceDays + requestedDays > 5)
                    return ServiceResult<bool>.Fail("Absence request exceeds monthly limit of 5 days");

                // Check total internship limit (20 days total)
                var totalAbsenceDays = await context.AbsenceRequests
                    .Where(ar => ar.InternId == internId && ar.Status == AbsenceStatus.Approved)
                    .SumAsync(ar => ar.TotalDays);

                if (totalAbsenceDays + requestedDays > 20)
                    return ServiceResult<bool>.Fail("Absence request exceeds total internship limit of 20 days");

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking absence limits for intern {InternId}", internId);
                return ServiceResult<bool>.Fail("An error occurred while checking absence limits");
            }
        }

        public async Task<ServiceResult<List<AbsenceRequestDto>>> GetDepartmentAbsencesAsync(int departmentId)
        {
            try
            {
                var requests = await context.AbsenceRequests
                    .Include(ar => ar.Intern)
                    .ThenInclude(i => i.User)
                    .ThenInclude(u => u.Department)
                    .Include(ar => ar.ApprovedBy)
                    .Where(ar =>
                        ar.Intern != null &&
                        ar.Intern.User != null &&
                        ar.Intern.User.Department != null && // Added null check for Department
                        ar.Intern.User.DepartmentId == departmentId)
                    .OrderByDescending(ar => ar.RequestedAt)
                    .Select(ar => MapToAbsenceRequestDto(ar))
                    .ToListAsync();

                return ServiceResult<List<AbsenceRequestDto>>.Ok(requests);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching department absences for department {DepartmentId}", departmentId);
                return ServiceResult<List<AbsenceRequestDto>>.Fail("An error occurred while fetching department absences");
            }
        }

        private static AbsenceRequestDto MapToAbsenceRequestDto(AbsenceRequest request)
        {
            var internUser = request.Intern?.User;
            return new AbsenceRequestDto
            {
                Id = request.Id,
                InternId = request.InternId,
                InternName = internUser != null
                    ? $"{internUser.FirstName} {internUser.LastName}"
                    : null,
                InternEmail = internUser?.Email,
                Reason = request.Reason,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = request.Status.ToString(),
                RejectionReason = request.RejectionReason,
                ApprovedByName = request.ApprovedBy != null ?
                    $"{request.ApprovedBy.FirstName} {request.ApprovedBy.LastName}" : null,
                RequestedAt = request.RequestedAt,
                ReviewedAt = request.ReviewedAt,
                TotalDays = request.TotalDays
            };
        }
    }
}
