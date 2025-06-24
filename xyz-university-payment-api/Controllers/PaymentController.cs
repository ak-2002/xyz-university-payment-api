// Purpose: Handles payment operations with comprehensive functionality
using Microsoft.AspNetCore.Mvc;
using xyz_university_payment_api.Services;
using xyz_university_payment_api.Interfaces;
using xyz_university_payment_api.Models;
using xyz_university_payment_api.DTOs;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;

namespace xyz_university_payment_api.Controllers
{
    [Authorize(Policy = "ApiScope")]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;
        private readonly IMapper _mapper;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger, IMapper mapper)
        {
            _paymentService = paymentService;
            _logger = logger;
            _mapper = mapper;
        }

        // POST api/payments/notify
        // Receives and processes payment notifications from Family Bank
        [HttpPost("notify")]
        public async Task<IActionResult> NotifyPayment([FromBody] CreatePaymentDto createPaymentDto)
        {
            _logger.LogInformation("NotifyPayment endpoint called with reference: {PaymentReference}", createPaymentDto.PaymentReference);
            
            // Map DTO to model
            var payment = _mapper.Map<PaymentNotification>(createPaymentDto);
            var result = await _paymentService.ProcessPaymentAsync(payment);

            if (!result.Success)
            {
                _logger.LogWarning("Payment processing failed: {Message}", result.Message);
                return BadRequest(new ApiResponseDto<PaymentResponseDto>
                {
                    Success = false,
                    Message = result.Message,
                    Data = new PaymentResponseDto
                    {
                        Success = false,
                        Message = result.Message,
                        StudentExists = result.StudentExists,
                        StudentIsActive = result.StudentIsActive,
                        ValidationErrors = new List<string>()
                    }
                });
            }

            _logger.LogInformation("Payment processed successfully: {PaymentReference}", createPaymentDto.PaymentReference);
            
            // Map result to DTO
            var paymentResponseDto = _mapper.Map<PaymentResponseDto>(result.ProcessedPayment);
            paymentResponseDto.Success = true;
            paymentResponseDto.Message = result.Message;
            paymentResponseDto.StudentExists = result.StudentExists;
            paymentResponseDto.StudentIsActive = result.StudentIsActive;

            return Ok(new ApiResponseDto<PaymentResponseDto>
            {
                Success = true,
                Message = "Payment processed successfully",
                Data = paymentResponseDto
            });
        }

        // GET api/payments
        // Retrieves all payments
        [HttpGet]
        public async Task<IActionResult> GetAllPayments([FromQuery] PaginationDto pagination)
        {
            _logger.LogInformation("GetAllPayments endpoint called with page {PageNumber}", pagination.PageNumber);
            var payments = await _paymentService.GetAllPaymentsAsync();
            
            // Map to DTOs
            var paymentDtos = _mapper.Map<List<PaymentDto>>(payments);
            
            // Apply pagination
            var totalCount = paymentDtos.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / pagination.PageSize);
            var hasPreviousPage = pagination.PageNumber > 1;
            var hasNextPage = pagination.PageNumber < totalPages;
            
            var pagedPayments = paymentDtos
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();

            var pagedResult = new PagedResultDto<PaymentDto>
            {
                Items = pagedPayments,
                TotalCount = totalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = hasPreviousPage,
                HasNextPage = hasNextPage,
                SortBy = pagination.SortBy,
                SortOrder = pagination.SortOrder
            };

            return Ok(new ApiResponseDto<PagedResultDto<PaymentDto>>
            {
                Success = true,
                Message = "Payments retrieved successfully",
                Data = pagedResult
            });
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
                return NotFound(new ApiResponseDto<PaymentDto>
                {
                    Success = false,
                    Message = "Payment not found",
                    Data = null
                });
            }
            
            var paymentDto = _mapper.Map<PaymentDto>(payment);
            return Ok(new ApiResponseDto<PaymentDto>
            {
                Success = true,
                Message = "Payment retrieved successfully",
                Data = paymentDto
            });
        }

        // GET api/payments/student/{studentNumber}
        // Retrieves payments for a specific student
        [HttpGet("student/{studentNumber}")]
        public async Task<IActionResult> GetPaymentsByStudent(string studentNumber, [FromQuery] PaginationDto pagination)
        {
            _logger.LogInformation("GetPaymentsByStudent endpoint called for student: {StudentNumber}", studentNumber);
            var payments = await _paymentService.GetPaymentsByStudentAsync(studentNumber);
            
            // Map to DTOs
            var paymentDtos = _mapper.Map<List<PaymentDto>>(payments);
            
            // Apply pagination
            var totalCount = paymentDtos.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / pagination.PageSize);
            var hasPreviousPage = pagination.PageNumber > 1;
            var hasNextPage = pagination.PageNumber < totalPages;
            
            var pagedPayments = paymentDtos
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();

            var pagedResult = new PagedResultDto<PaymentDto>
            {
                Items = pagedPayments,
                TotalCount = totalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = hasPreviousPage,
                HasNextPage = hasNextPage,
                SortBy = pagination.SortBy,
                SortOrder = pagination.SortOrder
            };

            return Ok(new ApiResponseDto<PagedResultDto<PaymentDto>>
            {
                Success = true,
                Message = "Student payments retrieved successfully",
                Data = pagedResult
            });
        }

        // GET api/payments/range
        // Retrieves payments within a date range
        [HttpGet("range")]
        public async Task<IActionResult> GetPaymentsByDateRange([FromQuery] DateRangeDto dateRange, [FromQuery] PaginationDto pagination)
        {
            _logger.LogInformation("GetPaymentsByDateRange endpoint called from {StartDate} to {EndDate}", dateRange.StartDate, dateRange.EndDate);
            var payments = await _paymentService.GetPaymentsByDateRangeAsync(dateRange.StartDate, dateRange.EndDate);
            
            // Map to DTOs
            var paymentDtos = _mapper.Map<List<PaymentDto>>(payments);
            
            // Apply pagination
            var totalCount = paymentDtos.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / pagination.PageSize);
            var hasPreviousPage = pagination.PageNumber > 1;
            var hasNextPage = pagination.PageNumber < totalPages;
            
            var pagedPayments = paymentDtos
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();

            var pagedResult = new PagedResultDto<PaymentDto>
            {
                Items = pagedPayments,
                TotalCount = totalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = hasPreviousPage,
                HasNextPage = hasNextPage,
                SortBy = pagination.SortBy,
                SortOrder = pagination.SortOrder
            };

            return Ok(new ApiResponseDto<PagedResultDto<PaymentDto>>
            {
                Success = true,
                Message = "Payments by date range retrieved successfully",
                Data = pagedResult
            });
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
                return NotFound(new ApiResponseDto<PaymentDto>
                {
                    Success = false,
                    Message = "Payment not found",
                    Data = null
                });
            }
            
            var paymentDto = _mapper.Map<PaymentDto>(payment);
            return Ok(new ApiResponseDto<PaymentDto>
            {
                Success = true,
                Message = "Payment retrieved successfully",
                Data = paymentDto
            });
        }

        // POST api/payments/validate
        // Validates payment data
        [HttpPost("validate")]
        public async Task<IActionResult> ValidatePayment([FromBody] CreatePaymentDto createPaymentDto)
        {
            _logger.LogInformation("ValidatePayment endpoint called for payment: {PaymentReference}", createPaymentDto.PaymentReference);
            
            // Map DTO to model
            var payment = _mapper.Map<PaymentNotification>(createPaymentDto);
            var validation = await _paymentService.ValidatePaymentAsync(payment);
            
            var validationDto = new PaymentValidationDto
            {
                IsValid = validation.IsValid,
                Errors = validation.Errors,
                PaymentReference = createPaymentDto.PaymentReference
            };
            
            return Ok(new ApiResponseDto<PaymentValidationDto>
            {
                Success = true,
                Message = validation.IsValid ? "Payment validation successful" : "Payment validation failed",
                Data = validationDto
            });
        }

        // GET api/payments/validate-reference/{paymentReference}
        // Validates if a payment reference is valid
        [HttpGet("validate-reference/{paymentReference}")]
        public async Task<IActionResult> ValidatePaymentReference(string paymentReference)
        {
            _logger.LogInformation("ValidatePaymentReference endpoint called for reference: {PaymentReference}", paymentReference);
            
            var isValid = await _paymentService.IsPaymentReferenceValidAsync(paymentReference);
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Payment reference validation completed",
                Data = new { paymentReference, isValid }
            });
        }

        // GET api/payments/student/{studentNumber}/total
        // Gets total amount paid by a student
        [HttpGet("student/{studentNumber}/total")]
        public async Task<IActionResult> GetTotalAmountPaidByStudent(string studentNumber)
        {
            _logger.LogInformation("GetTotalAmountPaidByStudent endpoint called for student: {StudentNumber}", studentNumber);
            
            var totalAmount = await _paymentService.GetTotalAmountPaidByStudentAsync(studentNumber);
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Total amount retrieved successfully",
                Data = new { studentNumber, totalAmount }
            });
        }

        // GET api/payments/student/{studentNumber}/summary
        // Gets payment summary for a student
        [HttpGet("student/{studentNumber}/summary")]
        public async Task<IActionResult> GetPaymentSummary(string studentNumber)
        {
            _logger.LogInformation("GetPaymentSummary endpoint called for student: {StudentNumber}", studentNumber);
            
            var summary = await _paymentService.GetPaymentSummaryAsync(studentNumber);
            var summaryDto = _mapper.Map<PaymentSummaryDto>(summary);
            
            return Ok(new ApiResponseDto<PaymentSummaryDto>
            {
                Success = true,
                Message = "Payment summary retrieved successfully",
                Data = summaryDto
            });
        }

        // POST api/payments/batch
        // Processes multiple payments in batch
        [HttpPost("batch")]
        public async Task<IActionResult> ProcessBatchPayments([FromBody] BatchPaymentDto batchPaymentDto)
        {
            _logger.LogInformation("ProcessBatchPayments endpoint called with {Count} payments", batchPaymentDto.Payments.Count);
            
            var result = await _paymentService.ProcessBatchPaymentsAsync(batchPaymentDto.Payments.Select(p => _mapper.Map<PaymentNotification>(p)).ToList());
            var resultDto = _mapper.Map<BatchPaymentResultDto>(batchPaymentDto);
            
            return Ok(new ApiResponseDto<BatchPaymentResultDto>
            {
                Success = true,
                Message = "Batch payment processing completed",
                Data = resultDto
            });
        }

        // POST api/payments/reconcile
        // Reconciles payments with bank data
        [HttpPost("reconcile")]
        public async Task<IActionResult> ReconcilePayments([FromBody] List<BankPaymentDataDto> bankData)
        {
            _logger.LogInformation("ReconcilePayments endpoint called with {Count} bank records", bankData.Count);
            
            var bankPaymentData = bankData.Select(b => new BankPaymentData
            {
                PaymentReference = b.PaymentReference,
                StudentNumber = b.StudentNumber,
                Amount = b.Amount,
                PaymentDate = b.TransactionDate
            }).ToList();
            
            var result = await _paymentService.ReconcilePaymentsAsync(bankPaymentData);
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Payment reconciliation completed",
                Data = result
            });
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

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Test messages published successfully. Check the logs for consumer activity.",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TestMessaging endpoint");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Error publishing test messages",
                    Data = null,
                    Errors = new List<string> { ex.Message }
                });
            }
        }

    }
}    

        
  
