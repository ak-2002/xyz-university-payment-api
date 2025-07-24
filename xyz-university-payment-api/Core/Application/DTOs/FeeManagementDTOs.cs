using xyz_university_payment_api.Core.Domain.Entities;

namespace xyz_university_payment_api.Core.Application.DTOs
{
    // Fee Category DTOs
    public class FeeCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public FeeCategoryType Type { get; set; }
        public FeeFrequency Frequency { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateFeeCategoryDto
    {
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public FeeCategoryType Type { get; set; } = FeeCategoryType.Standard;
        public FeeFrequency Frequency { get; set; } = FeeFrequency.OneTime;
        public bool IsRequired { get; set; } = true;
    }

    public class UpdateFeeCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public FeeCategoryType Type { get; set; }
        public FeeFrequency Frequency { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }
    }

    // Fee Structure DTOs
    public class FeeStructureDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<FeeStructureItemDto> FeeStructureItems { get; set; } = new List<FeeStructureItemDto>();
        public decimal TotalAmount => FeeStructureItems.Sum(item => item.Amount);
    }

    public class CreateFeeStructureDto
    {
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public required string AcademicYear { get; set; }
        public required string Semester { get; set; }
        public bool IsActive { get; set; } = true;
        public List<CreateFeeStructureItemDto> FeeStructureItems { get; set; } = new List<CreateFeeStructureItemDto>();
    }

    public class UpdateFeeStructureDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<CreateFeeStructureItemDto> FeeStructureItems { get; set; } = new List<CreateFeeStructureItemDto>();
    }

    // Fee Structure Item DTOs
    public class FeeStructureItemDto
    {
        public int Id { get; set; }
        public int FeeStructureId { get; set; }
        public int FeeCategoryId { get; set; }
        public string FeeCategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsRequired { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
    }

    public class CreateFeeStructureItemDto
    {
        public int FeeCategoryId { get; set; }
        public decimal Amount { get; set; }
        public bool IsRequired { get; set; } = true;
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
    }

    // Additional Fee DTOs
    public class AdditionalFeeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public FeeFrequency Frequency { get; set; }
        public FeeApplicability ApplicableTo { get; set; }
        public string? ApplicablePrograms { get; set; }
        public string? ApplicableClasses { get; set; }
        public string? ApplicableStudents { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAdditionalFeeDto
    {
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public FeeFrequency Frequency { get; set; } = FeeFrequency.OneTime;
        public FeeApplicability ApplicableTo { get; set; } = FeeApplicability.All;
        public string? ApplicablePrograms { get; set; }
        public string? ApplicableClasses { get; set; }
        public string? ApplicableStudents { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class UpdateAdditionalFeeDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public FeeFrequency Frequency { get; set; }
        public FeeApplicability ApplicableTo { get; set; }
        public string? ApplicablePrograms { get; set; }
        public string? ApplicableClasses { get; set; }
        public string? ApplicableStudents { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
    }

    // Student Fee Assignment DTOs
    public class StudentFeeAssignmentDto
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int FeeStructureId { get; set; }
        public string FeeStructureName { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public string AssignedBy { get; set; } = string.Empty;
    }

    public class AssignFeeStructureDto
    {
        public required string StudentNumber { get; set; }
        public required int FeeStructureId { get; set; }
        public required string AcademicYear { get; set; }
        public required string Semester { get; set; }
    }

    public class BulkAssignFeeStructureDto
    {
        public required List<string> StudentNumbers { get; set; }
        public required int FeeStructureId { get; set; }
        public required string AcademicYear { get; set; }
        public required string Semester { get; set; }
    }

    public class FlexibleAssignFeeStructureDto
    {
        public int FeeStructureId { get; set; }
        public string AcademicYear { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public List<string>? StudentNumbers { get; set; }
        public List<string>? Programs { get; set; }
    }

    public class AssignFeeStructureToAllDto
    {
        public int FeeStructureId { get; set; }
    }

    public class AssignmentResultDto
    {
        public int TotalAssigned { get; set; }
        public int OutstandingBalancesAdded { get; set; }
        public decimal TotalOutstandingAmount { get; set; }
    }

    // Student Fee Balance DTOs
    public class StudentFeeBalanceDto
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int FeeStructureItemId { get; set; }
        public string FeeCategoryName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal OutstandingBalance { get; set; }
        public DateTime DueDate { get; set; }
        public FeeBalanceStatus Status { get; set; }
        public bool IsActive { get; set; }
    }

    public class StudentFeeBalanceSummaryDto
    {
        public string StudentNumber { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string Program { get; set; } = string.Empty;
        public decimal TotalOutstandingBalance { get; set; }
        public decimal TotalPaid { get; set; }
        public DateTime NextPaymentDue { get; set; }
        public List<StudentFeeBalanceDto> FeeBalances { get; set; } = new List<StudentFeeBalanceDto>();
        public List<StudentAdditionalFeeDto> AdditionalFees { get; set; } = new List<StudentAdditionalFeeDto>();
    }

    // Student Additional Fee DTOs
    public class StudentAdditionalFeeDto
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int AdditionalFeeId { get; set; }
        public string AdditionalFeeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public FeeBalanceStatus Status { get; set; }
        public DateTime AssignedAt { get; set; }
    }

    // Report DTOs
    public class FeeReportDto
    {
        public int TotalStudents { get; set; }
        public int StudentsWithOutstandingFees { get; set; }
        public decimal TotalOutstandingAmount { get; set; }
        public decimal TotalCollectedAmount { get; set; }
        public List<FeeStructureDto> ActiveFeeStructures { get; set; } = new List<FeeStructureDto>();
        public List<AdditionalFeeDto> ActiveAdditionalFees { get; set; } = new List<AdditionalFeeDto>();
    }
} 