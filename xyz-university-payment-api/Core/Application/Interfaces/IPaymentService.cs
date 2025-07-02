using xyz_university_payment_api.Core.Domain.Entities;

namespace xyz_university_payment_api.Core.Application.Interfaces
{
    
    public interface IPaymentService
    {
        
        Task<PaymentProcessingResult> ProcessPaymentAsync(PaymentNotification payment);

   
        Task<IEnumerable<PaymentNotification>> GetAllPaymentsAsync();

        Task<PaymentNotification?> GetPaymentByIdAsync(int id);

        
        Task<IEnumerable<PaymentNotification>> GetPaymentsByStudentAsync(string studentNumber);

        Task<IEnumerable<PaymentNotification>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);

      
        Task<PaymentNotification?> GetPaymentByReferenceAsync(string paymentReference);

      
        Task<(bool IsValid, List<string> Errors)> ValidatePaymentAsync(PaymentNotification payment);

        Task<bool> IsPaymentReferenceValidAsync(string paymentReference);

        // Alias for consistency with controller
        Task<bool> PaymentReferenceExistsAsync(string paymentReference) => IsPaymentReferenceValidAsync(paymentReference);

       
        Task<decimal> GetTotalAmountPaidByStudentAsync(string studentNumber);

       
        Task<PaymentSummary> GetPaymentSummaryAsync(string studentNumber);

       
      
        Task<BatchProcessingResult> ProcessBatchPaymentsAsync(IEnumerable<PaymentNotification> payments);

        Task<ReconciliationResult> ReconcilePaymentsAsync(IEnumerable<BankPaymentData> bankData);

        // Test messaging method
        Task TestMessagingAsync();
    }

  
    public class PaymentProcessingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool StudentExists { get; set; }
        public bool StudentIsActive { get; set; }
        public PaymentNotification? ProcessedPayment { get; set; }
        public List<string> Warnings { get; set; } = new();
    }

  
    public class BatchProcessingResult
    {
        public int TotalProcessed { get; set; }
        public int Successful { get; set; }
        public int Failed { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<PaymentProcessingResult> Results { get; set; } = new();
        public List<PaymentNotification> SuccessfulPayments { get; set; } = new();
        public List<PaymentFailure> FailedPayments { get; set; } = new();
    }

    public class PaymentFailure
    {
        public PaymentNotification? Payment { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

  
    public class PaymentSummary
    {
        public string StudentNumber { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TotalPayments { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public decimal? AveragePaymentAmount { get; set; }
        public bool StudentIsActive { get; set; }
    }


    public class ReconciliationResult
    {
        public int TotalBankPayments { get; set; }
        public int MatchedPayments { get; set; }
        public List<BankPaymentData> UnmatchedPayments { get; set; } = new();
        public List<PaymentNotification> MissingPayments { get; set; } = new();
        public int TotalReconciled { get; set; }
        public int DiscrepanciesFound { get; set; }
        public List<string> Discrepancies { get; set; } = new();
        public bool ReconciliationSuccessful { get; set; }
    }

   
    public class BankPaymentData
    {
        public string PaymentReference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
    }
} 