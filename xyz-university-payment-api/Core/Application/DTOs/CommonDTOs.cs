using System.ComponentModel.DataAnnotations;

// Purpose: Common Data Transfer Objects used across the application
namespace xyz_university_payment_api.Core.Application.DTOs
{
    // Generic pagination result
    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
    }

    // Pagination parameters
    public class PaginationDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; } = "asc"; // "asc" or "desc"
    }

    // Standard API response wrapper
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? RequestId { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    // Alias for consistency with AuthorizationController
    public class ApiResponse<T> : ApiResponseDto<T>
    {
    }

    // Error response DTO
    public class ErrorResponseDto
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string? Instance { get; set; }
        public int StatusCode { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? RequestId { get; set; }
        public object? AdditionalData { get; set; }
    }

    // Date range filter DTO
    public class DateRangeDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    // Search and filter DTO
    public class SearchFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public Dictionary<string, object> Filters { get; set; } = new Dictionary<string, object>();
    }

    // Bulk operation result DTO
    public class GenericBulkOperationResultDto<T>
    {
        public int TotalItems { get; set; }
        public int SuccessfulItems { get; set; }
        public int FailedItems { get; set; }
        public List<T> SuccessfulResults { get; set; } = new List<T>();
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime OperationDate { get; set; } = DateTime.UtcNow;
    }

    // Health check DTO
    public class HealthCheckDto
    {
        public string Status { get; set; } = string.Empty;
        public DateTime CheckTime { get; set; } = DateTime.UtcNow;
        public TimeSpan Uptime { get; set; }
        public Dictionary<string, object> Components { get; set; } = new Dictionary<string, object>();
    }

    // Audit log DTO
    public class AuditLogDto
    {
        public int Id { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // Create, Update, Delete
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    public class PaginatedResponseDto<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    public class UpdateStatusRequest
    {
        [Required(ErrorMessage = "IsActive status is required")]
        public bool IsActive { get; set; }
    }
}