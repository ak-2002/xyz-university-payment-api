using xyz_university_payment_api.Core.Domain.Entities;

namespace xyz_university_payment_api.Core.Application.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendPaymentReceiptAsync(PaymentNotification payment, string studentEmail, string studentName);
        Task<bool> SendPaymentConfirmationAsync(PaymentNotification payment, string studentEmail, string studentName);
        Task<bool> SendPaymentFailureNotificationAsync(string studentEmail, string studentName, string errorMessage);
        Task<bool> SendWelcomeEmailAsync(string studentEmail, string studentName, string studentNumber);
    }
} 