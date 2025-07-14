using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using xyz_university_payment_api.Core.Application.Interfaces;
using xyz_university_payment_api.Core.Domain.Entities;
using xyz_university_payment_api.Infrastructure.Data;

namespace xyz_university_payment_api.Core.Application.Services
{
    public interface IDataSeedingService
    {
        Task SeedFeeSchedulesAsync();
        Task SeedStudentBalancesAsync();
        Task SeedAllDataAsync();
        Task<bool> HasSeedDataAsync();
    }

    public class DataSeedingService : IDataSeedingService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DataSeedingService> _logger;
        private readonly IStudentBalanceService _studentBalanceService;

        public DataSeedingService(
            AppDbContext context,
            ILogger<DataSeedingService> logger,
            IStudentBalanceService studentBalanceService)
        {
            _context = context;
            _logger = logger;
            _studentBalanceService = studentBalanceService;
        }

        public async Task<bool> HasSeedDataAsync()
        {
            return await _context.FeeSchedules.AnyAsync() && 
                   await _context.StudentBalances.AnyAsync();
        }

        public async Task SeedAllDataAsync()
        {
            _logger.LogInformation("Starting comprehensive data seeding...");

            await SeedFeeSchedulesAsync();
            await SeedStudentBalancesAsync();

            _logger.LogInformation("Data seeding completed successfully");
        }

        public async Task SeedFeeSchedulesAsync()
        {
            if (await _context.FeeSchedules.AnyAsync())
            {
                _logger.LogInformation("Fee schedules already exist, skipping seeding");
                return;
            }

            _logger.LogInformation("Seeding fee schedules...");

            var feeSchedules = new List<FeeSchedule>
            {
                // Computer Science Programs
                new FeeSchedule
                {
                    Semester = "Summer",
                    AcademicYear = "2025",
                    Program = "Computer Science",
                    TuitionFee = 4500.00m,
                    RegistrationFee = 500.00m,
                    LibraryFee = 200.00m,
                    LaboratoryFee = 300.00m,
                    OtherFees = 100.00m,
                    TotalAmount = 5600.00m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new FeeSchedule
                {
                    Semester = "Summer",
                    AcademicYear = "2025",
                    Program = "Information Technology",
                    TuitionFee = 4200.00m,
                    RegistrationFee = 500.00m,
                    LibraryFee = 200.00m,
                    LaboratoryFee = 250.00m,
                    OtherFees = 100.00m,
                    TotalAmount = 5250.00m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                // Business Programs
                new FeeSchedule
                {
                    Semester = "Summer",
                    AcademicYear = "2025",
                    Program = "Business Administration",
                    TuitionFee = 4000.00m,
                    RegistrationFee = 500.00m,
                    LibraryFee = 200.00m,
                    LaboratoryFee = 0.00m,
                    OtherFees = 100.00m,
                    TotalAmount = 4800.00m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new FeeSchedule
                {
                    Semester = "Summer",
                    AcademicYear = "2025",
                    Program = "Accounting",
                    TuitionFee = 4200.00m,
                    RegistrationFee = 500.00m,
                    LibraryFee = 200.00m,
                    LaboratoryFee = 150.00m,
                    OtherFees = 100.00m,
                    TotalAmount = 5150.00m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                // Engineering Programs
                new FeeSchedule
                {
                    Semester = "Summer",
                    AcademicYear = "2025",
                    Program = "Engineering",
                    TuitionFee = 5000.00m,
                    RegistrationFee = 500.00m,
                    LibraryFee = 200.00m,
                    LaboratoryFee = 400.00m,
                    OtherFees = 150.00m,
                    TotalAmount = 6250.00m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                // Social Sciences
                new FeeSchedule
                {
                    Semester = "Summer",
                    AcademicYear = "2025",
                    Program = "Sociology",
                    TuitionFee = 3800.00m,
                    RegistrationFee = 500.00m,
                    LibraryFee = 200.00m,
                    LaboratoryFee = 0.00m,
                    OtherFees = 100.00m,
                    TotalAmount = 4600.00m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.FeeSchedules.AddRangeAsync(feeSchedules);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} fee schedules", feeSchedules.Count);
        }

        public async Task SeedStudentBalancesAsync()
        {
            // Check if we need to update existing records or create new ones
            var existingBalances = await _context.StudentBalances.ToListAsync();
            var paymentNotifications = await _context.PaymentNotifications.ToListAsync();

            if (existingBalances.Any())
            {
                _logger.LogInformation("Updating existing student balances with actual payment data...");
                
                // Update existing records with actual payment data
                foreach (var balance in existingBalances)
                {
                    var studentPayments = paymentNotifications
                        .Where(p => p.StudentNumber == balance.StudentNumber)
                        .ToList();
                    
                    var actualAmountPaid = studentPayments.Sum(p => p.AmountPaid);
                    var outstandingBalance = Math.Max(0, balance.TotalAmount - actualAmountPaid);
                    var status = DetermineStatus(outstandingBalance, balance.TotalAmount);

                    balance.AmountPaid = actualAmountPaid;
                    balance.OutstandingBalance = outstandingBalance;
                    balance.Status = status;
                    balance.UpdatedAt = DateTime.UtcNow;
                    balance.Notes = GenerateBalanceNotes(status, actualAmountPaid, balance.TotalAmount);

                    _logger.LogInformation("Updated student {StudentNumber}: Total=${Total}, Actual Paid=${Paid}, Outstanding=${Outstanding}", 
                        balance.StudentNumber, balance.TotalAmount, actualAmountPaid, outstandingBalance);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated {Count} existing student balances", existingBalances.Count);
                
                // Clear cache to ensure fresh data
                await _studentBalanceService.ClearCacheAsync();
                return;
            }

            _logger.LogInformation("Seeding student balances...");

            // Get existing students and fee schedules
            var students = await _context.Students.ToListAsync();
            var feeSchedules = await _context.FeeSchedules.ToListAsync();

            if (!students.Any())
            {
                _logger.LogWarning("No students found. Please seed students first.");
                return;
            }

            if (!feeSchedules.Any())
            {
                _logger.LogWarning("No fee schedules found. Please seed fee schedules first.");
                return;
            }

            var studentBalances = new List<StudentBalance>();

            foreach (var student in students)
            {
                // Find matching fee schedule for student's program
                var feeSchedule = feeSchedules.FirstOrDefault(fs => 
                    fs.Program.Equals(student.Program, StringComparison.OrdinalIgnoreCase)) 
                    ?? feeSchedules.First(); // Fallback to first schedule

                // Get actual payments for this student from PaymentNotification table
                var studentPayments = paymentNotifications
                    .Where(p => p.StudentNumber == student.StudentNumber)
                    .ToList();
                
                var actualAmountPaid = studentPayments.Sum(p => p.AmountPaid);
                var totalAmount = feeSchedule.TotalAmount;
                var outstandingBalance = Math.Max(0, totalAmount - actualAmountPaid);
                var status = DetermineStatus(outstandingBalance, totalAmount);

                _logger.LogInformation("Student {StudentNumber}: Total Amount=${TotalAmount}, Actual Paid=${ActualPaid}, Outstanding=${Outstanding}", 
                    student.StudentNumber, totalAmount, actualAmountPaid, outstandingBalance);

                var studentBalance = new StudentBalance
                {
                    StudentNumber = student.StudentNumber,
                    FeeScheduleId = feeSchedule.Id,
                    TotalAmount = totalAmount,
                    AmountPaid = actualAmountPaid, // Use actual payment data
                    OutstandingBalance = outstandingBalance,
                    DueDate = DateTime.UtcNow.AddDays(30), // Due in 30 days
                    Status = status,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Notes = GenerateBalanceNotes(status, actualAmountPaid, totalAmount)
                };

                studentBalances.Add(studentBalance);
            }

            await _context.StudentBalances.AddRangeAsync(studentBalances);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} student balances using actual payment data", studentBalances.Count);

            // Clear cache to ensure fresh data
            await _studentBalanceService.ClearCacheAsync();
        }



        private string DetermineStatus(decimal outstandingBalance, decimal totalAmount)
        {
            if (outstandingBalance <= 0)
                return "Paid";
            else if (outstandingBalance == totalAmount)
                return "Outstanding";
            else if (outstandingBalance > totalAmount * 0.5m)
                return "Overdue";
            else
                return "Partial";
        }

        private string GenerateBalanceNotes(string status, decimal amountPaid, decimal totalAmount)
        {
            return status switch
            {
                "Paid" => $"Full payment received. Amount paid: ${amountPaid:F2}",
                "Outstanding" => $"No payment received. Total due: ${totalAmount:F2}",
                "Overdue" => $"Payment overdue. Amount paid: ${amountPaid:F2}, Outstanding: ${(totalAmount - amountPaid):F2}",
                "Partial" => $"Partial payment received. Amount paid: ${amountPaid:F2}, Outstanding: ${(totalAmount - amountPaid):F2}",
                _ => "Balance created during system initialization"
            };
        }
    }
} 