// Purpose: Data Transfer Objects for Student operations
namespace xyz_university_payment_api.DTOs
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
}