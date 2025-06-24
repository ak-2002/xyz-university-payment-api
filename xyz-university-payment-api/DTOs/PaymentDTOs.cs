// Purpose: Data Transfer Objects for Payment operations
namespace xyz_university_payment_api.DTOs
{
    // Input DTO for receiving payment notifications from Family Bank
    public class CreatePaymentDto
    {
        public string StudentNumber { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
    }

    // Output DTO for returning payment data to clients
    public class PaymentDto
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime DateReceived { get; set; }
        public string StudentName { get; set; } = string.Empty; // Joined from Student table
        public string StudentProgram { get; set; } = string.Empty; // Joined from Student table
    }

    // Response DTO for payment processing results
    public class PaymentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool StudentExists { get; set; }
        public bool StudentIsActive { get; set; }
        public PaymentDto? ProcessedPayment { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();
    }

    // DTO for payment validation
    public class PaymentValidationDto
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string PaymentReference { get; set; } = string.Empty;
    }

    // DTO for payment summary
    public class PaymentSummaryDto
    {
        public string StudentNumber { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int TotalPayments { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public decimal OutstandingBalance { get; set; }
        public DateTime LastPaymentDate { get; set; }
        public List<PaymentDto> RecentPayments { get; set; } = new List<PaymentDto>();
    }

    // DTO for batch payment processing
    public class BatchPaymentDto
    {
        public List<CreatePaymentDto> Payments { get; set; } = new List<CreatePaymentDto>();
        public bool ValidateOnly { get; set; } = false;
    }

    // DTO for batch payment results
    public class BatchPaymentResultDto
    {
        public int TotalProcessed { get; set; }
        public int SuccessfulPayments { get; set; }
        public int FailedPayments { get; set; }
        public List<PaymentResponseDto> Results { get; set; } = new List<PaymentResponseDto>();
        public List<string> Errors { get; set; } = new List<string>();
    }

    // DTO for payment reconciliation
    public class BankPaymentDataDto
    {
        public string PaymentReference { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string BankTransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    // DTO for reconciliation results
    public class ReconciliationResultDto
    {
        public int TotalBankRecords { get; set; }
        public int MatchedPayments { get; set; }
        public int UnmatchedPayments { get; set; }
        public List<string> Discrepancies { get; set; } = new List<string>();
        public DateTime ReconciliationDate { get; set; } = DateTime.UtcNow;
    }
}