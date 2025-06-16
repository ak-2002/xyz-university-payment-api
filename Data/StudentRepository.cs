using Microsoft.EntityFrameworkCore;
using xyz_university_payment_api.Interfaces;
using xyz_university_payment_api.Models;

namespace xyz_university_payment_api.Data
{
    
    public class StudentRepository : Repository<Student>, IStudentRepository
    {
        public StudentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Student?> GetByStudentNumberAsync(string studentNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);
        }

        public async Task<IEnumerable<Student>> GetActiveStudentsAsync()
        {
            return await _dbSet.Where(s => s.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetStudentsByProgramAsync(string program)
        {
            return await _dbSet.Where(s => s.Program == program).ToListAsync();
        }

        public async Task<bool> StudentNumberExistsAsync(string studentNumber)
        {
            return await _dbSet.AnyAsync(s => s.StudentNumber == studentNumber);
        }

        public async Task<IEnumerable<Student>> SearchStudentsByNameAsync(string searchTerm)
        {
            return await _dbSet
                .Where(s => s.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToListAsync();
        }

        public async Task<Student> UpdateActiveStatusAsync(int studentId, bool isActive)
        {
            var student = await _dbSet.FindAsync(studentId);
            if (student == null)
                throw new ArgumentException($"Student with ID {studentId} not found");

            student.IsActive = isActive;
            await SaveChangesAsync();
            return student;
        }
    }
}