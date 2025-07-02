// Purpose: Data Transfer Objects for Student operations
namespace xyz_university_payment_api.Core.Application.DTOs
{
    // Input DTO for creating new students
    public class CreateStudentDto
    {
        public string StudentNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Program { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
    }

    // Input DTO for updating existing students
    public class UpdateStudentDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Program { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    // Output DTO for returning student data to clients
    public class StudentDto
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Program { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // DTO for student summary with payment information
    public class StudentSummaryDto
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Program { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int TotalPayments { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public decimal OutstandingBalance { get; set; }
        public DateTime LastPaymentDate { get; set; }
        public DateTime EnrollmentDate { get; set; }
    }

    // DTO for student search and filtering
    public class StudentSearchDto
    {
        public string? StudentNumber { get; set; }
        public string? FullName { get; set; }
        public string? Program { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? EnrollmentDateFrom { get; set; }
        public DateTime? EnrollmentDateTo { get; set; }
    }

    // DTO for student response
    public class StudentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public StudentDto? Student { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();
    }

    // DTO for student validation
    public class StudentValidationDto
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string StudentNumber { get; set; } = string.Empty;
    }

    // DTO for student statistics
    public class StudentStatisticsDto
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int InactiveStudents { get; set; }
        public Dictionary<string, int> StudentsByProgram { get; set; } = new Dictionary<string, int>();
        public decimal TotalRevenue { get; set; }
        public decimal AveragePaymentAmount { get; set; }
        public int StudentsWithOutstandingBalance { get; set; }
    }

    // V3 DTOs
    public class StudentDtoV3
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Program { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public StudentStatisticsDto Statistics { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class StudentDetailDtoV3 : StudentDtoV3
    {
        public List<PaymentDto> PaymentHistory { get; set; } = new();
        public StudentProfileDto Profile { get; set; } = new();
        public List<StudentActivityDto> RecentActivity { get; set; } = new();
        public StudentPreferencesDto Preferences { get; set; } = new();
    }

    public class CreateStudentDtoV3
    {
        public string StudentNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Program { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class UpdateStudentDtoV3
    {
        public string? FullName { get; set; }
        public string? Program { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public bool? IsActive { get; set; }
        public List<string>? Tags { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class StudentFilterDtoV3
    {
        public string? StudentNumber { get; set; }
        public string? FullName { get; set; }
        public string? Program { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public List<string>? Tags { get; set; }
        public decimal? MinTotalPayments { get; set; }
        public decimal? MaxTotalPayments { get; set; }
        public string? SearchTerm { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class StudentProfileDto
    {
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public string EmergencyContact { get; set; } = string.Empty;
        public string EmergencyPhone { get; set; } = string.Empty;
    }

    public class StudentActivityDto
    {
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Details { get; set; } = new();
    }

    public class StudentPreferencesDto
    {
        public string PreferredLanguage { get; set; } = "en";
        public string TimeZone { get; set; } = "UTC";
        public bool EmailNotifications { get; set; } = true;
        public bool SmsNotifications { get; set; } = false;
        public List<string> NotificationTypes { get; set; } = new();
    }

    public class StudentAnalyticsFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Program { get; set; }
        public string? GroupBy { get; set; } // "program", "month", "year"
        public bool IncludeInactive { get; set; } = false;
    }

    public class StudentAnalyticsDto
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int InactiveStudents { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRevenuePerStudent { get; set; }
        public Dictionary<string, int> StudentsByProgram { get; set; } = new();
        public Dictionary<string, decimal> RevenueByMonth { get; set; } = new();
        public List<StudentTrendDto> Trends { get; set; } = new();
        public Dictionary<string, object> CustomMetrics { get; set; } = new();
    }

    public class StudentTrendDto
    {
        public string Period { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public decimal Revenue { get; set; }
        public double GrowthRate { get; set; }
    }

    public class BulkStudentOperationDto
    {
        public string Operation { get; set; } = string.Empty; // "activate", "deactivate", "update", "delete"
        public List<int> StudentIds { get; set; } = new();
        public Dictionary<string, object>? UpdateData { get; set; }
        public bool SkipValidation { get; set; } = false;
    }

    public class BulkOperationResultDto
    {
        public int TotalProcessed { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<BulkOperationDetailDto> Details { get; set; } = new();
    }

    public class BulkOperationDetailDto
    {
        public int StudentId { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, object>? Changes { get; set; }
    }
} 