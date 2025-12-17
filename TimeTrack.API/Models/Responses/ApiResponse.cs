namespace TimeTrack.API.Models.Responses
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Result { get; set; }

        public static ApiResponse<T> Ok(T result, string? message = null)
            => new()
            {
                Success = true,
                Message = message,
                Result = result
            };

        public static ApiResponse<T> Fail(string message)
            => new()
            {
                Success = false,
                Message = message,
                Result = default
            };
    }
}
