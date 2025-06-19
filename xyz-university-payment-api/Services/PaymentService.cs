// Purpose: Payment service implementation with business logic
// Provides concrete implementation of IPaymentService interface
using xyz_university_payment_api.Interfaces;
using xyz_university_payment_api.Models;

namespace xyz_university_payment_api.Services
{
    /// <summary>
    /// Payment service implementation
    /// Provides concrete implementation of IPaymentService interface with business logic
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IPaymentRepository paymentRepository, IStudentRepository studentRepository, ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _studentRepository = studentRepository;
            _logger = logger;
        }

        public async Task<PaymentProcessingResult> ProcessPaymentAsync(PaymentNotification payment)
        {
            _logger.LogInformation("Processing payment: {PaymentReference} for student: {StudentNumber}", 
                payment.PaymentReference, payment.StudentNumber);

            // Validate payment
            var validation = await ValidatePaymentAsync(payment);
            if (!validation.IsValid)
            {
                _logger.LogWarning("Payment validation failed: {Errors}", string.Join(", ", validation.Errors));
                return new PaymentProcessingResult
                {
                    Success = false,
                    Message = $"Payment validation failed: {string.Join(", ", validation.Errors)}",
                    StudentExists = false,
                    StudentIsActive = false
                };
            }

            // Check for duplicate payment reference
            if (await _paymentRepository.PaymentReferenceExistsAsync(payment.PaymentReference))
            {
                _logger.LogWarning("Duplicate payment reference: {PaymentReference}", payment.PaymentReference);
                return new PaymentProcessingResult
                {
                    Success = false,
                    Message = $"Payment reference {payment.PaymentReference} already exists",
                    StudentExists = false,
                    StudentIsActive = false
                };
            }

            // Check if student exists
            var student = await _studentRepository.GetByStudentNumberAsync(payment.StudentNumber);
            if (student == null)
            {
                _logger.LogWarning("Student not found for payment: {StudentNumber}", payment.StudentNumber);
                return new PaymentProcessingResult
                {
                    Success = false,
                    Message = "Payment received but student not found.",
                    StudentExists = false,
                    StudentIsActive = false
                };
            }

            // Save the payment regardless of student activity to keep all records
            var processedPayment = await _paymentRepository.AddAsync(payment);

            _logger.LogInformation("Payment processed successfully: {PaymentReference}", payment.PaymentReference);

            return new PaymentProcessingResult
            {
                Success = true,
                Message = student.IsActive ? "Payment processed successfully. Student is currently enrolled." 
                                           : "Payment processed successfully. Student is not currently enrolled.",
                StudentExists = true,
                StudentIsActive = student.IsActive,
                ProcessedPayment = processedPayment
            };
        }

        public async Task<IEnumerable<PaymentNotification>> GetAllPaymentsAsync()
        {
            _logger.LogInformation("Retrieving all payments");
            return await _paymentRepository.GetAllAsync();
        }

        public async Task<PaymentNotification?> GetPaymentByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving payment with ID: {PaymentId}", id);
            return await _paymentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<PaymentNotification>> GetPaymentsByStudentAsync(string studentNumber)
        {
            _logger.LogInformation("Retrieving payments for student: {StudentNumber}", studentNumber);
            return await _paymentRepository.GetPaymentsByStudentAsync(studentNumber);
        }

        public async Task<IEnumerable<PaymentNotification>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            _logger.LogInformation("Retrieving payments from {StartDate} to {EndDate}", startDate, endDate);
            return await _paymentRepository.GetPaymentsByDateRangeAsync(startDate, endDate);
        }

        public async Task<PaymentNotification?> GetPaymentByReferenceAsync(string paymentReference)
        {
            _logger.LogInformation("Retrieving payment by reference: {PaymentReference}", paymentReference);
            return await _paymentRepository.GetByPaymentReferenceAsync(paymentReference);
        }

        public Task<(bool IsValid, List<string> Errors)> ValidatePaymentAsync(PaymentNotification payment)
        {
            var errors = new List<string>();

            // Validate payment reference
            if (string.IsNullOrWhiteSpace(payment.PaymentReference))
            {
                errors.Add("Payment reference is required");
            }
            else if (!payment.PaymentReference.StartsWith("REF") || payment.PaymentReference.Length < 6)
            {
                errors.Add("Payment reference must start with 'REF' and be at least 6 characters long");
            }

            // Validate student number
            if (string.IsNullOrWhiteSpace(payment.StudentNumber))
            {
                errors.Add("Student number is required");
            }

            // Validate amount
            if (payment.AmountPaid <= 0)
            {
                errors.Add("Payment amount must be greater than zero");
            }
            else if (payment.AmountPaid > 100000) // Business rule: Maximum payment amount
            {
                errors.Add("Payment amount cannot exceed 100,000");
            }

            // Validate payment date
            if (payment.PaymentDate > DateTime.UtcNow)
            {
                errors.Add("Payment date cannot be in the future");
            }

            return Task.FromResult((errors.Count == 0, errors));
        }

        public async Task<bool> IsPaymentReferenceValidAsync(string paymentReference)
        {
            _logger.LogInformation("Validating payment reference: {PaymentReference}", paymentReference);
            
            // Check if payment reference already exists
            return !await _paymentRepository.PaymentReferenceExistsAsync(paymentReference);
        }

        public async Task<decimal> GetTotalAmountPaidByStudentAsync(string studentNumber)
        {
            _logger.LogInformation("Calculating total amount paid by student: {StudentNumber}", studentNumber);
            return await _paymentRepository.GetTotalAmountPaidByStudentAsync(studentNumber);
        }

        public async Task<PaymentSummary> GetPaymentSummaryAsync(string studentNumber)
        {
            _logger.LogInformation("Generating payment summary for student: {StudentNumber}", studentNumber);

            var payments = await _paymentRepository.GetPaymentsByStudentAsync(studentNumber);
            var paymentsList = payments.ToList();

            var summary = new PaymentSummary
            {
                StudentNumber = studentNumber,
                TotalAmountPaid = paymentsList.Sum(p => p.AmountPaid),
                TotalPayments = paymentsList.Count,
                LastPaymentDate = paymentsList.Any() ? paymentsList.Max(p => p.PaymentDate) : null,
                AveragePaymentAmount = paymentsList.Any() ? paymentsList.Average(p => p.AmountPaid) : null
            };

            return summary;
        }

        public async Task<BatchProcessingResult> ProcessBatchPaymentsAsync(IEnumerable<PaymentNotification> payments)
        {
            _logger.LogInformation("Processing batch of {Count} payments", payments.Count());

            var result = new BatchProcessingResult();
            var paymentList = payments.ToList();

            foreach (var payment in paymentList)
            {
                try
                {
                    var processingResult = await ProcessPaymentAsync(payment);
                    result.Results.Add(processingResult);

                    if (processingResult.Success)
                    {
                        result.Successful++;
                    }
                    else
                    {
                        result.Failed++;
                        result.Errors.Add($"Payment {payment.PaymentReference}: {processingResult.Message}");
                    }
                }
                catch (Exception ex)
                {
                    result.Failed++;
                    result.Errors.Add($"Payment {payment.PaymentReference}: {ex.Message}");
                    _logger.LogError(ex, "Error processing payment: {PaymentReference}", payment.PaymentReference);
                }
            }

            result.TotalProcessed = paymentList.Count;
            _logger.LogInformation("Batch processing completed: {Successful} successful, {Failed} failed", 
                result.Successful, result.Failed);

            return result;
        }

        public async Task<ReconciliationResult> ReconcilePaymentsAsync(IEnumerable<BankPaymentData> bankData)
        {
            _logger.LogInformation("Reconciling payments with bank data");

            var result = new ReconciliationResult();
            var bankDataList = bankData.ToList();

            foreach (var bankPayment in bankDataList)
            {
                var dbPayment = await _paymentRepository.GetByPaymentReferenceAsync(bankPayment.PaymentReference);
                
                if (dbPayment == null)
                {
                    result.DiscrepanciesFound++;
                    result.Discrepancies.Add($"Payment reference {bankPayment.PaymentReference} not found in database");
                }
                else if (dbPayment.AmountPaid != bankPayment.Amount)
                {
                    result.DiscrepanciesFound++;
                    result.Discrepancies.Add($"Amount mismatch for {bankPayment.PaymentReference}: DB={dbPayment.AmountPaid}, Bank={bankPayment.Amount}");
                }
                else if (dbPayment.StudentNumber != bankPayment.StudentNumber)
                {
                    result.DiscrepanciesFound++;
                    result.Discrepancies.Add($"Student number mismatch for {bankPayment.PaymentReference}: DB={dbPayment.StudentNumber}, Bank={bankPayment.StudentNumber}");
                }
                else
                {
                    result.TotalReconciled++;
                }
            }

            result.ReconciliationSuccessful = result.DiscrepanciesFound == 0;
            _logger.LogInformation("Reconciliation completed: {Reconciled} reconciled, {Discrepancies} discrepancies", 
                result.TotalReconciled, result.DiscrepanciesFound);

            return result;
        }
    }
}