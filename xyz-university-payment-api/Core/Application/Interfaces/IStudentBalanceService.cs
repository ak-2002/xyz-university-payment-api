using xyz_university_payment_api.Core.Application.DTOs;

namespace xyz_university_payment_api.Core.Application.Interfaces
{
    public interface IStudentBalanceService
    {
        Task<StudentBalanceSummaryDto> GetStudentBalanceSummaryAsync(string studentNumber);
        Task<decimal> GetOutstandingBalanceAsync(string studentNumber);
        Task<decimal> GetTotalPaidAsync(string studentNumber);
        Task<DateTime> GetNextPaymentDueAsync(string studentNumber);
        Task UpdateStudentBalanceAsync(string studentNumber, decimal paymentAmount);
        Task<OutstandingBalanceReportDto> GetOutstandingBalanceReportAsync();
        Task ClearCacheAsync();
    }
} 