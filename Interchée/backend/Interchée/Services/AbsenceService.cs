using Interchée.Data;
using Interchée.Dtos;
using Interchée.Entities;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Services
{
    public class AbsenceService : IAbsenceService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AbsenceService> _logger;

        public AbsenceService(AppDbContext context, ILogger<AbsenceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResult<AbsenceRequestDto>> CreateAbsenceRequestAsync(CreateAbsenceRequestDto request, string userId)
        {
            try
            {
                _logger.LogInformation("Creating absence request for user {UserId}", userId);

                // Convert string to Guid for comparison
                if (!Guid.TryParse(userId, out Guid userGuid))
                    return ServiceResult<AbsenceRequestDto>.Fail("Invalid user ID format");

                // Get intern associated with user
                var intern = await _context.Interns
                    .Include(i => i.User)
                    .FirstOrDefaultAsync(i => i.UserId == userGuid);

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
                    Reason = request.Reason ?? string.Empty,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Status = AbsenceStatus.Pending,
                    RequestedAt = DateTime.UtcNow
                };

                _context.AbsenceRequests.Add(absenceRequest);
                await _context.SaveChangesAsync();

                var dto = await MapToAbsenceRequestDto(absenceRequest);
                return ServiceResult<AbsenceRequestDto>.Ok(dto, "Absence request created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating absence request for user {UserId}", userId);
                return ServiceResult<AbsenceRequestDto>.Fail("An error occurred while creating absence request");
            }
        }

        public async Task<ServiceResult> ApproveAbsenceRequestAsync(int requestId, ApproveAbsenceRequestDto request, string approverId)
        {
            try
            {
                _logger.LogInformation("Processing absence request {RequestId} by approver {ApproverId}", requestId, approverId);

                var absenceRequest = await _context.AbsenceRequests
                    .Include(ar => ar.Intern)
                    .ThenInclude(i => i.User)
                    .FirstOrDefaultAsync(ar => ar.Id == requestId);

                if (absenceRequest == null)
                    return ServiceResult.Fail("Absence request not found");

                if (absenceRequest.Status != AbsenceStatus.Pending)
                    return ServiceResult.Fail("Absence request has already been processed");

                // Update request
                absenceRequest.Status = request.IsApproved ? AbsenceStatus.Approved : AbsenceStatus.Rejected;

                if (Guid.TryParse(approverId, out Guid approverGuid))
                {
                    absenceRequest.ApprovedById = approverGuid;
                }

                absenceRequest.ReviewedAt = DateTime.UtcNow;

                if (!request.IsApproved)
                    absenceRequest.RejectionReason = request.RejectionReason;

                await _context.SaveChangesAsync();

                var action = request.IsApproved ? "approved" : "rejected";
                _logger.LogInformation("Absence request {RequestId} {Action}", requestId, action);

                return ServiceResult.Ok($"Absence request {action} successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing absence request {RequestId}", requestId);
                return ServiceResult.Fail("An error occurred while processing the absence request");
            }
        }

        public async Task<ServiceResult<List<AbsenceRequestDto>>> GetMyAbsenceRequestsAsync(string userId)
        {
            try
            {
                // Convert string to Guid for comparison
                if (!Guid.TryParse(userId, out Guid userGuid))
                    return ServiceResult<List<AbsenceRequestDto>>.Fail("Invalid user ID format");

                var intern = await _context.Interns.FirstOrDefaultAsync(i => i.UserId == userGuid);
                if (intern == null)
                    return ServiceResult<List<AbsenceRequestDto>>.Fail("Intern not found");

                var requests = await _context.AbsenceRequests
                    .Where(ar => ar.InternId == intern.Id)
                    .Include(ar => ar.Intern)
                    .ThenInclude(i => i.User)
                    .Include(ar => ar.ApprovedBy)
                    .OrderByDescending(ar => ar.RequestedAt)
                    .Select(ar => MapToAbsenceRequestDto(ar))
                    .ToListAsync();

                return ServiceResult<List<AbsenceRequestDto>>.Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching absence requests for user {UserId}", userId);
                return ServiceResult<List<AbsenceRequestDto>>.Fail("An error occurred while fetching absence requests");
            }
        }

        public async Task<ServiceResult<List<AbsenceRequestDto>>> GetPendingRequestsAsync(string supervisorId)
        {
            try
            {
                // Convert string to Guid for comparison
                if (!Guid.TryParse(supervisorId, out Guid supervisorGuid))
                    return ServiceResult<List<AbsenceRequestDto>>.Fail("Invalid supervisor ID format");

                var supervisedInternIds = await _context.Interns
                    .Where(i => i.SupervisorId == supervisorGuid)
                    .Select(i => i.Id)
                    .ToListAsync();

                var requests = await _context.AbsenceRequests
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
                _logger.LogError(ex, "Error fetching pending absence requests for supervisor {SupervisorId}", supervisorId);
                return ServiceResult<List<AbsenceRequestDto>>.Fail("An error occurred while fetching pending requests");
            }
        }

        public async Task<ServiceResult<AbsenceSummaryDto>> GetAbsenceSummaryAsync(int internId)
        {
            try
            {
                var intern = await _context.Interns
                    .Include(i => i.User)
                    .FirstOrDefaultAsync(i => i.Id == internId);

                if (intern == null)
                    return ServiceResult<AbsenceSummaryDto>.Fail("Intern not found");

                if (intern.User == null)
                    return ServiceResult<AbsenceSummaryDto>.Fail("User information not found for intern");

                var requests = await _context.AbsenceRequests
                    .Where(ar => ar.InternId == internId)
                    .ToListAsync();

                var totalApprovedDays = requests
                    .Where(r => r.Status == AbsenceStatus.Approved)
                    .Sum(r => (r.EndDate - r.StartDate).Days + 1);

                var summary = new AbsenceSummaryDto
                {
                    InternId = internId,
                    InternName = $"{(intern.User.FirstName ?? string.Empty)} {(intern.User.LastName ?? string.Empty)}".Trim(),
                    TotalAbsenceDays = totalApprovedDays,
                    PendingRequests = requests.Count(r => r.Status == AbsenceStatus.Pending),
                    ApprovedRequests = requests.Count(r => r.Status == AbsenceStatus.Approved),
                    RejectedRequests = requests.Count(r => r.Status == AbsenceStatus.Rejected),
                    RemainingAbsenceDays = Math.Max(0, 20 - totalApprovedDays)
                };

                return ServiceResult<AbsenceSummaryDto>.Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching absence summary for intern {InternId}", internId);
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

                var monthlyAbsenceDays = await _context.AbsenceRequests
                    .Where(ar => ar.InternId == internId &&
                                ar.Status == AbsenceStatus.Approved &&
                                ar.StartDate >= currentMonthStart &&
                                ar.EndDate <= currentMonthEnd)
                    .SumAsync(ar => (int?)(ar.EndDate - ar.StartDate).Days + 1) ?? 0;

                if (monthlyAbsenceDays + requestedDays > 5)
                    return ServiceResult<bool>.Fail("Absence request exceeds monthly limit of 5 days");

                // Check total internship limit (20 days total)
                var totalAbsenceDays = await _context.AbsenceRequests
                    .Where(ar => ar.InternId == internId && ar.Status == AbsenceStatus.Approved)
                    .SumAsync(ar => (int?)(ar.EndDate - ar.StartDate).Days + 1) ?? 0;

                if (totalAbsenceDays + requestedDays > 20)
                    return ServiceResult<bool>.Fail("Absence request exceeds total internship limit of 20 days");

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking absence limits for intern {InternId}", internId);
                return ServiceResult<bool>.Fail("An error occurred while checking absence limits");
            }
        }

        public async Task<ServiceResult<List<AbsenceRequestDto>>> GetDepartmentAbsencesAsync(int departmentId)
        {
            try
            {
                var requests = await _context.AbsenceRequests
                    .Include(ar => ar.Intern)
                    .ThenInclude(i => i.User)
                    .ThenInclude(u => u.Department)
                    .Include(ar => ar.ApprovedBy)
                    .Where(ar => ar.Intern != null &&
                                ar.Intern.User != null &&
                                ar.Intern.User.DepartmentId == departmentId)
                    .OrderByDescending(ar => ar.RequestedAt)
                    .Select(ar => MapToAbsenceRequestDto(ar))
                    .ToListAsync();

                return ServiceResult<List<AbsenceRequestDto>>.Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching department absences for department {DepartmentId}", departmentId);
                return ServiceResult<List<AbsenceRequestDto>>.Fail("An error occurred while fetching department absences");
            }
        }

        private async Task<AbsenceRequestDto> MapToAbsenceRequestDto(AbsenceRequest request)
        {
            // Ensure related entities are loaded
            await _context.Entry(request)
                .Reference(r => r.Intern)
                .LoadAsync();

            if (request.Intern != null)
            {
                await _context.Entry(request.Intern)
                    .Reference(i => i.User)
                    .LoadAsync();
            }

            if (request.ApprovedById.HasValue)
            {
                await _context.Entry(request)
                    .Reference(r => r.ApprovedBy)
                    .LoadAsync();
            }

            var internUser = request.Intern?.User;
            var approvedByUser = request.ApprovedBy;

            return new AbsenceRequestDto
            {
                Id = request.Id,
                InternId = request.InternId,
                InternName = internUser != null
                    ? $"{(internUser.FirstName ?? string.Empty)} {(internUser.LastName ?? string.Empty)}".Trim()
                    : "Unknown Intern",
                InternEmail = internUser?.Email ?? "Unknown Email",
                Reason = request.Reason ?? string.Empty,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = request.Status.ToString(),
                RejectionReason = request.RejectionReason,
                ApprovedByName = approvedByUser != null
                    ? $"{(approvedByUser.FirstName ?? string.Empty)} {(approvedByUser.LastName ?? string.Empty)}".Trim()
                    : null,
                RequestedAt = request.RequestedAt,
                ReviewedAt = request.ReviewedAt,
                TotalDays = (request.EndDate - request.StartDate).Days + 1
            };
        }
    }
}