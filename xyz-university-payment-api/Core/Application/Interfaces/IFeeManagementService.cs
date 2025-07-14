using xyz_university_payment_api.Core.Application.DTOs;
using xyz_university_payment_api.Core.Domain.Entities;

namespace xyz_university_payment_api.Core.Application.Interfaces
{
    public interface IFeeManagementService
    {
        // Fee Category Operations
        Task<List<FeeCategoryDto>> GetAllFeeCategoriesAsync();
        Task<FeeCategoryDto?> GetFeeCategoryByIdAsync(int id);
        Task<FeeCategoryDto> CreateFeeCategoryAsync(CreateFeeCategoryDto dto);
        Task<FeeCategoryDto> UpdateFeeCategoryAsync(int id, UpdateFeeCategoryDto dto);
        Task<bool> DeleteFeeCategoryAsync(int id);

        // Fee Structure Operations
        Task<List<FeeStructureDto>> GetAllFeeStructuresAsync();
        Task<FeeStructureDto?> GetFeeStructureByIdAsync(int id);
        Task<List<FeeStructureDto>> GetFeeStructuresByAcademicYearAsync(string academicYear, string semester);
        Task<FeeStructureDto> CreateFeeStructureAsync(CreateFeeStructureDto dto);
        Task<FeeStructureDto> UpdateFeeStructureAsync(int id, UpdateFeeStructureDto dto);
        Task<bool> DeleteFeeStructureAsync(int id);

        // Additional Fee Operations
        Task<List<AdditionalFeeDto>> GetAllAdditionalFeesAsync();
        Task<AdditionalFeeDto?> GetAdditionalFeeByIdAsync(int id);
        Task<AdditionalFeeDto> CreateAdditionalFeeAsync(CreateAdditionalFeeDto dto);
        Task<AdditionalFeeDto> UpdateAdditionalFeeAsync(int id, UpdateAdditionalFeeDto dto);
        Task<bool> DeleteAdditionalFeeAsync(int id);

        // Student Fee Assignment Operations
        Task<List<StudentFeeAssignmentDto>> GetStudentFeeAssignmentsAsync(string studentNumber);
        Task<StudentFeeAssignmentDto> AssignFeeStructureToStudentAsync(AssignFeeStructureDto dto);
        Task<List<StudentFeeAssignmentDto>> BulkAssignFeeStructureAsync(BulkAssignFeeStructureDto dto);
        Task<bool> RemoveFeeStructureAssignmentAsync(int assignmentId);

        // Student Fee Balance Operations
        Task<StudentFeeBalanceSummaryDto> GetStudentFeeBalanceSummaryAsync(string studentNumber);
        Task<List<StudentFeeBalanceDto>> GetStudentFeeBalancesAsync(string studentNumber);
        Task<List<StudentFeeBalanceDto>> GetStudentFeeBalancesByStatusAsync(FeeBalanceStatus status);
        Task<StudentFeeBalanceDto> UpdateStudentFeeBalanceAsync(int balanceId, decimal amountPaid);
        Task<bool> RecalculateStudentBalancesAsync(string studentNumber);

        // Student Additional Fee Operations
        Task<List<StudentAdditionalFeeDto>> GetStudentAdditionalFeesAsync(string studentNumber);
        Task<StudentAdditionalFeeDto> AssignAdditionalFeeToStudentAsync(string studentNumber, int additionalFeeId);
        Task<bool> RemoveAdditionalFeeFromStudentAsync(int studentAdditionalFeeId);

        // Report Operations
        Task<FeeReportDto> GetFeeReportAsync();
        Task<FeeReportDto> GetFeeReportByAcademicYearAsync(string academicYear, string semester);
        Task<List<StudentFeeBalanceSummaryDto>> GetStudentsWithOutstandingFeesAsync();
        Task<List<StudentFeeBalanceSummaryDto>> GetStudentsWithOverdueFeesAsync();

        // Utility Operations
        Task<bool> ApplyAdditionalFeesToStudentsAsync(int additionalFeeId);
        Task<bool> UpdateFeeBalanceStatusesAsync();
        Task<bool> GenerateFeeBalancesForNewAssignmentsAsync();
    }
} 