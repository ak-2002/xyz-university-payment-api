// Purpose: Handles payment processing logic
using xyz_university_payment_api.Data;
using xyz_university_payment_api.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace xyz_university_payment_api.Services
{
    public class PaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context)
        {
            _context = context;
        }

        // Processes payment by verifying the student and saving the payment record
        public async Task<bool> ProcessPaymentAsync(PaymentNotification payment)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentNumber == payment.StudentNumber && s.IsActive);
            if (student == null)
                return false; // Student not found or not active

            _context.PaymentNotifications.Add(payment); // Save payment to database
            await _context.SaveChangesAsync();
            return true;
        }
        public List<PaymentNotification> GetAllPayments()
{
    return _context.Payments.ToList();
}

    }
}