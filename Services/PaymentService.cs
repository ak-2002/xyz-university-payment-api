// Purpose: Handles payment processing logic and combines student verification with payment storage
using xyz_university_payment_api.Data;
using xyz_university_payment_api.Models;
using System.Collections.Generic;
using System.Linq;
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

        // Custom response model for clearer API feedback
        public class PaymentResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public bool StudentExists { get; set; }
            public bool StudentIsActive { get; set; }
        }

        // Processes payment and verifies student in one step
        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentNotification payment)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentNumber == payment.StudentNumber);

            if (student == null)
            {
                return new PaymentResponse
                {
                    Success = false,
                    Message = "Payment received but student not found.",
                    StudentExists = false,
                    StudentIsActive = false
                };
            }

            // Save the payment regardless of student activity to keep all records
            _context.PaymentNotifications.Add(payment);
            await _context.SaveChangesAsync();

            return new PaymentResponse
            {
                Success = true,
                Message = student.IsActive ? "Payment processed successfully. Student is currently enrolled." 
                                           : "Payment processed successfully. Student is not currently enrolled.",
                StudentExists = true,
                StudentIsActive = student.IsActive
            };
        }

        // Retrieves all payment records
        public List<PaymentNotification> GetAllPayments()
        {
            return _context.Payments.ToList();
        }
    }
}
