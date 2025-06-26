// Purpose: Payment service implementation with business logic
// Provides concrete implementation of IPaymentService interface
using xyz_university_payment_api.Interfaces;
using xyz_university_payment_api.Models;
using xyz_university_payment_api.Exceptions;
using PaymentNotification = xyz_university_payment_api.Models.PaymentNotification;

namespace xyz_university_payment_api.Services
{
    /// <summary>
    /// Payment service implementation
    /// Provides concrete implementation of IPaymentService interface with business logic
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentService> _logger;
        private readonly IMessagePublisher _messagePublisher;

        public PaymentService(IUnitOfWork unitOfWork, ILogger<PaymentService> logger, IMessagePublisher messagePublisher)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _messagePublisher = messagePublisher;
        }

        public async Task<PaymentProcessingResult> ProcessPaymentAsync(PaymentNotification payment)
        {
            _logger.LogInformation("Processing payment: {PaymentReference} for student: {StudentNumber}", 
                payment.PaymentReference, payment.StudentNumber);

            try
            {
                // Validate payment
                var validation = await ValidatePaymentAsync(payment);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Payment validation failed: {Errors}", string.Join(", ", validation.Errors));
                    
                    // Publish validation failure message
                    await _messagePublisher.PublishPaymentValidationAsync(new PaymentValidationMessage
                    {
                        PaymentReference = payment.PaymentReference,
                        StudentNumber = payment.StudentNumber,
                        Amount = payment.AmountPaid,
                        PaymentDate = payment.PaymentDate,
                        Status = "ValidationFailed",
                        Message = $"Payment validation failed: {string.Join(", ", validation.Errors)}",
                        ValidationErrors = validation.Errors
                    });

                    throw new ValidationException(validation.Errors);
                }

                // Check for duplicate payment reference
                if (await _unitOfWork.Payments.AnyAsync(p => p.PaymentReference == payment.PaymentReference))
                {
                    _logger.LogWarning("Duplicate payment reference: {PaymentReference}", payment.PaymentReference);
                    
                    // Publish failed payment message
                    await _messagePublisher.PublishPaymentFailedAsync(new PaymentFailedMessage
                    {
                        PaymentReference = payment.PaymentReference,
                        StudentNumber = payment.StudentNumber,
                        Amount = payment.AmountPaid,
                        PaymentDate = payment.PaymentDate,
                        Status = "DuplicateReference",
                        Message = $"Payment reference {payment.PaymentReference} already exists",
                        ErrorReason = "Duplicate payment reference"
                    });

                    throw new DuplicatePaymentException(payment.PaymentReference);
                }

                // Check if student exists
                var student = await _unitOfWork.Students.FirstOrDefaultAsync(s => s.StudentNumber == payment.StudentNumber);
                if (student == null)
                {
                    _logger.LogWarning("Student not found for payment: {StudentNumber}", payment.StudentNumber);
                    
                    // Publish failed payment message
                    await _messagePublisher.PublishPaymentFailedAsync(new PaymentFailedMessage
                    {
                        PaymentReference = payment.PaymentReference,
                        StudentNumber = payment.StudentNumber,
                        Amount = payment.AmountPaid,
                        PaymentDate = payment.PaymentDate,
                        Status = "StudentNotFound",
                        Message = "Payment received but student not found.",
                        ErrorReason = "Student not found in system"
                    });

                    throw new StudentNotFoundException(payment.StudentNumber);
                }

                // Save the payment regardless of student activity to keep all records
                var processedPayment = await _unitOfWork.Payments.AddAsync(payment);

                _logger.LogInformation("Payment processed successfully: {PaymentReference}", payment.PaymentReference);

                // Publish successful payment message
                await _messagePublisher.PublishPaymentProcessedAsync(new PaymentProcessedMessage
                {
                    PaymentReference = payment.PaymentReference,
                    StudentNumber = payment.StudentNumber,
                    Amount = payment.AmountPaid,
                    PaymentDate = payment.PaymentDate,
                    Status = "Processed",
                    Message = student.IsActive ? "Payment processed successfully. Student is currently enrolled." 
                                               : "Payment processed successfully. Student is not currently enrolled.",
                    StudentExists = true,
                    StudentIsActive = student.IsActive
                });

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
            catch (ApiException)
            {
                // Re-throw custom exceptions as they are already properly formatted
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing payment: {PaymentReference}", payment.PaymentReference);
                throw new PaymentProcessingException("An unexpected error occurred while processing the payment", ex);
            }
        }

        public async Task<IEnumerable<PaymentNotification>> GetAllPaymentsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all payments");
                return await _unitOfWork.Payments.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all payments");
                throw new DatabaseException("Failed to retrieve payments", ex);
            }
        }

        public async Task<PaymentNotification?> GetPaymentByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving payment with ID: {PaymentId}", id);
                var payment = await _unitOfWork.Payments.GetByIdAsync(id);
                
                if (payment == null)
                {
                    throw new PaymentNotFoundException(id);
                }
                
                return payment;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment with ID: {PaymentId}", id);
                throw new DatabaseException($"Failed to retrieve payment with ID {id}", ex);
            }
        }

        public async Task<IEnumerable<PaymentNotification>> GetPaymentsByStudentAsync(string studentNumber)
        {
            try
            {
                _logger.LogInformation("Retrieving payments for student: {StudentNumber}", studentNumber);
                return await _unitOfWork.Payments.FindAsync(p => p.StudentNumber == studentNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for student: {StudentNumber}", studentNumber);
                throw new DatabaseException($"Failed to retrieve payments for student {studentNumber}", ex);
            }
        }

        public async Task<IEnumerable<PaymentNotification>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Retrieving payments from {StartDate} to {EndDate}", startDate, endDate);
                return await _unitOfWork.Payments.FindAsync(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments by date range");
                throw new DatabaseException("Failed to retrieve payments by date range", ex);
            }
        }

        public async Task<PaymentNotification?> GetPaymentByReferenceAsync(string paymentReference)
        {
            try
            {
                _logger.LogInformation("Retrieving payment by reference: {PaymentReference}", paymentReference);
                var payment = await _unitOfWork.Payments.FirstOrDefaultAsync(p => p.PaymentReference == paymentReference);
                
                if (payment == null)
                {
                    throw new PaymentNotFoundException(paymentReference);
                }
                
                return payment;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment by reference: {PaymentReference}", paymentReference);
                throw new DatabaseException($"Failed to retrieve payment with reference {paymentReference}", ex);
            }
        }

        public Task<(bool IsValid, List<string> Errors)> ValidatePaymentAsync(PaymentNotification payment)
        {
            var errors = new List<string>();

            // Validate payment reference
            if (string.IsNullOrWhiteSpace(payment.PaymentReference))
            {
                errors.Add("Payment reference is required");
            }
            else if (payment.PaymentReference.Length > 50)
            {
                errors.Add("Payment reference cannot exceed 50 characters");
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

            // Validate payment date
            if (payment.PaymentDate > DateTime.UtcNow)
            {
                errors.Add("Payment date cannot be in the future");
            }

            return Task.FromResult((errors.Count == 0, errors));
        }

        public async Task<bool> IsPaymentReferenceValidAsync(string paymentReference)
        {
            try
            {
                return !await _unitOfWork.Payments.AnyAsync(p => p.PaymentReference == paymentReference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking payment reference validity: {PaymentReference}", paymentReference);
                throw new DatabaseException("Failed to check payment reference validity", ex);
            }
        }

        public async Task<decimal> GetTotalAmountPaidByStudentAsync(string studentNumber)
        {
            try
            {
                var payments = await _unitOfWork.Payments.FindAsync(p => p.StudentNumber == studentNumber);
                return payments.Sum(p => p.AmountPaid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total amount for student: {StudentNumber}", studentNumber);
                throw new DatabaseException($"Failed to calculate total amount for student {studentNumber}", ex);
            }
        }

        public async Task<PaymentSummary> GetPaymentSummaryAsync(string studentNumber)
        {
            try
            {
                var payments = await _unitOfWork.Payments.FindAsync(p => p.StudentNumber == studentNumber);
                var student = await _unitOfWork.Students.FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);

                return new PaymentSummary
                {
                    StudentNumber = studentNumber,
                    StudentName = student?.FullName ?? "Unknown",
                    TotalAmount = payments.Sum(p => p.AmountPaid),
                    TotalPayments = payments.Count(),
                    LastPaymentDate = payments.Any() ? payments.Max(p => p.PaymentDate) : null,
                    AveragePaymentAmount = payments.Any() ? payments.Average(p => p.AmountPaid) : null,
                    StudentIsActive = student?.IsActive ?? false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating payment summary for student: {StudentNumber}", studentNumber);
                throw new DatabaseException($"Failed to generate payment summary for student {studentNumber}", ex);
            }
        }

        public async Task<BatchProcessingResult> ProcessBatchPaymentsAsync(IEnumerable<PaymentNotification> payments)
        {
            var result = new BatchProcessingResult
            {
                TotalProcessed = 0,
                Successful = 0,
                Failed = 0,
                Errors = new List<string>(),
                Results = new List<PaymentProcessingResult>(),
                SuccessfulPayments = new List<PaymentNotification>(),
                FailedPayments = new List<PaymentFailure>()
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                foreach (var payment in payments)
                {
                    try
                    {
                        var processedPayment = await ProcessPaymentAsync(payment);
                        result.SuccessfulPayments.Add(processedPayment.ProcessedPayment!);
                        result.Results.Add(processedPayment);
                        result.Successful++;
                        result.TotalProcessed++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedPayments.Add(new PaymentFailure
                        {
                            Payment = payment,
                            ErrorMessage = ex.Message
                        });
                        result.Errors.Add($"Payment {payment.PaymentReference}: {ex.Message}");
                        result.Failed++;
                        result.TotalProcessed++;
                    }
                }

                await _unitOfWork.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error processing batch payments");
                throw new BatchProcessingException("Failed to process batch payments", ex);
            }
        }

        public async Task<ReconciliationResult> ReconcilePaymentsAsync(IEnumerable<BankPaymentData> bankData)
        {
            var result = new ReconciliationResult
            {
                TotalBankPayments = bankData.Count(),
                MatchedPayments = 0,
                UnmatchedPayments = new List<BankPaymentData>(),
                MissingPayments = new List<PaymentNotification>(),
                TotalReconciled = 0,
                DiscrepanciesFound = 0,
                Discrepancies = new List<string>(),
                ReconciliationSuccessful = true
            };

            try
            {
                foreach (var bankPayment in bankData)
                {
                    var systemPayment = await _unitOfWork.Payments.FirstOrDefaultAsync(
                        p => p.PaymentReference == bankPayment.PaymentReference);

                    if (systemPayment != null)
                    {
                        result.MatchedPayments++;
                        result.TotalReconciled++;
                    }
                    else
                    {
                        result.UnmatchedPayments.Add(bankPayment);
                        result.DiscrepanciesFound++;
                        result.Discrepancies.Add($"Payment reference {bankPayment.PaymentReference} not found in system");
                    }
                }

                result.ReconciliationSuccessful = result.DiscrepanciesFound == 0;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reconciling payments");
                throw new ReconciliationException("Failed to reconcile payments", ex);
            }
        }
    }
}