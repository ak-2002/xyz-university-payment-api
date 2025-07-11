using MassTransit;
using xyz_university_payment_api.Core.Domain.Entities;
using xyz_university_payment_api.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace xyz_university_payment_api.Core.Application.Services
{
    public class PaymentMessageConsumer : IConsumer<PaymentProcessedMessage>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<PaymentMessageConsumer> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentMessageConsumer(IEmailService emailService, ILogger<PaymentMessageConsumer> logger, IUnitOfWork unitOfWork)
        {
            _emailService = emailService;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task Consume(ConsumeContext<PaymentProcessedMessage> context)
        {
            var message = context.Message;
            _logger.LogInformation("Processing payment message: {PaymentReference} for student: {StudentNumber}", 
                message.PaymentReference, message.StudentNumber);

            try
            {
                // Get student information
                var students = await _unitOfWork.Students.FindAsync(s => s.StudentNumber == message.StudentNumber);
                var student = students.FirstOrDefault();
                if (student == null)
                {
                    _logger.LogWarning("Student not found for payment message: {StudentNumber}", message.StudentNumber);
                    return;
                }

                var studentEmail = !string.IsNullOrEmpty(student.Email) ? student.Email : $"{student.StudentNumber}@student.xyzuniversity.edu";

                // Send payment receipt email
                var emailSent = await _emailService.SendPaymentReceiptAsync(
                    new PaymentNotification
                    {
                        PaymentReference = message.PaymentReference,
                        StudentNumber = message.StudentNumber,
                        AmountPaid = message.Amount,
                        PaymentDate = message.PaymentDate,
                        PaymentMethod = "M-Pesa" // Default, will be updated from database
                    },
                    studentEmail,
                    student.FullName
                );

                if (emailSent)
                {
                    _logger.LogInformation("Payment receipt email sent successfully for payment: {PaymentReference}", message.PaymentReference);
                }
                else
                {
                    _logger.LogWarning("Failed to send payment receipt email for payment: {PaymentReference}", message.PaymentReference);
                }

                // Send payment confirmation email
                var confirmationSent = await _emailService.SendPaymentConfirmationAsync(
                    new PaymentNotification
                    {
                        PaymentReference = message.PaymentReference,
                        StudentNumber = message.StudentNumber,
                        AmountPaid = message.Amount,
                        PaymentDate = message.PaymentDate,
                        PaymentMethod = "M-Pesa"
                    },
                    studentEmail,
                    student.FullName
                );

                if (confirmationSent)
                {
                    _logger.LogInformation("Payment confirmation email sent successfully for payment: {PaymentReference}", message.PaymentReference);
                }
                else
                {
                    _logger.LogWarning("Failed to send payment confirmation email for payment: {PaymentReference}", message.PaymentReference);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment message: {PaymentReference}", message.PaymentReference);
                throw;
            }
        }
    }

    public class PaymentFailedMessageConsumer : IConsumer<PaymentFailedMessage>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<PaymentFailedMessageConsumer> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentFailedMessageConsumer(IEmailService emailService, ILogger<PaymentFailedMessageConsumer> logger, IUnitOfWork unitOfWork)
        {
            _emailService = emailService;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task Consume(ConsumeContext<PaymentFailedMessage> context)
        {
            var message = context.Message;
            _logger.LogInformation("Processing payment failure message: {PaymentReference} for student: {StudentNumber}", 
                message.PaymentReference, message.StudentNumber);

            try
            {
                // Get student information
                var students = await _unitOfWork.Students.FindAsync(s => s.StudentNumber == message.StudentNumber);
                var student = students.FirstOrDefault();
                if (student == null)
                {
                    _logger.LogWarning("Student not found for payment failure message: {StudentNumber}", message.StudentNumber);
                    return;
                }

                var studentEmail = !string.IsNullOrEmpty(student.Email) ? student.Email : $"{student.StudentNumber}@student.xyzuniversity.edu";

                // Send payment failure notification
                var emailSent = await _emailService.SendPaymentFailureNotificationAsync(
                    studentEmail,
                    student.FullName,
                    message.ErrorReason
                );

                if (emailSent)
                {
                    _logger.LogInformation("Payment failure notification sent successfully for payment: {PaymentReference}", message.PaymentReference);
                }
                else
                {
                    _logger.LogWarning("Failed to send payment failure notification for payment: {PaymentReference}", message.PaymentReference);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment failure message: {PaymentReference}", message.PaymentReference);
                throw;
            }
        }
    }
}