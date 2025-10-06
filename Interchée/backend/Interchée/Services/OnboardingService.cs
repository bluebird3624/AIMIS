using Interchée.Contracts.Onboarding;
using Interchée.Data;
using Interchée.Entities;
using Interchée.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Interchée.Services
{
    public class OnboardingService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userMgr;

        private const string Pending = "Pending";
        private const string Approved = "Approved";
        private const string Rejected = "Rejected";

        private static readonly HashSet<string> AllowedDeptRoles =
            new(StringComparer.OrdinalIgnoreCase) { "Admin", "HR", "Supervisor", "Instructor", "Intern" };

        public OnboardingService(AppDbContext db, UserManager<AppUser> userMgr)
        {
            _db = db;
            _userMgr = userMgr;
        }

        // -------------------
        // Username suggestion
        // -------------------

        private static string Slugify(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            // Normalize accents → ASCII
            var normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            var ascii = sb.ToString().Normalize(NormalizationForm.FormC);

            // Keep letters/digits, collapse separators to single dot
            var lowered = ascii.ToLowerInvariant();
            lowered = Regex.Replace(lowered, @"[^a-z0-9]+", ".");   // non-alphanum → dot
            lowered = Regex.Replace(lowered, @"^\.+|\.+$", "");     // trim dots
            lowered = Regex.Replace(lowered, @"\.{2,}", ".");       // collapse dots
            return lowered;
        }

        private static string BaseUserName(string first, string last)
            => string.IsNullOrWhiteSpace(first) && string.IsNullOrWhiteSpace(last)
                ? string.Empty
                : $"{Slugify(first)}.{Slugify(last)}".Trim('.');

        private async Task<string> SuggestUniqueUserNameAsync(string first, string last, CancellationToken ct = default)
        {
            var baseName = BaseUserName(first, last);
            if (string.IsNullOrWhiteSpace(baseName))
                baseName = "user";

            // Try base, then numbered suffixes
            var candidate = baseName;
            for (var i = 0; i < 20; i++)
            {
                if (i > 0) candidate = $"{baseName}{i}";

                // Check Identity users for existence
                var exists = await _userMgr.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.UserName == candidate, ct);

                if (!exists) return candidate;
            }

            // Fallback with random suffix
            return $"{baseName}{Random.Shared.Next(1000, 9999)}";
        }

        private static string ComposeDisplay(string first, string last)
            => $"{(first ?? string.Empty).Trim()} {(last ?? string.Empty).Trim()}".Trim();

        private static DecisionReadDto ToDecisionRead(OnboardingDecision d) =>
        new(d.Id, d.Action, d.Reason, d.ActorUserId, d.CreatedAt);
        private OnboardingRequestReadDto ToRead(OnboardingRequest e, string proposedUserName)
        {
            var display = ComposeDisplay(e.FirstName, e.LastName);

            var decisions = e.Decisions?
            .OrderByDescending(d => d.CreatedAt)
            .Select(ToDecisionRead)
            .ToList();

            return new OnboardingRequestReadDto(
                e.Id,
                e.Email,
                e.FullName,
                e.FirstName,
                e.LastName,
                e.MiddleName,
                display,
                e.DepartmentId,
                e.Status,
                e.RequestedAt,
                e.ApprovedByUserId,
                e.ApprovedAt,
                proposedUserName,
                decisions
            );
        }

        private async Task AddDecisionAsync(long requestId, Guid actorUserId, string action, string? reason = null)
        {
            _db.OnboardingDecisions.Add(new OnboardingDecision
            {
                RequestId = requestId,
                Action = action,
                Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
                ActorUserId = actorUserId,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        // CREATE
        public async Task<OnboardingRequestReadDto> CreateAsync(OnboardingRequestCreateDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var first = dto.FirstName.Trim();
            var last = dto.LastName.Trim();
            var mid = string.IsNullOrWhiteSpace(dto.MiddleName) ? null : dto.MiddleName.Trim();
            var full = $"{first} {last}";

            var deptExists = await _db.Departments.AsNoTracking().AnyAsync(d => d.Id == dto.DepartmentId);
            if (!deptExists) throw new InvalidOperationException("Department does not exist.");

            var duplicatePending = await _db.OnboardingRequests
                .AnyAsync(x => x.Email == email && x.Status == Pending);
            if (duplicatePending) throw new InvalidOperationException("A pending request for this email already exists.");

            var entity = new OnboardingRequest
            {
                Email = email,
                FirstName = first,
                LastName = last,
                MiddleName = mid,
                FullName = full,
                DepartmentId = dto.DepartmentId,
                Status = Pending,
                RequestedAt = DateTime.UtcNow
            };

            _db.OnboardingRequests.Add(entity);
            await _db.SaveChangesAsync();

            // Reload including Decisions so the mapper can project timeline (will be empty now)
            var reloaded = await _db.OnboardingRequests
                .Include(x => x.Decisions)
                .FirstAsync(x => x.Id == entity.Id);

            // Suggested username (not persisted)
            var proposed = await SuggestUniqueUserNameAsync(first, last);

            return ToRead(reloaded, proposed);
        }

        // LIST
        public async Task<List<OnboardingRequestReadDto>> GetAsync(string? status = null, int? departmentId = null)
        {
            var q = _db.OnboardingRequests.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(x => x.Status == status.Trim());

            if (departmentId.HasValue)
                q = q.Where(x => x.DepartmentId == departmentId.Value);

            // Eager-load decisions for timeline
            var list = await q
                .Include(x => x.Decisions)
                .OrderByDescending(x => x.RequestedAt)
                .ToListAsync();

            var result = new List<OnboardingRequestReadDto>(list.Count);
            foreach (var e in list)
            {
                var proposed = await SuggestUniqueUserNameAsync(e.FirstName, e.LastName);
                result.Add(ToRead(e, proposed));
            }
            return result;
        }

        // APPROVE
        public async Task<OnboardingRequestReadDto> ApproveAsync(
            long id,
            ApproveOnboardingDto dto,
            Guid approverUserId,
            string roleName)
        {
            var r = await _db.OnboardingRequests.FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new KeyNotFoundException("Onboarding request not found.");

            if (r.Status != Pending)
                throw new InvalidOperationException("Only pending requests can be approved.");

            // Canonicalize/validate role
            var canonicalRole = RoleHelper.ToCanonical(roleName);
            if (canonicalRole is null)
                throw new InvalidOperationException("Invalid role name.");

            // Department must be active
            var activeDept = await _db.Departments.AnyAsync(d => d.Id == r.DepartmentId && d.IsActive);
            if (!activeDept)
                throw new InvalidOperationException("Department is inactive; cannot approve onboarding.");

            // Pre-checks for user creation
            var userName = dto.UserName.Trim();
            if (await _userMgr.FindByNameAsync(userName) is not null)
                throw new InvalidOperationException("Username is already taken.");

            var normalizedEmail = r.Email.Trim().ToLowerInvariant();
            if (await _userMgr.FindByEmailAsync(normalizedEmail) is not null)
                throw new InvalidOperationException("A user with this email already exists.");

            using var tx = await _db.Database.BeginTransactionAsync();

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = userName,
                Email = normalizedEmail,
                EmailConfirmed = true,
                IsActive = true,
                FirstName = r.FirstName,
                LastName = r.LastName,
                MiddleName = r.MiddleName
            };

            var createRes = await _userMgr.CreateAsync(user, dto.TempPassword);
            if (!createRes.Succeeded)
            {
                var msg = string.Join("; ", createRes.Errors.Select(e => e.Description));
                throw new InvalidOperationException(msg);
            }

            _db.DepartmentRoleAssignments.Add(new DepartmentRoleAssignment
            {
                UserId = user.Id,
                DepartmentId = r.DepartmentId,
                RoleName = canonicalRole,
                AssignedAt = DateTime.UtcNow
            });

            // Snapshot fields on the request
            r.Status = Approved;
            r.ApprovedByUserId = approverUserId;
            r.ApprovedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Decision log: Approved (no reason)
            _db.OnboardingDecisions.Add(new OnboardingDecision
            {
                RequestId = r.Id,
                Action = "Approved",
                Reason = null,
                ActorUserId = approverUserId,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            await tx.CommitAsync();

            // Reload with decisions for read mapping
            var reloaded = await _db.OnboardingRequests
                .Include(x => x.Decisions)
                .FirstAsync(x => x.Id == r.Id);

            var proposedForRead = await SuggestUniqueUserNameAsync(r.FirstName, r.LastName);
            return ToRead(reloaded, proposedForRead);
        }

        // REJECT  (now accepts a reason and logs the decision)
        public async Task<OnboardingRequestReadDto> RejectAsync(long id, Guid approverUserId, string? reason = null)
        {
            var r = await _db.OnboardingRequests.FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new KeyNotFoundException("Onboarding request not found.");

            if (r.Status != Pending)
                throw new InvalidOperationException("Only pending requests can be rejected.");

            r.Status = Rejected;
            r.ApprovedByUserId = approverUserId;   // who took the decision
            r.ApprovedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Decision log: Rejected (with reason)
            _db.OnboardingDecisions.Add(new OnboardingDecision
            {
                RequestId = r.Id,
                Action = "Rejected",
                Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
                ActorUserId = approverUserId,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            // Reload with decisions for read mapping
            var reloaded = await _db.OnboardingRequests
                .Include(x => x.Decisions)
                .FirstAsync(x => x.Id == r.Id);

            var proposedForRead = await SuggestUniqueUserNameAsync(r.FirstName, r.LastName);
            return ToRead(reloaded, proposedForRead);
        }

    }
}
