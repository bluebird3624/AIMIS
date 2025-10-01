// Common/ApiResponse.cs
namespace Interchée.Common
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = [];

        // Default constructor
        public ApiResponse() { }

        // Constructor for success responses
        public ApiResponse(bool success, string message = "")
        {
            Success = success;
            Message = message;
        }

        // Constructor for error responses
        public ApiResponse(bool success, List<string> errors)
        {
            Success = success;
            Errors = errors;
        }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; set; }

        // Default constructor
        public ApiResponse() { }

        // Constructor for success responses with data
        public ApiResponse(T data, bool success, string message = "")
        {
            Data = data;
            Success = success;
            Message = message;
        }

        // Constructor for error responses with data type
        public ApiResponse(T data, bool success, List<string> errors)
        {
            Data = data;
            Success = success;
            Errors = errors;
        }
    }
}