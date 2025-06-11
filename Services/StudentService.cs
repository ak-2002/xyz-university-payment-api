// Purpose: Handles student validation logic
using xyz_university_payment_api.Data;
using xyz_university_payment_api.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace xyz_university_payment_api.Services
{
    public class StudentService
    {
        private readonly AppDbContext _context;

        public StudentService(AppDbContext context)
        {
            _context = context;
        }

        // Validates if a student number exists and is active
        public async Task<Student> ValidateStudentAsync(string studentNumber)
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.StudentNumber == studentNumber && s.IsActive);
        }
    }
}