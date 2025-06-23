// Purpose: Handles payment operations with comprehensive functionality
using Microsoft.AspNetCore.Mvc;
using xyz_university_payment_api.Services;
using xyz_university_payment_api.Interfaces;
using xyz_university_payment_api.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace xyz_university_payment_api.Controllers
{
    [Authorize(Policy = "ApiScope")]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        // POST api/payments/notify
        // Receives and processes payment notifications from Family Bank
        [HttpPost("notify")]
        public async Task<IActionResult> NotifyPayment([FromBody] PaymentNotification payment)
        {
            _logger.LogInformation("NotifyPayment endpoint called with reference: {PaymentReference}", payment.PaymentReference);
            var result = await _paymentService.ProcessPaymentAsync(payment);

            if (!result.Success)
            {
                _logger.LogWarning("Payment processing failed: {Message}", result.Message);
                return BadRequest(new
                {
                    success = false,
                    message = result.Message,
                    studentExists = result.StudentExists,
                    studentIsActive = result.StudentIsActive
                });
            }

            _logger.LogInformation("Payment processed successfully: {PaymentReference}", payment.PaymentReference);
            return Ok(new
            {
                success = true,
                message = result.Message,
                studentExists = result.StudentExists,
                studentIsActive = result.StudentIsActive,
                processedPayment = result.ProcessedPayment
            });
        }

        // GET api/payments
        // Retrieves all payments
        [HttpGet]
        public async Task<IActionResult> GetAllPayments()
        {
            _logger.LogInformation("GetAllPayments endpoint called");
            var payments = await _paymentService.GetAllPaymentsAsync();
            return Ok(payments);
        }

        // GET api/payments/{id}
        // Retrieves a payment by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            _logger.LogInformation("GetPaymentById endpoint called with ID: {PaymentId}", id);
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            
            if (payment == null)
            {
                _logger.LogWarning("Payment not found with ID: {PaymentId}", id);
                return NotFound(new { message = "Payment not found" });
            }
            
            return Ok(payment);
        }

        // GET api/payments/student/{studentNumber}
        // Retrieves payments for a specific student
        [HttpGet("student/{studentNumber}")]
        public async Task<IActionResult> GetPaymentsByStudent(string studentNumber)
        {
            _logger.LogInformation("GetPaymentsByStudent endpoint called for student: {StudentNumber}", studentNumber);
            var payments = await _paymentService.GetPaymentsByStudentAsync(studentNumber);
            return Ok(payments);
        }

        // GET api/payments/range
        // Retrieves payments within a date range
        [HttpGet("range")]
        public async Task<IActionResult> GetPaymentsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            _logger.LogInformation("GetPaymentsByDateRange endpoint called from {StartDate} to {EndDate}", startDate, endDate);
            var payments = await _paymentService.GetPaymentsByDateRangeAsync(startDate, endDate);
            return Ok(payments);
        }

        // GET api/payments/reference/{paymentReference}
        // Retrieves a payment by reference number
        [HttpGet("reference/{paymentReference}")]
        public async Task<IActionResult> GetPaymentByReference(string paymentReference)
        {
            _logger.LogInformation("GetPaymentByReference endpoint called with reference: {PaymentReference}", paymentReference);
            var payment = await _paymentService.GetPaymentByReferenceAsync(paymentReference);
            
            if (payment == null)
            {
                _logger.LogWarning("Payment not found with reference: {PaymentReference}", paymentReference);
                return NotFound(new { message = "Payment not found" });
            }
            
            return Ok(payment);
        }

        // POST api/payments/validate
        // Validates payment data
        [HttpPost("validate")]
        public async Task<IActionResult> ValidatePayment([FromBody] PaymentNotification payment)
        {
            _logger.LogInformation("ValidatePayment endpoint called for payment: {PaymentReference}", payment.PaymentReference);
            
            var validation = await _paymentService.ValidatePaymentAsync(payment);
            return Ok(new { isValid = validation.IsValid, errors = validation.Errors });
        }

        // GET api/payments/validate-reference/{paymentReference}
        // Validates if a payment reference is valid
        [HttpGet("validate-reference/{paymentReference}")]
        public async Task<IActionResult> ValidatePaymentReference(string paymentReference)
        {
            _logger.LogInformation("ValidatePaymentReference endpoint called for reference: {PaymentReference}", paymentReference);
            
            var isValid = await _paymentService.IsPaymentReferenceValidAsync(paymentReference);
            return Ok(new { paymentReference, isValid });
        }

        // GET api/payments/student/{studentNumber}/total
        // Gets total amount paid by a student
        [HttpGet("student/{studentNumber}/total")]
        public async Task<IActionResult> GetTotalAmountPaidByStudent(string studentNumber)
        {
            _logger.LogInformation("GetTotalAmountPaidByStudent endpoint called for student: {StudentNumber}", studentNumber);
            
            var totalAmount = await _paymentService.GetTotalAmountPaidByStudentAsync(studentNumber);
            return Ok(new { studentNumber, totalAmount });
        }

        // GET api/payments/student/{studentNumber}/summary
        // Gets payment summary for a student
        [HttpGet("student/{studentNumber}/summary")]
        public async Task<IActionResult> GetPaymentSummary(string studentNumber)
        {
            _logger.LogInformation("GetPaymentSummary endpoint called for student: {StudentNumber}", studentNumber);
            
            var summary = await _paymentService.GetPaymentSummaryAsync(studentNumber);
            return Ok(summary);
        }

        // POST api/payments/batch
        // Processes multiple payments in batch
        [HttpPost("batch")]
        public async Task<IActionResult> ProcessBatchPayments([FromBody] List<PaymentNotification> payments)
        {
            _logger.LogInformation("ProcessBatchPayments endpoint called with {Count} payments", payments.Count);
            
            var result = await _paymentService.ProcessBatchPaymentsAsync(payments);
            return Ok(result);
        }

        // POST api/payments/reconcile
        // Reconciles payments with bank data
        [HttpPost("reconcile")]
        public async Task<IActionResult> ReconcilePayments([FromBody] List<BankPaymentData> bankData)
        {
            _logger.LogInformation("ReconcilePayments endpoint called with {Count} bank records", bankData.Count);
            
            var result = await _paymentService.ReconcilePaymentsAsync(bankData);
            return Ok(result);
        }
              

        // POST api/payments/test-messaging
        // Test endpoint to verify RabbitMQ messaging functionality
        [HttpPost("test-messaging")]
        public async Task<IActionResult> TestMessaging()
        {
            _logger.LogInformation("TestMessaging endpoint called");
            
            try
            {
                // Get the message publisher from the service provider
                var messagePublisher = HttpContext.RequestServices.GetRequiredService<IMessagePublisher>();
                
                // Test payment processed message
                await messagePublisher.PublishPaymentProcessedAsync(new PaymentProcessedMessage
                {
                    PaymentReference = "TEST-REF-001",
                    StudentNumber = "S12345",
                    Amount = 5000m,
                    PaymentDate = DateTime.UtcNow,
                    Status = "TestProcessed",
                    Message = "Test payment processed successfully",
                    StudentExists = true,
                    StudentIsActive = true
                });

                // Test payment failed message
                await messagePublisher.PublishPaymentFailedAsync(new PaymentFailedMessage
                {
                    PaymentReference = "TEST-REF-002",
                    StudentNumber = "S67890",
                    Amount = 3000m,
                    PaymentDate = DateTime.UtcNow,
                    Status = "TestFailed",
                    Message = "Test payment failed",
                    ErrorReason = "Test error reason"
                });

                // Test payment validation message
                await messagePublisher.PublishPaymentValidationAsync(new PaymentValidationMessage
                {
                    PaymentReference = "TEST-REF-003",
                    StudentNumber = "S66001",
                    Amount = 4000m,
                    PaymentDate = DateTime.UtcNow,
                    Status = "TestValidation",
                    Message = "Test payment validation",
                    ValidationErrors = new List<string> { "Test validation error 1", "Test validation error 2" }
                });

                return Ok(new { 
                    success = true, 
                    message = "Test messages published successfully. Check the logs for consumer activity." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TestMessaging endpoint");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error publishing test messages", 
                    error = ex.Message 
                });
            }
        }

    }
}    

        
  
