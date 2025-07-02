namespace xyz_university_payment_api.Core.Domain.Entities
{
    //messag emodel for payment events sent throguh rabbit mq
    public class PaymentMessage
    {
        public string PaymentReference{get; set;} =string.Empty;
        public string StudentNumber {get; set;} = string.Empty;
        public decimal Amount {get; set;}
        public DateTime PaymentDate{get; set;}
        public string Status {get; set;} = string.Empty;
        public string Message {get; set;} = string.Empty;
        public DateTime Timestamp {get; set;} = DateTime.UtcNow;
        public string MessageType { get; set; } = string.Empty;
    }


    // Message for successful payment processing

    public class PaymentProcessedMessage : PaymentMessage
    {
        public bool StudentExists {get; set;}
        public bool StudentIsActive { get; set;}
    }

    //Message for failed payment processing

    public class PaymentFailedMessage : PaymentMessage
    {
        public string ErrorReason {get; set;} = string.Empty;

    }

    // Message for payment validation events

    public class PaymentValidationMessage : PaymentMessage
    {
        public List<string> ValidationErrors {get; set;} = new List<string>();
    }
    
}