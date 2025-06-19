// Purpose: Specific repository interface for Student entity
// This extends the generic repository and adds Student-specific business logic methods
using xyz_university_payment_api.Models;

namespace xyz_university_payment_api.Interfaces
{
    /// <summary>
    /// Repository interface specifically for Student entity operations
    /// Extends the generic repository and adds Student-specific business logic methods
    /// This follows the Interface Segregation Principle by providing specific methods for Student operations
    /// </summary>
    public interface IStudentRepository : IRepository<Student>
    {
        /// <summary>
        /// Retrieves a student by their student number (business identifier)
        /// This is different from ID as student numbers are business keys, not database keys
        /// </summary>
        /// <param name="studentNumber">The unique student number</param>
        /// <returns>The student if found, null otherwise</returns>
        Task<Student?> GetByStudentNumberAsync(string studentNumber);

        /// <summary>
        /// Retrieves all active students (where IsActive = true)
        /// This is a common business requirement for enrollment systems
        /// </summary>
        /// <returns>Collection of active students</returns>
        Task<IEnumerable<Student>> GetActiveStudentsAsync();

        /// <summary>
        /// Retrieves all students in a specific program
        /// Useful for program-specific operations and reporting
        /// </summary>
        /// <param name="program">The program name to filter by</param>
        /// <returns>Collection of students in the specified program</returns>
        Task<IEnumerable<Student>> GetStudentsByProgramAsync(string program);

        /// <summary>
        /// Checks if a student number already exists in the database
        /// Important for validation before creating new students
        /// </summary>
        /// <param name="studentNumber">The student number to check</param>
        /// <returns>True if the student number exists, false otherwise</returns>
        Task<bool> StudentNumberExistsAsync(string studentNumber);

        /// <summary>
        /// Searches students by name (partial match)
        /// Useful for search functionality in the UI
        /// </summary>
        /// <param name="searchTerm">The name or partial name to search for</param>
        /// <returns>Collection of students matching the search term</returns>
        Task<IEnumerable<Student>> SearchStudentsByNameAsync(string searchTerm);

        /// <summary>
        /// Updates a student's active status
        /// Common operation for enrollment management
        /// </summary>
        /// <param name="studentId">The ID of the student</param>
        /// <param name="isActive">The new active status</param>
        /// <returns>The updated student</returns>
        Task<Student> UpdateActiveStatusAsync(int studentId, bool isActive);
    }
} 