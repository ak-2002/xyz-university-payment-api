// Purpose: Student service implementation with business logic
// Provides concrete implementation of IStudentService interface
using xyz_university_payment_api.Interfaces;
using xyz_university_payment_api.Models;
using xyz_university_payment_api.Exceptions;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StudentService> _logger;

        public StudentService(IUnitOfWork unitOfWork, ILogger<StudentService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all students");
                return await _unitOfWork.Students.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all students");
                throw new DatabaseException("Failed to retrieve students", ex);
            }
        }

        public async Task<Student?> GetStudentByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving student with ID: {StudentId}", id);
                var student = await _unitOfWork.Students.GetByIdAsync(id);
                
                if (student == null)
                {
                    throw new StudentNotFoundException(id);
                }
                
                return student;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student with ID: {StudentId}", id);
                throw new DatabaseException($"Failed to retrieve student with ID {id}", ex);
            }
        }

        public async Task<Student?> GetStudentByNumberAsync(string studentNumber)
        {
            try
            {
                _logger.LogInformation("Retrieving student with number: {StudentNumber}", studentNumber);
                var student = await _unitOfWork.Students.FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);
                
                if (student == null)
                {
                    throw new StudentNotFoundException(studentNumber);
                }
                
                return student;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student with number: {StudentNumber}", studentNumber);
                throw new DatabaseException($"Failed to retrieve student with number {studentNumber}", ex);
            }
        }

        public async Task<Student> CreateStudentAsync(Student student)
        {
            try
            {
                _logger.LogInformation("Creating new student: {StudentNumber}", student.StudentNumber);

                // Business validation
                var validation = await ValidateStudentAsync(student);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Student validation failed: {Errors}", string.Join(", ", validation.Errors));
                    throw new ValidationException(validation.Errors);
                }

                // Check for duplicate student number
                if (await _unitOfWork.Students.AnyAsync(s => s.StudentNumber == student.StudentNumber))
                {
                    _logger.LogWarning("Student number already exists: {StudentNumber}", student.StudentNumber);
                    throw new DuplicateStudentException(student.StudentNumber);
                }

                var createdStudent = await _unitOfWork.Students.AddAsync(student);
                _logger.LogInformation("Student created successfully: {StudentNumber}", student.StudentNumber);
                return createdStudent;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating student: {StudentNumber}", student.StudentNumber);
                throw new DatabaseException($"Failed to create student {student.StudentNumber}", ex);
            }
        }

        public async Task<Student> UpdateStudentAsync(Student student)
        {
            try
            {
                _logger.LogInformation("Updating student: {StudentNumber}", student.StudentNumber);

                // Business validation
                var validation = await ValidateStudentAsync(student);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Student validation failed: {Errors}", string.Join(", ", validation.Errors));
                    throw new ValidationException(validation.Errors);
                }

                // Check if student exists
                var existingStudent = await _unitOfWork.Students.GetByIdAsync(student.Id);
                if (existingStudent == null)
                {
                    _logger.LogWarning("Student not found for update: {StudentId}", student.Id);
                    throw new StudentNotFoundException(student.Id);
                }

                var updatedStudent = await _unitOfWork.Students.UpdateAsync(student);
                _logger.LogInformation("Student updated successfully: {StudentNumber}", student.StudentNumber);
                return updatedStudent;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student: {StudentNumber}", student.StudentNumber);
                throw new DatabaseException($"Failed to update student {student.StudentNumber}", ex);
            }
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting student with ID: {StudentId}", id);

                var student = await _unitOfWork.Students.GetByIdAsync(id);
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

                await _unitOfWork.Students.DeleteAsync(student);
                _logger.LogInformation("Student deleted successfully: {StudentId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student with ID: {StudentId}", id);
                throw new DatabaseException($"Failed to delete student with ID {id}", ex);
            }
        }

        public async Task<IEnumerable<Student>> GetActiveStudentsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving active students");
                return await _unitOfWork.Students.FindAsync(s => s.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active students");
                throw new DatabaseException("Failed to retrieve active students", ex);
            }
        }

        public async Task<IEnumerable<Student>> GetStudentsByProgramAsync(string program)
        {
            try
            {
                _logger.LogInformation("Retrieving students by program: {Program}", program);
                return await _unitOfWork.Students.FindAsync(s => s.Program.ToUpper() == program.ToUpper());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving students by program: {Program}", program);
                throw new DatabaseException($"Failed to retrieve students by program {program}", ex);
            }
        }

        public async Task<IEnumerable<Student>> SearchStudentsAsync(string searchTerm)
        {
            try
            {
                _logger.LogInformation("Searching students with term: {SearchTerm}", searchTerm);
                return await _unitOfWork.Students.FindAsync(s => 
                    s.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    s.StudentNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching students with term: {SearchTerm}", searchTerm);
                throw new DatabaseException($"Failed to search students with term {searchTerm}", ex);
            }
        }

        public async Task<Student> UpdateStudentStatusAsync(int studentId, bool isActive)
        {
            try
            {
                _logger.LogInformation("Updating student status: {StudentId} to {IsActive}", studentId, isActive);
                
                var student = await _unitOfWork.Students.GetByIdAsync(studentId);
                if (student == null)
                {
                    throw new StudentNotFoundException(studentId);
                }

                student.IsActive = isActive;
                var updatedStudent = await _unitOfWork.Students.UpdateAsync(student);
                _logger.LogInformation("Student status updated successfully: {StudentId}", studentId);
                return updatedStudent;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student status: {StudentId}", studentId);
                throw new DatabaseException($"Failed to update student status for ID {studentId}", ex);
            }
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
            try
            {
                _logger.LogInformation("Checking enrollment eligibility for student: {StudentNumber}", studentNumber);

                var student = await _unitOfWork.Students.FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);
                if (student == null)
                {
                    _logger.LogWarning("Student not found for enrollment check: {StudentNumber}", studentNumber);
                    return false;
                }

                // Business rule: Student must be active to be eligible for enrollment
                return student.IsActive;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking enrollment eligibility for student: {StudentNumber}", studentNumber);
                throw new DatabaseException($"Failed to check enrollment eligibility for student {studentNumber}", ex);
            }
        }

        public async Task<EnrollmentEligibilityResult> CheckEnrollmentEligibilityAsync(string studentNumber)
        {
            try
            {
                _logger.LogInformation("Checking detailed enrollment eligibility for student: {StudentNumber}", studentNumber);

                var result = new EnrollmentEligibilityResult();
                var student = await _unitOfWork.Students.FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);
                
                if (student == null)
                {
                    result.IsEligible = false;
                    result.Reasons.Add("Student not found in the system");
                    return result;
                }

                // Check if student is active
                if (!student.IsActive)
                {
                    result.Reasons.Add("Student account is not active");
                }

                // Check if student has completed required prerequisites (placeholder for future implementation)
                // This could include checking academic standing, completed courses, etc.

                // Check if student has any outstanding financial obligations (placeholder)
                // This could include checking payment status, outstanding fees, etc.

                // For now, eligibility is based on active status
                result.IsEligible = student.IsActive;
                
                if (result.IsEligible)
                {
                    result.Reasons.Add("Student meets all enrollment requirements");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking detailed enrollment eligibility for student: {StudentNumber}", studentNumber);
                throw new DatabaseException($"Failed to check enrollment eligibility for student {studentNumber}", ex);
            }
        }
    }
}