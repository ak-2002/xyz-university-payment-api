// Purpose: Handles student validation logic
using xyz_university_payment_api.Data;
using xyz_university_payment_api.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace xyz_university_payment_api.Services
{
    public class StudentService
    {
        private readonly AppDbContext _context;
         private readonly ILogger<StudentService> _logger;

        public StudentService(AppDbContext context, ILogger<StudentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Validates if a student number exists and is active
        public async Task<Student> ValidateStudentAsync(string studentNumber)
        {
            _logger.LogInformation("Validating student number: {StudentNumber}", studentNumber);
            var student =  await _context.Students.FirstOrDefaultAsync(s => s.StudentNumber == studentNumber && s.IsActive);
            if (student == null)
            {
                _logger.LogWarning("Student not found or inactive: {StudentNumber}", studentNumber);
            }

            return student;
        }
    }
}