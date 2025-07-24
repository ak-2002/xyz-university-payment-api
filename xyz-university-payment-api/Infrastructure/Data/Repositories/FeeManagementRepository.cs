using Microsoft.EntityFrameworkCore;
using xyz_university_payment_api.Core.Domain.Entities;
using xyz_university_payment_api.Infrastructure.Data.Repositories;

namespace xyz_university_payment_api.Infrastructure.Data.Repositories
{
    public class FeeManagementRepository : IFeeManagementRepository
    {
        private readonly AppDbContext _context;

        public FeeManagementRepository(AppDbContext context)
        {
            _context = context;
        }

        // Fee Category operations
        public async Task<List<FeeCategory>> GetAllFeeCategoriesAsync()
        {
            return await _context.FeeCategories
                .Where(fc => fc.IsActive)
                .OrderBy(fc => fc.Name)
                .ToListAsync();
        }

        public async Task<FeeCategory?> GetFeeCategoryByIdAsync(int id)
        {
            return await _context.FeeCategories
                .Include(fc => fc.FeeStructureItems)
                .FirstOrDefaultAsync(fc => fc.Id == id);
        }

        public async Task<FeeCategory> CreateFeeCategoryAsync(FeeCategory feeCategory)
        {
            _context.FeeCategories.Add(feeCategory);
            await _context.SaveChangesAsync();
            return feeCategory;
        }

        public async Task<FeeCategory> UpdateFeeCategoryAsync(FeeCategory feeCategory)
        {
            feeCategory.UpdatedAt = DateTime.UtcNow;
            _context.FeeCategories.Update(feeCategory);
            await _context.SaveChangesAsync();
            return feeCategory;
        }

        public async Task<bool> DeleteFeeCategoryAsync(int id)
        {
            var feeCategory = await _context.FeeCategories.FindAsync(id);
            if (feeCategory == null) return false;

            feeCategory.IsActive = false;
            feeCategory.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // Fee Structure operations
        public async Task<List<FeeStructure>> GetAllFeeStructuresAsync()
        {
            return await _context.FeeStructures
                .Where(fs => fs.IsActive)
                .Include(fs => fs.FeeStructureItems)
                    .ThenInclude(fsi => fsi.FeeCategory)
                .OrderByDescending(fs => fs.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<FeeStructure>> GetAllFeeStructuresIncludingInactiveAsync()
        {
            return await _context.FeeStructures
                .Include(fs => fs.FeeStructureItems)
                    .ThenInclude(fsi => fsi.FeeCategory)
                .OrderByDescending(fs => fs.CreatedAt)
                .ToListAsync();
        }

        public async Task<FeeStructure?> GetFeeStructureByIdAsync(int id)
        {
            return await _context.FeeStructures
                .Include(fs => fs.FeeStructureItems)
                    .ThenInclude(fsi => fsi.FeeCategory)
                .FirstOrDefaultAsync(fs => fs.Id == id);
        }

        public async Task<List<FeeStructure>> GetFeeStructuresByAcademicYearAsync(string academicYear, string semester)
        {
            return await _context.FeeStructures
                .Where(fs => fs.IsActive && fs.AcademicYear == academicYear && fs.Semester == semester)
                .Include(fs => fs.FeeStructureItems)
                    .ThenInclude(fsi => fsi.FeeCategory)
                .OrderBy(fs => fs.Name)
                .ToListAsync();
        }

        public async Task<FeeStructure> CreateFeeStructureAsync(FeeStructure feeStructure)
        {
            _context.FeeStructures.Add(feeStructure);
            await _context.SaveChangesAsync();
            return feeStructure;
        }

        public async Task<FeeStructure> UpdateFeeStructureAsync(FeeStructure feeStructure)
        {
            feeStructure.UpdatedAt = DateTime.UtcNow;
            _context.FeeStructures.Update(feeStructure);
            await _context.SaveChangesAsync();
            return feeStructure;
        }

        public async Task<bool> DeleteFeeStructureAsync(int id)
        {
            var feeStructure = await _context.FeeStructures.FindAsync(id);
            if (feeStructure == null) return false;

            feeStructure.IsActive = false;
            feeStructure.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // Fee Structure Item operations
        public async Task<List<FeeStructureItem>> GetFeeStructureItemsByStructureIdAsync(int feeStructureId)
        {
            return await _context.FeeStructureItems
                .Include(fsi => fsi.FeeCategory)
                .Where(fsi => fsi.FeeStructureId == feeStructureId)
                .OrderBy(fsi => fsi.FeeCategory.Name)
                .ToListAsync();
        }

        public async Task<FeeStructureItem?> GetFeeStructureItemByIdAsync(int id)
        {
            return await _context.FeeStructureItems
                .Include(fsi => fsi.FeeCategory)
                .FirstOrDefaultAsync(fsi => fsi.Id == id);
        }

        public async Task<FeeStructureItem> CreateFeeStructureItemAsync(FeeStructureItem item)
        {
            _context.FeeStructureItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<FeeStructureItem> UpdateFeeStructureItemAsync(FeeStructureItem item)
        {
            item.UpdatedAt = DateTime.UtcNow;
            _context.FeeStructureItems.Update(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> DeleteFeeStructureItemAsync(int id)
        {
            var item = await _context.FeeStructureItems.FindAsync(id);
            if (item == null) return false;

            _context.FeeStructureItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        // Additional Fee operations
        public async Task<List<AdditionalFee>> GetAllAdditionalFeesAsync()
        {
            return await _context.AdditionalFees
                .Where(af => af.IsActive)
                .OrderByDescending(af => af.CreatedAt)
                .ToListAsync();
        }

        public async Task<AdditionalFee?> GetAdditionalFeeByIdAsync(int id)
        {
            return await _context.AdditionalFees
                .Include(af => af.StudentAdditionalFees)
                .FirstOrDefaultAsync(af => af.Id == id);
        }

        public async Task<AdditionalFee> CreateAdditionalFeeAsync(AdditionalFee additionalFee)
        {
            _context.AdditionalFees.Add(additionalFee);
            await _context.SaveChangesAsync();
            return additionalFee;
        }

        public async Task<AdditionalFee> UpdateAdditionalFeeAsync(AdditionalFee additionalFee)
        {
            additionalFee.UpdatedAt = DateTime.UtcNow;
            _context.AdditionalFees.Update(additionalFee);
            await _context.SaveChangesAsync();
            return additionalFee;
        }

        public async Task<bool> DeleteAdditionalFeeAsync(int id)
        {
            var additionalFee = await _context.AdditionalFees.FindAsync(id);
            if (additionalFee == null) return false;

            additionalFee.IsActive = false;
            additionalFee.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // Student Fee Assignment operations
        public async Task<List<StudentFeeAssignment>> GetStudentFeeAssignmentsAsync(string studentNumber)
        {
            return await _context.StudentFeeAssignments
                .Include(sfa => sfa.FeeStructure)
                .Where(sfa => sfa.StudentNumber == studentNumber)
                .OrderByDescending(sfa => sfa.AssignedAt)
                .ToListAsync();
        }

        public async Task<StudentFeeAssignment?> GetStudentFeeAssignmentByIdAsync(int id)
        {
            return await _context.StudentFeeAssignments
                .Include(sfa => sfa.FeeStructure)
                .Include(sfa => sfa.Student)
                .FirstOrDefaultAsync(sfa => sfa.Id == id);
        }

        public async Task<StudentFeeAssignment> CreateStudentFeeAssignmentAsync(StudentFeeAssignment assignment)
        {
            _context.StudentFeeAssignments.Add(assignment);
            await _context.SaveChangesAsync();
            return assignment;
        }

        public async Task<bool> DeleteStudentFeeAssignmentAsync(int id)
        {
            var assignment = await _context.StudentFeeAssignments.FindAsync(id);
            if (assignment == null) return false;

            _context.StudentFeeAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsStudentFeeAssignmentAsync(string studentNumber, int feeStructureId, string academicYear, string semester)
        {
            return await _context.StudentFeeAssignments
                .AnyAsync(sfa => sfa.StudentNumber == studentNumber && 
                                sfa.FeeStructureId == feeStructureId && 
                                sfa.AcademicYear == academicYear && 
                                sfa.Semester == semester);
        }

        public async Task<List<StudentFeeAssignment>> GetAllStudentFeeAssignmentsAsync()
        {
            return await _context.StudentFeeAssignments
                .Include(sfa => sfa.FeeStructure)
                .Include(sfa => sfa.Student)
                .OrderByDescending(sfa => sfa.AssignedAt)
                .ToListAsync();
        }

        // Student Fee Balance operations
        public async Task<List<StudentFeeBalance>> GetStudentFeeBalancesAsync(string studentNumber)
        {
            return await _context.StudentFeeBalances
                .Include(b => b.FeeStructureItem)
                .Where(b => b.StudentNumber == studentNumber)
                .ToListAsync();
        }

        public async Task<StudentFeeBalance?> GetStudentFeeBalanceByIdAsync(int id)
        {
            return await _context.StudentFeeBalances
                .Include(sfb => sfb.FeeStructureItem)
                    .ThenInclude(fsi => fsi.FeeCategory)
                .Include(sfb => sfb.Student)
                .FirstOrDefaultAsync(sfb => sfb.Id == id);
        }

        public async Task<StudentFeeBalance?> GetStudentFeeBalanceByStudentAndItemAsync(string studentNumber, int feeStructureItemId)
        {
            return await _context.StudentFeeBalances
                .Include(sfb => sfb.FeeStructureItem)
                .FirstOrDefaultAsync(sfb => sfb.StudentNumber == studentNumber && 
                                           sfb.FeeStructureItemId == feeStructureItemId && 
                                           sfb.IsActive);
        }

        public async Task<List<StudentFeeBalance>> GetStudentFeeBalancesByStatusAsync(FeeBalanceStatus status)
        {
            return await _context.StudentFeeBalances
                .Include(sfb => sfb.Student)
                .Include(sfb => sfb.FeeStructureItem)
                    .ThenInclude(fsi => fsi.FeeCategory)
                .Where(sfb => sfb.Status == status && sfb.IsActive)
                .OrderBy(sfb => sfb.DueDate)
                .ToListAsync();
        }

        public async Task<List<StudentFeeBalance>> GetAllStudentFeeBalancesAsync()
        {
            return await _context.StudentFeeBalances
                .Include(sfb => sfb.Student)
                .Include(sfb => sfb.FeeStructureItem)
                    .ThenInclude(fsi => fsi.FeeCategory)
                .Where(sfb => sfb.IsActive)
                .OrderBy(sfb => sfb.DueDate)
                .ToListAsync();
        }

        public async Task<StudentFeeBalance> CreateStudentFeeBalanceAsync(StudentFeeBalance balance)
        {
            _context.StudentFeeBalances.Add(balance);
            await _context.SaveChangesAsync();
            return balance;
        }

        public async Task<StudentFeeBalance> UpdateStudentFeeBalanceAsync(StudentFeeBalance balance)
        {
            balance.UpdatedAt = DateTime.UtcNow;
            _context.StudentFeeBalances.Update(balance);
            await _context.SaveChangesAsync();
            return balance;
        }

        public async Task<bool> DeleteStudentFeeBalanceAsync(int id)
        {
            var balance = await _context.StudentFeeBalances.FindAsync(id);
            if (balance == null) return false;

            balance.IsActive = false;
            balance.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // Student Additional Fee operations
        public async Task<List<StudentAdditionalFee>> GetStudentAdditionalFeesAsync(string studentNumber)
        {
            return await _context.StudentAdditionalFees
                .Include(saf => saf.AdditionalFee)
                .Where(saf => saf.StudentNumber == studentNumber)
                .OrderBy(saf => saf.DueDate)
                .ToListAsync();
        }

        public async Task<StudentAdditionalFee?> GetStudentAdditionalFeeByIdAsync(int id)
        {
            return await _context.StudentAdditionalFees
                .Include(saf => saf.AdditionalFee)
                .Include(saf => saf.Student)
                .FirstOrDefaultAsync(saf => saf.Id == id);
        }

        public async Task<StudentAdditionalFee?> GetStudentAdditionalFeeByStudentAndFeeAsync(string studentNumber, int additionalFeeId)
        {
            return await _context.StudentAdditionalFees
                .Include(saf => saf.AdditionalFee)
                .FirstOrDefaultAsync(saf => saf.StudentNumber == studentNumber && 
                                           saf.AdditionalFeeId == additionalFeeId);
        }

        public async Task<StudentAdditionalFee> CreateStudentAdditionalFeeAsync(StudentAdditionalFee studentAdditionalFee)
        {
            _context.StudentAdditionalFees.Add(studentAdditionalFee);
            await _context.SaveChangesAsync();
            return studentAdditionalFee;
        }

        public async Task<StudentAdditionalFee> UpdateStudentAdditionalFeeAsync(StudentAdditionalFee studentAdditionalFee)
        {
            studentAdditionalFee.UpdatedAt = DateTime.UtcNow;
            _context.StudentAdditionalFees.Update(studentAdditionalFee);
            await _context.SaveChangesAsync();
            return studentAdditionalFee;
        }

        public async Task<bool> DeleteStudentAdditionalFeeAsync(int id)
        {
            var studentAdditionalFee = await _context.StudentAdditionalFees.FindAsync(id);
            if (studentAdditionalFee == null) return false;

            _context.StudentAdditionalFees.Remove(studentAdditionalFee);
            await _context.SaveChangesAsync();
            return true;
        }

        // Utility operations
        public async Task<List<Student>> GetStudentsByProgramAsync(string program)
        {
            return await _context.Students
                .Where(s => s.Program == program)
                .OrderBy(s => s.FullName)
                .ToListAsync();
        }

        public async Task<List<Student>> GetStudentsByClassAsync(string className)
        {
            // Assuming class information is stored in a property or related table
            // This might need to be adjusted based on your actual data model
            return await _context.Students
                .Where(s => s.Program.Contains(className)) // Placeholder logic
                .OrderBy(s => s.FullName)
                .ToListAsync();
        }

        public async Task<List<Student>> GetStudentsByNumbersAsync(List<string> studentNumbers)
        {
            return await _context.Students
                .Where(s => studentNumbers.Contains(s.StudentNumber))
                .OrderBy(s => s.FullName)
                .ToListAsync();
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            return await _context.Students.ToListAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Migration operations
        public async Task<List<StudentBalance>> GetAllOldStudentBalancesAsync()
        {
            try
            {
                Console.WriteLine("Getting all old student balances from database...");
                var balances = await _context.StudentBalances
                    .Include(sb => sb.Student)
                    .Include(sb => sb.FeeSchedule)
                    .Where(sb => sb.IsActive)
                    .OrderBy(sb => sb.StudentNumber)
                    .ToListAsync();
                
                Console.WriteLine($"Retrieved {balances.Count} old student balances");
                return balances;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting old student balances: {ex.Message}");
                throw;
            }
        }

        public async Task<FeeStructureItem?> GetOrCreateFeeStructureItemForOldBalanceAsync(StudentBalance oldBalance)
        {
            try
            {
                Console.WriteLine($"Looking for existing fee structure for FeeScheduleId: {oldBalance.FeeScheduleId}");
                
                // First, try to find an existing fee structure that matches the old balance
                var existingFeeStructure = await _context.FeeStructures
                    .Include(fs => fs.FeeStructureItems)
                    .FirstOrDefaultAsync(fs => fs.Id == oldBalance.FeeScheduleId);

                if (existingFeeStructure != null && existingFeeStructure.FeeStructureItems.Any())
                {
                    Console.WriteLine($"Found existing fee structure {existingFeeStructure.Id} with {existingFeeStructure.FeeStructureItems.Count} items");
                    // Use the first fee structure item from the existing structure
                    return existingFeeStructure.FeeStructureItems.First();
                }

                Console.WriteLine("No existing fee structure found, creating new one...");
                
                // If no existing structure, create a new one based on the old balance
                var feeCategory = await _context.FeeCategories.FirstOrDefaultAsync();
                if (feeCategory == null)
                {
                    Console.WriteLine("No fee categories found, creating default category...");
                    feeCategory = await CreateDefaultFeeCategoryAsync();
                }

                Console.WriteLine($"Using fee category: {feeCategory.Name} (ID: {feeCategory.Id})");

                // Generate a unique name using a combination of FeeScheduleId and a timestamp
                var uniqueName = $"Migrated Structure - {oldBalance.FeeScheduleId} - {DateTime.UtcNow.Ticks}";
                
                // Check if a fee structure with this name already exists (should be very unlikely with timestamp)
                var existingStructureWithName = await _context.FeeStructures
                    .FirstOrDefaultAsync(fs => fs.Name == uniqueName &&
                                               fs.AcademicYear == "2025" &&
                                               fs.Semester == "Summer");

                if (existingStructureWithName != null)
                {
                    Console.WriteLine($"Fee structure with name '{uniqueName}' already exists, using existing one");
                    var existingItem = await _context.FeeStructureItems
                        .FirstOrDefaultAsync(fsi => fsi.FeeStructureId == existingStructureWithName.Id);
                    return existingItem;
                }

                var feeStructure = new FeeStructure
                {
                    Name = uniqueName,
                    Description = $"Migrated from old fee schedule {oldBalance.FeeScheduleId}",
                    AcademicYear = "2025",
                    Semester = "Summer",
                    IsActive = true
                };

                Console.WriteLine($"Creating fee structure: {feeStructure.Name}");
                await _context.FeeStructures.AddAsync(feeStructure);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Created fee structure with ID: {feeStructure.Id}");

                var feeStructureItem = new FeeStructureItem
                {
                    FeeStructureId = feeStructure.Id,
                    FeeCategoryId = feeCategory.Id,
                    Description = "Migrated tuition fee",
                    Amount = oldBalance.TotalAmount,
                    DueDate = oldBalance.DueDate,
                    IsRequired = true
                };

                Console.WriteLine($"Creating fee structure item with amount: ${feeStructureItem.Amount}");
                await _context.FeeStructureItems.AddAsync(feeStructureItem);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Created fee structure item with ID: {feeStructureItem.Id}");

                return feeStructureItem;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetOrCreateFeeStructureItemForOldBalanceAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task<FeeCategory> CreateDefaultFeeCategoryAsync()
        {
            var defaultCategory = new FeeCategory
            {
                Name = "Tuition",
                Description = "Default tuition fee category",
                IsActive = true
            };

            await _context.FeeCategories.AddAsync(defaultCategory);
            await _context.SaveChangesAsync();
            return defaultCategory;
        }

        public async Task<List<PaymentNotification>> GetAllPaymentsAsync()
        {
            return await _context.PaymentNotifications.ToListAsync();
        }
    }
} 