using xyz_university_payment_api.Models;


namespace xyz_university_payment_api.Interfaces
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
    }

    public class EnrollmentEligibilityResult
    {
        public bool IsEligible { get; set; }
        public List<string> Reasons { get; set; } = new List<string>();
    }
} 