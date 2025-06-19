// Purpose: Student service implementation with business logic
// Provides concrete implementation of IStudentService interface
using xyz_university_payment_api.Interfaces;
using xyz_university_payment_api.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace xyz_university_payment_api.Services
{
    /// <summary>
    /// Student service implementation
    /// Provides concrete implementation of IStudentService interface with business logic
    /// </summary>
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ILogger<StudentService> _logger;

        public StudentService(IStudentRepository studentRepository, ILogger<StudentService> logger)
        {
            _studentRepository = studentRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            _logger.LogInformation("Retrieving all students");
            return await _studentRepository.GetAllAsync();
        }

        public async Task<Student?> GetStudentByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving student with ID: {StudentId}", id);
            return await _studentRepository.GetByIdAsync(id);
        }

        public async Task<Student?> GetStudentByNumberAsync(string studentNumber)
        {
            _logger.LogInformation("Retrieving student with number: {StudentNumber}", studentNumber);
            return await _studentRepository.GetByStudentNumberAsync(studentNumber);
        }

        public async Task<Student> CreateStudentAsync(Student student)
        {
            _logger.LogInformation("Creating new student: {StudentNumber}", student.StudentNumber);

            // Business validation
            var validation = await ValidateStudentAsync(student);
            if (!validation.IsValid)
            {
                _logger.LogWarning("Student validation failed: {Errors}", string.Join(", ", validation.Errors));
                throw new ArgumentException($"Student validation failed: {string.Join(", ", validation.Errors)}");
            }

            // Check for duplicate student number
            if (await _studentRepository.StudentNumberExistsAsync(student.StudentNumber))
            {
                _logger.LogWarning("Student number already exists: {StudentNumber}", student.StudentNumber);
                throw new ArgumentException($"Student number {student.StudentNumber} already exists");
            }

            var createdStudent = await _studentRepository.AddAsync(student);
            _logger.LogInformation("Student created successfully: {StudentNumber}", student.StudentNumber);
            return createdStudent;
        }

        public async Task<Student> UpdateStudentAsync(Student student)
        {
            _logger.LogInformation("Updating student: {StudentNumber}", student.StudentNumber);

            // Business validation
            var validation = await ValidateStudentAsync(student);
            if (!validation.IsValid)
            {
                _logger.LogWarning("Student validation failed: {Errors}", string.Join(", ", validation.Errors));
                throw new ArgumentException($"Student validation failed: {string.Join(", ", validation.Errors)}");
            }

            // Check if student exists
            var existingStudent = await _studentRepository.GetByIdAsync(student.Id);
            if (existingStudent == null)
            {
                _logger.LogWarning("Student not found for update: {StudentId}", student.Id);
                throw new ArgumentException($"Student with ID {student.Id} not found");
            }

            var updatedStudent = await _studentRepository.UpdateAsync(student);
            _logger.LogInformation("Student updated successfully: {StudentNumber}", student.StudentNumber);
            return updatedStudent;
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            _logger.LogInformation("Deleting student with ID: {StudentId}", id);

            var student = await _studentRepository.GetByIdAsync(id);
            if (student == null)
            {
                _logger.LogWarning("Student not found for deletion: {StudentId}", id);
                return false;
            }

            // Business rule: Check if student can be deleted
            if (student.IsActive)
            {
                _logger.LogWarning("Cannot delete active student: {StudentNumber}", student.StudentNumber);
                throw new InvalidOperationException($"Cannot delete active student {student.StudentNumber}");
            }

            var result = await _studentRepository.DeleteAsync(student);
            _logger.LogInformation("Student deleted successfully: {StudentId}", id);
            return result;
        }

        public async Task<IEnumerable<Student>> GetActiveStudentsAsync()
        {
            _logger.LogInformation("Retrieving active students");
            return await _studentRepository.GetActiveStudentsAsync();
        }

        public async Task<IEnumerable<Student>> GetStudentsByProgramAsync(string program)
        {
            _logger.LogInformation("Retrieving students by program: {Program}", program);
            return await _studentRepository.GetStudentsByProgramAsync(program);
        }

        public async Task<IEnumerable<Student>> SearchStudentsAsync(string searchTerm)
        {
            _logger.LogInformation("Searching students with term: {SearchTerm}", searchTerm);
            return await _studentRepository.SearchStudentsByNameAsync(searchTerm);
        }

        public async Task<Student> UpdateStudentStatusAsync(int studentId, bool isActive)
        {
            _logger.LogInformation("Updating student status: {StudentId} to {IsActive}", studentId, isActive);
            return await _studentRepository.UpdateActiveStatusAsync(studentId, isActive);
        }

        public Task<(bool IsValid, List<string> Errors)> ValidateStudentAsync(Student student)
        {
            var errors = new List<string>();

            // Validate student number format
            if (string.IsNullOrWhiteSpace(student.StudentNumber))
            {
                errors.Add("Student number is required");
            }
            else if (!student.StudentNumber.StartsWith("S") || student.StudentNumber.Length < 6)
            {
                errors.Add("Student number must start with 'S' and be at least 6 characters long");
            }

            // Validate full name
            if (string.IsNullOrWhiteSpace(student.FullName))
            {
                errors.Add("Full name is required");
            }
            else if (student.FullName.Length < 2)
            {
                errors.Add("Full name must be at least 2 characters long");
            }

            // Validate program
            if (string.IsNullOrWhiteSpace(student.Program))
            {
                errors.Add("Program is required");
            }

            // Validate program against allowed programs
            var allowedPrograms = new[] { "CS", "IT", "ACC", "SOCIOLOGY", "ENGINEERING", "MEDICINE" };
            if (!allowedPrograms.Contains(student.Program.ToUpper()))
            {
                errors.Add($"Program must be one of: {string.Join(", ", allowedPrograms)}");
            }

            return Task.FromResult((errors.Count == 0, errors));
        }

        public async Task<bool> IsStudentEligibleForEnrollmentAsync(string studentNumber)
        {
            _logger.LogInformation("Checking enrollment eligibility for student: {StudentNumber}", studentNumber);

            var student = await _studentRepository.GetByStudentNumberAsync(studentNumber);
            if (student == null)
            {
                _logger.LogWarning("Student not found for enrollment check: {StudentNumber}", studentNumber);
                return false;
            }

            // Business rule: Student must be active to be eligible for enrollment
            return student.IsActive;
        }
    }
}