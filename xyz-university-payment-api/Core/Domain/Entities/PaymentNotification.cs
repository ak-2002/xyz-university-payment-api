// Purpose: Represents payment transaction data received from Family Bank
namespace xyz_university_payment_api.Core.Domain.Entities
{
    public class PaymentNotification
    {
        public int Id { get; set; } // Primary key
        public required string StudentNumber { get; set; } // Student number linked to the payment
        public required string PaymentReference { get; set; } // Bank-provided payment reference number
        public decimal AmountPaid { get; set; } // Payment amount
        public DateTime PaymentDate { get; set; } // Date when the payment was made
        public DateTime DateReceived { get; set; } = DateTime.UtcNow; // Date when the payment was received by the API
        public string PaymentMethod { get; set; } = "M-Pesa"; // Payment method: M-Pesa, Cash, Cheque, Bank Transfer
        public string? TransactionId { get; set; } // Transaction ID for M-Pesa or bank transfers
        public string? ReceiptNumber { get; set; } // Receipt number for cash/cheque payments
        public string? Notes { get; set; } // Additional notes about the payment
    }
}