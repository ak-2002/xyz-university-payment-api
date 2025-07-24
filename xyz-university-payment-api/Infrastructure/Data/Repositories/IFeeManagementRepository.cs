using xyz_university_payment_api.Core.Domain.Entities;

namespace xyz_university_payment_api.Infrastructure.Data.Repositories
{
    public interface IFeeManagementRepository
    {
        // Fee Category operations
        Task<List<FeeCategory>> GetAllFeeCategoriesAsync();
        Task<FeeCategory?> GetFeeCategoryByIdAsync(int id);
        Task<FeeCategory> CreateFeeCategoryAsync(FeeCategory feeCategory);
        Task<FeeCategory> UpdateFeeCategoryAsync(FeeCategory feeCategory);
        Task<bool> DeleteFeeCategoryAsync(int id);

        // Fee Structure operations
        Task<List<FeeStructure>> GetAllFeeStructuresAsync();
        Task<List<FeeStructure>> GetAllFeeStructuresIncludingInactiveAsync();
        Task<FeeStructure?> GetFeeStructureByIdAsync(int id);
        Task<List<FeeStructure>> GetFeeStructuresByAcademicYearAsync(string academicYear, string semester);
        Task<FeeStructure> CreateFeeStructureAsync(FeeStructure feeStructure);
        Task<FeeStructure> UpdateFeeStructureAsync(FeeStructure feeStructure);
        Task<bool> DeleteFeeStructureAsync(int id);

        // Fee Structure Item operations
        Task<List<FeeStructureItem>> GetFeeStructureItemsByStructureIdAsync(int feeStructureId);
        Task<FeeStructureItem?> GetFeeStructureItemByIdAsync(int id);
        Task<FeeStructureItem> CreateFeeStructureItemAsync(FeeStructureItem item);
        Task<FeeStructureItem> UpdateFeeStructureItemAsync(FeeStructureItem item);
        Task<bool> DeleteFeeStructureItemAsync(int id);

        // Additional Fee operations
        Task<List<AdditionalFee>> GetAllAdditionalFeesAsync();
        Task<AdditionalFee?> GetAdditionalFeeByIdAsync(int id);
        Task<AdditionalFee> CreateAdditionalFeeAsync(AdditionalFee additionalFee);
        Task<AdditionalFee> UpdateAdditionalFeeAsync(AdditionalFee additionalFee);
        Task<bool> DeleteAdditionalFeeAsync(int id);

        // Student Fee Assignment operations
        Task<List<StudentFeeAssignment>> GetStudentFeeAssignmentsAsync(string studentNumber);
        Task<StudentFeeAssignment?> GetStudentFeeAssignmentByIdAsync(int id);
        Task<StudentFeeAssignment> CreateStudentFeeAssignmentAsync(StudentFeeAssignment assignment);
        Task<bool> DeleteStudentFeeAssignmentAsync(int id);
        Task<bool> ExistsStudentFeeAssignmentAsync(string studentNumber, int feeStructureId, string academicYear, string semester);
        Task<List<StudentFeeAssignment>> GetAllStudentFeeAssignmentsAsync();

        // Student Fee Balance operations
        Task<List<StudentFeeBalance>> GetStudentFeeBalancesAsync(string studentNumber);
        Task<StudentFeeBalance?> GetStudentFeeBalanceByIdAsync(int id);
        Task<StudentFeeBalance?> GetStudentFeeBalanceByStudentAndItemAsync(string studentNumber, int feeStructureItemId);
        Task<List<StudentFeeBalance>> GetStudentFeeBalancesByStatusAsync(FeeBalanceStatus status);
        Task<List<StudentFeeBalance>> GetAllStudentFeeBalancesAsync();
        Task<StudentFeeBalance> CreateStudentFeeBalanceAsync(StudentFeeBalance balance);
        Task<StudentFeeBalance> UpdateStudentFeeBalanceAsync(StudentFeeBalance balance);
        Task<bool> DeleteStudentFeeBalanceAsync(int id);

        // Payment operations
        Task<List<PaymentNotification>> GetAllPaymentsAsync();

        // Student Additional Fee operations
        Task<List<StudentAdditionalFee>> GetStudentAdditionalFeesAsync(string studentNumber);
        Task<StudentAdditionalFee?> GetStudentAdditionalFeeByIdAsync(int id);
        Task<StudentAdditionalFee?> GetStudentAdditionalFeeByStudentAndFeeAsync(string studentNumber, int additionalFeeId);
        Task<StudentAdditionalFee> CreateStudentAdditionalFeeAsync(StudentAdditionalFee studentAdditionalFee);
        Task<StudentAdditionalFee> UpdateStudentAdditionalFeeAsync(StudentAdditionalFee studentAdditionalFee);
        Task<bool> DeleteStudentAdditionalFeeAsync(int id);

        // Utility operations
        Task<List<Student>> GetStudentsByProgramAsync(string program);
        Task<List<Student>> GetStudentsByClassAsync(string className);
        Task<List<Student>> GetStudentsByNumbersAsync(List<string> studentNumbers);
        Task<int> SaveChangesAsync();
        
        // Migration operations
        Task<List<StudentBalance>> GetAllOldStudentBalancesAsync();
        Task<FeeStructureItem?> GetOrCreateFeeStructureItemForOldBalanceAsync(StudentBalance oldBalance);
        Task<List<Student>> GetAllStudentsAsync();
    }
} 