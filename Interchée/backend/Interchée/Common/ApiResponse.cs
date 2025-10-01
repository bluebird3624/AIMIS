// Common/ApiResponse.cs

namespace Interchee.Common
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Error { get; set; } = new List<string>();

        internal static object? Errors(string v)
        {
            throw new NotImplementedException();
        }

        // Renamed from 'Success' to 'SuccessResponse' to avoid CS0102
        internal static object? SuccessResponse(object interns)
        {
            throw new NotImplementedException();
        }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; set; }
    }
}