using xyz_university_payment_api.Models;

namespace xyz_university_payment_api.Interfaces
{
    //Interface for publishing messages to rabbitMQ

    public interface IMessagePublisher
    
    {
        Task PublishPaymentProcessedAsync(PaymentProcessedMessage message); //publishes a payment processed message
        
        Task PublishPaymentFailedAsync(PaymentFailedMessage message); // publishes a payment failed message

        Task PublishPaymentValidationAsync(PaymentValidationMessage message); // published a payment validation message

        Task PublishPaymentMessageAsync(PaymentMessage message); //publishes a generic payment message
    }


}