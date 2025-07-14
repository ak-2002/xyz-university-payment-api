namespace xyz_university_payment_api.Core.Application.DTOs
{
    public class FeeScheduleDto
    {
        public int Id { get; set; }
        public string Semester { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public string Program { get; set; } = string.Empty;
        public decimal TuitionFee { get; set; }
        public decimal RegistrationFee { get; set; }
        public decimal LibraryFee { get; set; }
        public decimal LaboratoryFee { get; set; }
        public decimal OtherFees { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Description { get; set; }
    }

    public class CreateFeeScheduleDto
    {
        public string Semester { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public string Program { get; set; } = string.Empty;
        public decimal TuitionFee { get; set; }
        public decimal RegistrationFee { get; set; }
        public decimal LibraryFee { get; set; }
        public decimal LaboratoryFee { get; set; }
        public decimal OtherFees { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DueDate { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateFeeScheduleDto
    {
        public decimal TuitionFee { get; set; }
        public decimal RegistrationFee { get; set; }
        public decimal LibraryFee { get; set; }
        public decimal LaboratoryFee { get; set; }
        public decimal OtherFees { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
    }

    public class StudentBalanceDto
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public int FeeScheduleId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal OutstandingBalance { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Notes { get; set; }
        
        // Additional properties for display
        public string StudentName { get; set; } = string.Empty;
        public string Program { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
    }

    public class CreateStudentBalanceDto
    {
        public string StudentNumber { get; set; } = string.Empty;
        public int FeeScheduleId { get; set; }
        public DateTime DueDate { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateStudentBalanceDto
    {
        public decimal AmountPaid { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
    }

    public class PaymentPlanDto
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public int StudentBalanceId { get; set; }
        public string PlanType { get; set; } = string.Empty;
        public int TotalInstallments { get; set; }
        public int CompletedInstallments { get; set; }
        public decimal InstallmentAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal RemainingAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Notes { get; set; }
        
        // Additional properties for display
        public string StudentName { get; set; } = string.Empty;
        public string Program { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
    }

    public class CreatePaymentPlanDto
    {
        public string StudentNumber { get; set; } = string.Empty;
        public int StudentBalanceId { get; set; }
        public string PlanType { get; set; } = string.Empty;
        public int TotalInstallments { get; set; }
        public decimal InstallmentAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdatePaymentPlanDto
    {
        public int CompletedInstallments { get; set; }
        public decimal AmountPaid { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
    }

    public class StudentBalanceSummaryDto
    {
        public string StudentNumber { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string Program { get; set; } = string.Empty;
        public string CurrentSemester { get; set; } = string.Empty;
        public decimal TotalOutstandingBalance { get; set; }
        public decimal CurrentSemesterBalance { get; set; }
        public decimal TotalPaid { get; set; }
        public DateTime NextPaymentDue { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public List<StudentBalanceDto> SemesterBalances { get; set; } = new List<StudentBalanceDto>();
    }

    public class OutstandingBalanceReportDto
    {
        public int TotalStudents { get; set; }
        public int StudentsWithBalance { get; set; }
        public decimal TotalOutstandingAmount { get; set; }
        public decimal AverageOutstandingAmount { get; set; }
        public List<StudentBalanceSummaryDto> TopOutstandingBalances { get; set; } = new List<StudentBalanceSummaryDto>();
        public Dictionary<string, decimal> OutstandingByProgram { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> OutstandingBySemester { get; set; } = new Dictionary<string, decimal>();
    }
} 