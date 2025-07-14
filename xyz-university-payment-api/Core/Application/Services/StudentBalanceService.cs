using xyz_university_payment_api.Core.Application.Interfaces;
using xyz_university_payment_api.Core.Domain.Entities;
using xyz_university_payment_api.Core.Domain.Exceptions;
using xyz_university_payment_api.Core.Application.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace xyz_university_payment_api.Core.Application.Services
{
    public class StudentBalanceService : IStudentBalanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StudentBalanceService> _logger;
        private readonly ICacheService _cacheService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StudentBalanceService(
            IUnitOfWork unitOfWork, 
            ILogger<StudentBalanceService> logger, 
            ICacheService cacheService, 
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cacheService = cacheService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<StudentBalanceSummaryDto> GetStudentBalanceSummaryAsync(string studentNumber)
        {
            try
            {
                _logger.LogInformation("Getting balance summary for student: {StudentNumber}", studentNumber);

                // Try to get from cache first
                var cacheKey = $"student_balance_summary_{studentNumber}";
                var cachedSummary = await _cacheService.GetAsync<StudentBalanceSummaryDto>(cacheKey);
                if (cachedSummary != null)
                {
                    _logger.LogInformation("Student balance summary retrieved from cache: {StudentNumber}", studentNumber);
                    return cachedSummary;
                }

                // Get student information
                var student = await _unitOfWork.Students.FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);
                if (student == null)
                {
                    throw new StudentNotFoundException(studentNumber);
                }

                // Get all payments for the student from existing PaymentNotification table
                var payments = await _unitOfWork.Payments.FindAsync(p => p.StudentNumber == studentNumber);
                var totalPaid = payments.Sum(p => p.AmountPaid);

                // Get all student balances from new StudentBalance table
                var studentBalances = await _unitOfWork.StudentBalances.FindAsync(sb => sb.StudentNumber == studentNumber && sb.IsActive);
                
                decimal totalOutstandingBalance;
                decimal currentSemesterBalance;
                DateTime nextPaymentDue;
                string paymentStatus;
                string currentSemester = "Summer 2025";

                if (studentBalances.Any())
                {
                    // Use new balance system if available
                    totalOutstandingBalance = studentBalances.Sum(sb => sb.OutstandingBalance);
                    currentSemesterBalance = studentBalances
                        .Where(sb => sb.FeeSchedule?.Semester == currentSemester)
                        .FirstOrDefault()?.OutstandingBalance ?? 0;
                    nextPaymentDue = studentBalances
                        .Where(sb => sb.OutstandingBalance > 0)
                        .OrderBy(sb => sb.DueDate)
                        .FirstOrDefault()?.DueDate ?? DateTime.UtcNow.AddDays(30);
                    
                    _logger.LogInformation("Using new balance system for student {StudentNumber}: Outstanding=${Outstanding}", 
                        studentNumber, totalOutstandingBalance);
                }
                else
                {
                    // Fallback to payment notification data with estimated balance
                    // Estimate total tuition based on program
                    var estimatedTuition = student.Program switch
                    {
                        "Computer Science" => 5600.00m,
                        "Information Technology" => 5250.00m,
                        "Business Administration" => 4800.00m,
                        "Accounting" => 5150.00m,
                        "Engineering" => 6250.00m,
                        "Sociology" => 4600.00m,
                        "CS" => 5600.00m, // Handle abbreviated program names
                        "IT" => 5250.00m,
                        "ACC" => 5150.00m,
                        _ => 5000.00m // Default fallback
                    };
                    
                    totalOutstandingBalance = Math.Max(0, estimatedTuition - totalPaid);
                    currentSemesterBalance = totalOutstandingBalance;
                    nextPaymentDue = DateTime.UtcNow.AddDays(30);
                    
                    _logger.LogInformation("Using payment notification fallback for student {StudentNumber}: Tuition=${Tuition}, Paid=${Paid}, Outstanding=${Outstanding}", 
                        studentNumber, estimatedTuition, totalPaid, totalOutstandingBalance);
                }

                // Determine payment status
                paymentStatus = totalOutstandingBalance <= 0 ? "Paid" : 
                              totalPaid == 0 ? "Outstanding" : "Partial";

                var summary = new StudentBalanceSummaryDto
                {
                    StudentNumber = studentNumber,
                    StudentName = student.FullName,
                    Program = student.Program,
                    CurrentSemester = currentSemester,
                    TotalOutstandingBalance = totalOutstandingBalance,
                    CurrentSemesterBalance = currentSemesterBalance,
                    TotalPaid = totalPaid,
                    NextPaymentDue = nextPaymentDue,
                    PaymentStatus = paymentStatus,
                    SemesterBalances = studentBalances.Select(sb => new StudentBalanceDto
                    {
                        Id = sb.Id,
                        StudentNumber = sb.StudentNumber,
                        FeeScheduleId = sb.FeeScheduleId,
                        TotalAmount = sb.TotalAmount,
                        AmountPaid = sb.AmountPaid,
                        OutstandingBalance = sb.OutstandingBalance,
                        DueDate = sb.DueDate,
                        Status = sb.Status,
                        IsActive = sb.IsActive,
                        CreatedAt = sb.CreatedAt,
                        UpdatedAt = sb.UpdatedAt,
                        Notes = sb.Notes,
                        StudentName = student.FullName,
                        Program = student.Program,
                        Semester = sb.FeeSchedule?.Semester ?? "Unknown",
                        AcademicYear = sb.FeeSchedule?.AcademicYear ?? "Unknown"
                    }).ToList()
                };

                // Cache the summary for 15 minutes
                await _cacheService.SetAsync(cacheKey, summary, TimeSpan.FromMinutes(15));
                _logger.LogInformation("Student balance summary cached: {StudentNumber}", studentNumber);

                return summary;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student balance summary for student: {StudentNumber}", studentNumber);
                throw new DatabaseException($"Failed to get balance summary for student {studentNumber}", ex);
            }
        }

        public async Task<decimal> GetOutstandingBalanceAsync(string studentNumber)
        {
            try
            {
                _logger.LogInformation("Getting outstanding balance for student: {StudentNumber}", studentNumber);

                // Try to get from cache first
                var cacheKey = $"student_outstanding_balance_{studentNumber}";
                var cachedBalance = await _cacheService.GetAsync<decimal>(cacheKey);
                if (cachedBalance != 0)
                {
                    _logger.LogInformation("Student outstanding balance retrieved from cache: {StudentNumber}", studentNumber);
                    return cachedBalance;
                }

                // Get all active student balances
                var studentBalances = await _unitOfWork.StudentBalances.FindAsync(sb => sb.StudentNumber == studentNumber && sb.IsActive);
                
                decimal outstandingBalance;
                
                if (studentBalances.Any())
                {
                    // Use new balance system if available
                    outstandingBalance = studentBalances.Sum(sb => sb.OutstandingBalance);
                }
                else
                {
                    // Fallback to payment notification data
                    var student = await _unitOfWork.Students.FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);
                    if (student == null)
                    {
                        throw new StudentNotFoundException(studentNumber);
                    }
                    
                    var payments = await _unitOfWork.Payments.FindAsync(p => p.StudentNumber == studentNumber);
                    var totalPaid = payments.Sum(p => p.AmountPaid);
                    
                    // Estimate total tuition based on program
                    var estimatedTuition = student.Program switch
                    {
                        "Computer Science" => 5600.00m,
                        "Information Technology" => 5250.00m,
                        "Business Administration" => 4800.00m,
                        "Accounting" => 5150.00m,
                        "Engineering" => 6250.00m,
                        "Sociology" => 4600.00m,
                        "CS" => 5600.00m,
                        "IT" => 5250.00m,
                        "ACC" => 5150.00m,
                        _ => 5000.00m
                    };
                    
                    outstandingBalance = Math.Max(0, estimatedTuition - totalPaid);
                }

                // Cache the balance for 10 minutes
                await _cacheService.SetAsync(cacheKey, outstandingBalance, TimeSpan.FromMinutes(10));
                _logger.LogInformation("Student outstanding balance cached: {StudentNumber}", studentNumber);

                return outstandingBalance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting outstanding balance for student: {StudentNumber}", studentNumber);
                throw new DatabaseException($"Failed to get outstanding balance for student {studentNumber}", ex);
            }
        }

        public async Task<decimal> GetTotalPaidAsync(string studentNumber)
        {
            try
            {
                _logger.LogInformation("Getting total paid amount for student: {StudentNumber}", studentNumber);

                // Try to get from cache first
                var cacheKey = $"student_total_paid_{studentNumber}";
                var cachedAmount = await _cacheService.GetAsync<decimal>(cacheKey);
                if (cachedAmount != 0)
                {
                    _logger.LogInformation("Student total paid amount retrieved from cache: {StudentNumber}", studentNumber);
                    return cachedAmount;
                }

                // Get all payments for the student
                var payments = await _unitOfWork.Payments.FindAsync(p => p.StudentNumber == studentNumber);
                var totalPaid = payments.Sum(p => p.AmountPaid);

                // Cache the amount for 10 minutes
                await _cacheService.SetAsync(cacheKey, totalPaid, TimeSpan.FromMinutes(10));
                _logger.LogInformation("Student total paid amount cached: {StudentNumber}", studentNumber);

                return totalPaid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total paid amount for student: {StudentNumber}", studentNumber);
                throw new DatabaseException($"Failed to get total paid amount for student {studentNumber}", ex);
            }
        }

        public async Task<DateTime> GetNextPaymentDueAsync(string studentNumber)
        {
            try
            {
                _logger.LogInformation("Getting next payment due date for student: {StudentNumber}", studentNumber);

                // Get all active student balances with outstanding amounts
                var studentBalances = await _unitOfWork.StudentBalances.FindAsync(sb => 
                    sb.StudentNumber == studentNumber && 
                    sb.IsActive && 
                    sb.OutstandingBalance > 0);

                if (!studentBalances.Any())
                {
                    // If no outstanding balances, return a default date
                    return DateTime.UtcNow.AddDays(30);
                }

                // Return the earliest due date
                return studentBalances.Min(sb => sb.DueDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next payment due date for student: {StudentNumber}", studentNumber);
                throw new DatabaseException($"Failed to get next payment due date for student {studentNumber}", ex);
            }
        }

        public async Task UpdateStudentBalanceAsync(string studentNumber, decimal paymentAmount)
        {
            try
            {
                _logger.LogInformation("Updating student balance for student: {StudentNumber} with payment: {Amount}", studentNumber, paymentAmount);

                // Get all active student balances
                var studentBalances = await _unitOfWork.StudentBalances.FindAsync(sb => 
                    sb.StudentNumber == studentNumber && 
                    sb.IsActive && 
                    sb.OutstandingBalance > 0);

                if (!studentBalances.Any())
                {
                    _logger.LogWarning("No active balances found for student: {StudentNumber}", studentNumber);
                    return;
                }

                var remainingPayment = paymentAmount;

                // Apply payment to balances in order of due date (earliest first)
                foreach (var balance in studentBalances.OrderBy(sb => sb.DueDate))
                {
                    if (remainingPayment <= 0) break;

                    var amountToApply = Math.Min(remainingPayment, balance.OutstandingBalance);
                    
                    balance.AmountPaid += amountToApply;
                    balance.OutstandingBalance -= amountToApply;
                    balance.UpdatedAt = DateTime.UtcNow;

                    // Update status based on outstanding balance
                    if (balance.OutstandingBalance <= 0)
                    {
                        balance.Status = "Paid";
                    }
                    else if (balance.OutstandingBalance < balance.TotalAmount)
                    {
                        balance.Status = "Partial";
                    }

                    remainingPayment -= amountToApply;
                }

                await _unitOfWork.SaveChangesAsync();

                // Invalidate related caches
                await InvalidateStudentBalanceCachesAsync(studentNumber);

                _logger.LogInformation("Student balance updated successfully for student: {StudentNumber}", studentNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student balance for student: {StudentNumber}", studentNumber);
                throw new DatabaseException($"Failed to update student balance for student {studentNumber}", ex);
            }
        }

        public async Task<OutstandingBalanceReportDto> GetOutstandingBalanceReportAsync()
        {
            try
            {
                _logger.LogInformation("Generating outstanding balance report");

                // Try to get from cache first
                var cacheKey = "outstanding_balance_report";
                var cachedReport = await _cacheService.GetAsync<OutstandingBalanceReportDto>(cacheKey);
                if (cachedReport != null)
                {
                    _logger.LogInformation("Outstanding balance report retrieved from cache");
                    return cachedReport;
                }

                // Get all students
                var students = await _unitOfWork.Students.GetAllAsync();
                var totalStudents = students.Count();

                // Get all active student balances
                var studentBalances = await _unitOfWork.StudentBalances.FindAsync(sb => sb.IsActive);
                
                // Calculate statistics
                var studentsWithBalance = studentBalances.Select(sb => sb.StudentNumber).Distinct().Count();
                var totalOutstandingAmount = studentBalances.Sum(sb => sb.OutstandingBalance);
                var averageOutstandingAmount = studentsWithBalance > 0 ? totalOutstandingAmount / studentsWithBalance : 0;

                // Get top outstanding balances
                var topOutstandingBalances = studentBalances
                    .OrderByDescending(sb => sb.OutstandingBalance)
                    .Take(10)
                    .Select(sb => new StudentBalanceSummaryDto
                    {
                        StudentNumber = sb.StudentNumber,
                        StudentName = students.FirstOrDefault(s => s.StudentNumber == sb.StudentNumber)?.FullName ?? "Unknown",
                        Program = students.FirstOrDefault(s => s.StudentNumber == sb.StudentNumber)?.Program ?? "Unknown",
                        CurrentSemester = sb.FeeSchedule?.Semester ?? "Unknown",
                        TotalOutstandingBalance = sb.OutstandingBalance,
                        CurrentSemesterBalance = sb.OutstandingBalance,
                        TotalPaid = sb.AmountPaid,
                        NextPaymentDue = sb.DueDate,
                        PaymentStatus = sb.Status
                    })
                    .ToList();

                // Calculate outstanding by program
                var outstandingByProgram = studentBalances
                    .GroupBy(sb => students.FirstOrDefault(s => s.StudentNumber == sb.StudentNumber)?.Program ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Sum(sb => sb.OutstandingBalance));

                // Calculate outstanding by semester
                var outstandingBySemester = studentBalances
                    .GroupBy(sb => sb.FeeSchedule?.Semester ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Sum(sb => sb.OutstandingBalance));

                var report = new OutstandingBalanceReportDto
                {
                    TotalStudents = totalStudents,
                    StudentsWithBalance = studentsWithBalance,
                    TotalOutstandingAmount = totalOutstandingAmount,
                    AverageOutstandingAmount = averageOutstandingAmount,
                    TopOutstandingBalances = topOutstandingBalances,
                    OutstandingByProgram = outstandingByProgram,
                    OutstandingBySemester = outstandingBySemester
                };

                // Cache the report for 30 minutes
                await _cacheService.SetAsync(cacheKey, report, TimeSpan.FromMinutes(30));
                _logger.LogInformation("Outstanding balance report cached");

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating outstanding balance report");
                throw new DatabaseException("Failed to generate outstanding balance report", ex);
            }
        }

        public async Task ClearCacheAsync()
        {
            try
            {
                _logger.LogInformation("Clearing all student balance caches");

                // Get all students to clear their individual caches
                var students = await _unitOfWork.Students.GetAllAsync();
                
                foreach (var student in students)
                {
                    await InvalidateStudentBalanceCachesAsync(student.StudentNumber);
                }

                // Clear report cache
                await _cacheService.RemoveAsync("outstanding_balance_report");

                _logger.LogInformation("All student balance caches cleared successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing student balance caches");
                throw new DatabaseException("Failed to clear student balance caches", ex);
            }
        }

        private async Task InvalidateStudentBalanceCachesAsync(string studentNumber)
        {
            try
            {
                var cacheKeys = new[]
                {
                    $"student_balance_summary_{studentNumber}",
                    $"student_outstanding_balance_{studentNumber}",
                    $"student_total_paid_{studentNumber}"
                };

                foreach (var key in cacheKeys)
                {
                    await _cacheService.RemoveAsync(key);
                }

                _logger.LogInformation("Student balance caches invalidated for student: {StudentNumber}", studentNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating student balance caches for student: {StudentNumber}", studentNumber);
            }
        }
    }
} 