using MassTransit;
using xyz_university_payment_api.Core.Application.Interfaces;
using xyz_university_payment_api.Core.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace xyz_university_payment_api.Core.Application.Services
{

    // Implementation of message publisher using RabbitMQ and MassTransit

    public class RabbitMQMessagePublisher : IMessagePublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<RabbitMQMessagePublisher> _logger;

        public RabbitMQMessagePublisher(IPublishEndpoint publishEndpoint, ILogger<RabbitMQMessagePublisher> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task PublishPaymentProcessedAsync(PaymentProcessedMessage message)
        {
            try
            {
                _logger.LogInformation("Publishing payment processed message: {PaymentReference}", message.PaymentReference);
                await _publishEndpoint.Publish(message);
                _logger.LogInformation("Successfully published payment processed message: {PaymentReference}", message.PaymentReference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish payment processed message: {PaymentReference}", message.PaymentReference);
                throw;
            }
        }

        public async Task PublishPaymentFailedAsync(PaymentFailedMessage message)
        {
            try
            {
                _logger.LogWarning("Publishing payment failed message: {PaymentReference}, Reason: {ErrorReason}",
                    message.PaymentReference, message.ErrorReason);
                await _publishEndpoint.Publish(message);
                _logger.LogInformation("Successfully published payment failed message: {PaymentReference}", message.PaymentReference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish payment failed message: {PaymentReference}", message.PaymentReference);
                throw;
            }
        }

        public async Task PublishPaymentValidationAsync(PaymentValidationMessage message)
        {
            try
            {
                _logger.LogInformation("Publishing payment validation message: {PaymentReference}", message.PaymentReference);
                await _publishEndpoint.Publish(message);
                _logger.LogInformation("Successfully published payment validation message: {PaymentReference}", message.PaymentReference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish payment validation message: {PaymentReference}", message.PaymentReference);
                throw;
            }
        }

        public async Task PublishPaymentMessageAsync(PaymentMessage message)
        {
            try
            {
                _logger.LogInformation("Publishing generic payment message: {PaymentReference}", message.PaymentReference);
                await _publishEndpoint.Publish(message);
                _logger.LogInformation("Successfully published generic payment message: {PaymentReference}", message.PaymentReference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish generic payment message: {PaymentReference}", message.PaymentReference);
                throw;
            }
        }
    }
}