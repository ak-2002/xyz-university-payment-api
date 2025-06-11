// Purpose: Represents payment transaction data received from Family Bank
namespace xyz_university_payment_api.Models
{
    public class PaymentNotification
    {
        public int Id { get; set; } // Primary key
        public string StudentNumber { get; set; } // Student number linked to the payment
        public string PaymentReference { get; set; } // Bank-provided payment reference number
        public decimal AmountPaid { get; set; } // Payment amount
        public DateTime PaymentDate { get; set; } // Date when the payment was made
        public DateTime DateReceived { get; set; } = DateTime.UtcNow; // Date when the payment was received by the API
    }
}