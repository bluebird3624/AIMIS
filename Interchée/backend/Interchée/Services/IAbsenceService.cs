using Interchée.Dtos;

namespace Interchée.Services
{
    public interface IAbsenceService
    {
        Task<ServiceResult<AbsenceRequestDto>> CreateAbsenceRequestAsync(CreateAbsenceRequestDto request, string userId);
        Task<ServiceResult> ApproveAbsenceRequestAsync(int requestId, ApproveAbsenceRequestDto request, string approverId);
        Task<ServiceResult<List<AbsenceRequestDto>>> GetMyAbsenceRequestsAsync(string userId);
        Task<ServiceResult<List<AbsenceRequestDto>>> GetPendingRequestsAsync(string supervisorId);
        Task<ServiceResult<AbsenceSummaryDto>> GetAbsenceSummaryAsync(int internId);
        Task<ServiceResult<bool>> CheckAbsenceLimitAsync(int internId, DateTime startDate, DateTime endDate);
        Task<ServiceResult<List<AbsenceRequestDto>>> GetDepartmentAbsencesAsync(int departmentId);
    }

    public class ServiceResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        public static ServiceResult Ok(string message = "") => new() { Success = true, Message = message };
        public static ServiceResult Fail(string message) => new() { Success = false, Message = message };
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; set; }

        public static ServiceResult<T> Ok(T data, string message = "") => new() { Success = true, Data = data, Message = message };
        public static new ServiceResult<T> Fail(string message) => new() { Success = false, Message = message };

        internal static ServiceResult<List<AbsenceRequestDto>> Ok(List<Task<AbsenceRequestDto>> requests)
        {
            throw new NotImplementedException();
        }
    }
}