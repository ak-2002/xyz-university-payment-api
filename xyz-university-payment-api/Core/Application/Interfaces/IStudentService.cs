using xyz_university_payment_api.Core.Domain.Entities;
using xyz_university_payment_api.Core.Application.DTOs;

namespace xyz_university_payment_api.Core.Application.Interfaces
{

    public interface IStudentService
    {

        Task<IEnumerable<Student>> GetAllStudentsAsync();


        Task<Student?> GetStudentByIdAsync(int id);


        Task<Student?> GetStudentByNumberAsync(string studentNumber);


        Task<Student> CreateStudentAsync(Student student);


        Task<Student> UpdateStudentAsync(Student student);


        Task<bool> DeleteStudentAsync(int id);


        Task<IEnumerable<Student>> GetActiveStudentsAsync();


        Task<IEnumerable<Student>> GetStudentsByProgramAsync(string program);

        Task<IEnumerable<Student>> SearchStudentsAsync(string searchTerm);


        Task<Student> UpdateStudentStatusAsync(int studentId, bool isActive);


        Task<(bool IsValid, List<string> Errors)> ValidateStudentAsync(Student student);

        Task<bool> IsStudentEligibleForEnrollmentAsync(string studentNumber);

        // Enhanced enrollment eligibility check
        Task<EnrollmentEligibilityResult> CheckEnrollmentEligibilityAsync(string studentNumber);

        // V3 Methods
        Task<ApiResponse<PaginatedResponseDto<StudentDtoV3>>> GetStudentsV3Async(
            StudentFilterDtoV3 filter, int page, int pageSize, string sortBy, string sortOrder, bool includeAnalytics);

        Task<ApiResponse<StudentDetailDtoV3>> GetStudentV3Async(int id, bool includePaymentHistory);

        Task<ApiResponse<StudentDtoV3>> CreateStudentV3Async(CreateStudentDtoV3 createStudentDto);

        Task<ApiResponse<StudentDtoV3>> UpdateStudentV3Async(int id, UpdateStudentDtoV3 updateStudentDto);

        Task<ApiResponse<bool>> DeleteStudentV3Async(int id, bool permanent);

        Task<ApiResponse<StudentAnalyticsDto>> GetStudentAnalyticsAsync(StudentAnalyticsFilterDto filter);

        Task<ApiResponse<BulkOperationResultDto>> BulkOperationsAsync(BulkStudentOperationDto bulkOperation);

        Task<ApiResponse<byte[]>> ExportStudentsAsync(StudentFilterDtoV3 filter, string format, bool includePaymentHistory);
    }

    public class EnrollmentEligibilityResult
    {
        public bool IsEligible { get; set; }
        public List<string> Reasons { get; set; } = new List<string>();
    }
}