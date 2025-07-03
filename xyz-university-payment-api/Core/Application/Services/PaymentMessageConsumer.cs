using MassTransit;
using xyz_university_payment_api.Core.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace xyz_university_payment_api.Core.Application.Services
{
    /// <summary>
    /// Consumer for payment processed messages
    /// </summary>
    public class PaymentProcessedMessageConsumer : IConsumer<PaymentProcessedMessage>
    {
        private readonly ILogger<PaymentProcessedMessageConsumer> _logger;

        public PaymentProcessedMessageConsumer(ILogger<PaymentProcessedMessageConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentProcessedMessage> context)
        {
            var message = context.Message;
            _logger.LogInformation("Received payment processed message: {PaymentReference} for student {StudentNumber}",
                message.PaymentReference, message.StudentNumber);

            // Here you would typically:
            // 1. Send email confirmation to student
            // 2. Update student account balance
            // 3. Send notification to finance department
            // 4. Log to audit trail

            _logger.LogInformation("Payment processed successfully: Amount {Amount}, Student Active: {StudentIsActive}",
                message.Amount, message.StudentIsActive);

            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Consumer for payment failed messages
    /// </summary>
    public class PaymentFailedMessageConsumer : IConsumer<PaymentFailedMessage>
    {
        private readonly ILogger<PaymentFailedMessageConsumer> _logger;

        public PaymentFailedMessageConsumer(ILogger<PaymentFailedMessageConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentFailedMessage> context)
        {
            var message = context.Message;
            _logger.LogWarning("Received payment failed message: {PaymentReference} for student {StudentNumber}, Reason: {ErrorReason}",
                message.PaymentReference, message.StudentNumber, message.ErrorReason);

            // Here you would typically:
            // 1. Send failure notification to student
            // 2. Alert finance department
            // 3. Log to error tracking system
            // 4. Create support ticket if needed

            _logger.LogWarning("Payment failed: Amount {Amount}, Error: {ErrorReason}",
                message.Amount, message.ErrorReason);

            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Consumer for payment validation messages
    /// </summary>
    public class PaymentValidationMessageConsumer : IConsumer<PaymentValidationMessage>
    {
        private readonly ILogger<PaymentValidationMessageConsumer> _logger;

        public PaymentValidationMessageConsumer(ILogger<PaymentValidationMessageConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentValidationMessage> context)
        {
            var message = context.Message;
            _logger.LogInformation("Received payment validation message: {PaymentReference} for student {StudentNumber}",
                message.PaymentReference, message.StudentNumber);

            if (message.ValidationErrors.Any())
            {
                _logger.LogWarning("Payment validation failed with {ErrorCount} errors: {Errors}",
                    message.ValidationErrors.Count, string.Join(", ", message.ValidationErrors));
            }
            else
            {
                _logger.LogInformation("Payment validation passed for reference: {PaymentReference}", message.PaymentReference);
            }

            // Here you would typically:
            // 1. Log validation results
            // 2. Send validation report to relevant departments
            // 3. Update validation metrics

            await Task.CompletedTask;
        }
    }
}