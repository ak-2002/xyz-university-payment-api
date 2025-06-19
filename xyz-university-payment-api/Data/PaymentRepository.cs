// Purpose: Payment repository implementation
// Provides concrete implementation of IPaymentRepository interface
using Microsoft.EntityFrameworkCore;
using xyz_university_payment_api.Interfaces;
using xyz_university_payment_api.Models;

namespace xyz_university_payment_api.Data
{
 
    public class PaymentRepository : Repository<PaymentNotification>, IPaymentRepository
    {
        public PaymentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PaymentNotification>> GetPaymentsByStudentAsync(string studentNumber)
        {
            return await _dbSet
                .Where(p => p.StudentNumber == studentNumber)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentNotification>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<PaymentNotification?> GetByPaymentReferenceAsync(string paymentReference)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.PaymentReference == paymentReference);
        }

        public async Task<bool> PaymentReferenceExistsAsync(string paymentReference)
        {
            return await _dbSet.AnyAsync(p => p.PaymentReference == paymentReference);
        }

        public async Task<decimal> GetTotalAmountPaidByStudentAsync(string studentNumber)
        {
            return await _dbSet
                .Where(p => p.StudentNumber == studentNumber)
                .SumAsync(p => p.AmountPaid);
        }

        public async Task<IEnumerable<PaymentNotification>> GetPaymentsAboveAmountAsync(decimal minimumAmount)
        {
            return await _dbSet
                .Where(p => p.AmountPaid >= minimumAmount)
                .OrderByDescending(p => p.AmountPaid)
                .ToListAsync();
        }

        public async Task<PaymentNotification?> GetLatestPaymentByStudentAsync(string studentNumber)
        {
            return await _dbSet
                .Where(p => p.StudentNumber == studentNumber)
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PaymentNotification>> GetPaymentsByDateReceivedAsync(DateTime dateReceived)
        {
            return await _dbSet
                .Where(p => p.DateReceived.Date == dateReceived.Date)
                .OrderByDescending(p => p.DateReceived)
                .ToListAsync();
        }
    }
} 