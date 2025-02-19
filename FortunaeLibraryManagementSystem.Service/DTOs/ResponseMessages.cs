

using System.Diagnostics;

namespace FortunaeLibraryManagementSystem.Service.DTOs;
public class ResponseMessages
{
    public class ApiErrorResponse
    {
        public int Status { get; set; }
        public string? ErrorCode { get; set; }
        public string? Message { get; set; }
        public IDictionary<string, string[]>? Errors { get; set; }
        public DeveloperMessage? DeveloperMessage { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public double RuntimeSeconds { get; set; }
    }

    public class DeveloperMessage
    {
        public string? Exception { get; set; }
        public string? StackTrace { get; set; }
        public string? InnerException { get; set; }
    }

    public class ApiSuccessResponse<T>
    {
        public int Status { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public double RuntimeSeconds { get; set; }

        // Pagination details (only included if response contains a paginated list)
        public PaginationInfo? Pagination { get; set; }

        public static ApiSuccessResponse<T> Create(T data, Stopwatch stopwatch, int? pageNumber = null, int? pageSize = null, int? totalCount = null)
        {
            return new ApiSuccessResponse<T>
            {
                Status = 200,
                Message = "Request successful",
                Data = data,
                RuntimeSeconds = stopwatch.Elapsed.TotalSeconds,
                Pagination = (pageNumber.HasValue && pageSize.HasValue && totalCount.HasValue)
                    ? new PaginationInfo
                    {
                        PageNumber = pageNumber.Value,
                        PageSize = pageSize.Value,
                        TotalCount = totalCount.Value,
                        TotalPages = (int)Math.Ceiling(totalCount.Value / (double)pageSize.Value)
                    }
                    : null
            };
        }
    }

    public class PaginationInfo
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}
