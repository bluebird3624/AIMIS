// Common/ApiResponse.cs
namespace Interchée.Common
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();

        // Static factory methods for success
        public static ApiResponse Success(string message = "")
            => new ApiResponse { Success = true, Message = message };

        public static ApiResponse<T> Success<T>(T data, string message = "")
            => new ApiResponse<T> { Success = true, Data = data, Message = message };

        // Static factory methods for error
        public static ApiResponse Error(string error)
            => new ApiResponse { Success = false, Errors = new List<string> { error } };

        public static ApiResponse Error(List<string> errors)
            => new ApiResponse { Success = false, Errors = errors };

        public static ApiResponse<T> Error<T>(string error)
            => new ApiResponse<T> { Success = false, Errors = new List<string> { error } };

        public static ApiResponse<T> Error<T>(List<string> errors)
            => new ApiResponse<T> { Success = false, Errors = errors };
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T Data { get; set; }
    }
}