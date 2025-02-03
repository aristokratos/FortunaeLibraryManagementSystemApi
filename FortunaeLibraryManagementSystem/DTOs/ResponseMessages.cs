namespace FortunaeLibraryManagementSystem.DTOs
{
    public class ResponseMessages
    {
        public class ApiErrorResponse
        {
            public int Status { get; set; }
            public string ErrorCode { get; set; }
            public string Message { get; set; }
            public IDictionary<string, string[]> Errors { get; set; }
            public DeveloperMessage DeveloperMessage { get; set; }
        }

        public class DeveloperMessage
        {
            public string Exception { get; set; }
            public string StackTrace { get; set; }
            public string InnerException { get; set; }
        }
    }
}
