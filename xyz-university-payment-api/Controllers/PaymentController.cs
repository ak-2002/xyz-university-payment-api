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
using xyz_university_payment_api.Attributes;

namespace xyz_university_payment_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all payment endpoints
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
        [AuthorizePayment("create")]
        [Authorize(Roles = "Admin,FinanceManager,Accountant")]
        public async Task<IActionResult> NotifyPayment([FromBody] CreatePaymentDto createPaymentDto)
        {
            _logger.LogInformation("NotifyPayment endpoint called with reference: {PaymentReference}", createPaymentDto.PaymentReference);
            
            // Map DTO to model
            var payment = _mapper.Map<PaymentNotification>(createPaymentDto);
            var result = await _paymentService.ProcessPaymentAsync(payment);

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
        [AuthorizePayment("read")]
        [Authorize(Policy = "PaymentAccess")]
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
        [AuthorizePayment("read")]
        [Authorize(Policy = "PaymentAccess")]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            _logger.LogInformation("GetPaymentById endpoint called with ID: {PaymentId}", id);
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            
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
        [AuthorizePayment("read")]
        [Authorize(Policy = "PaymentAccess")]
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
        [AuthorizePayment("read")]
        [Authorize(Policy = "PaymentAccess")]
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
        // Retrieves a payment by reference
        [HttpGet("reference/{paymentReference}")]
        [AuthorizePayment("read")]
        [Authorize(Policy = "PaymentAccess")]
        public async Task<IActionResult> GetPaymentByReference(string paymentReference)
        {
            _logger.LogInformation("GetPaymentByReference endpoint called with reference: {PaymentReference}", paymentReference);
            var payment = await _paymentService.GetPaymentByReferenceAsync(paymentReference);
            
            var paymentDto = _mapper.Map<PaymentDto>(payment);
            return Ok(new ApiResponseDto<PaymentDto>
            {
                Success = true,
                Message = "Payment retrieved successfully",
                Data = paymentDto
            });
        }

        // POST api/payments/validate
        // Validates a payment without processing it
        [HttpPost("validate")]
        [AuthorizePayment("read")]
        [Authorize(Policy = "PaymentAccess")]
        public async Task<IActionResult> ValidatePayment([FromBody] CreatePaymentDto createPaymentDto)
        {
            _logger.LogInformation("ValidatePayment endpoint called with reference: {PaymentReference}", createPaymentDto.PaymentReference);
            
            var payment = _mapper.Map<PaymentNotification>(createPaymentDto);
            var validationResult = await _paymentService.ValidatePaymentAsync(payment);
            
            var validationDto = _mapper.Map<PaymentValidationDto>(validationResult);
            return Ok(new ApiResponseDto<PaymentValidationDto>
            {
                Success = true,
                Message = "Payment validation completed",
                Data = validationDto
            });
        }

        // GET api/payments/validate-reference/{paymentReference}
        // Validates if a payment reference exists
        [HttpGet("validate-reference/{paymentReference}")]
        [AuthorizePayment("read")]
        [Authorize(Policy = "PaymentAccess")]
        public async Task<IActionResult> ValidatePaymentReference(string paymentReference)
        {
            _logger.LogInformation("ValidatePaymentReference endpoint called with reference: {PaymentReference}", paymentReference);
            var exists = await _paymentService.PaymentReferenceExistsAsync(paymentReference);
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Payment reference validation completed",
                Data = new { ReferenceExists = exists, PaymentReference = paymentReference }
            });
        }

        // GET api/payments/student/{studentNumber}/total
        // Gets total amount paid by a student
        [HttpGet("student/{studentNumber}/total")]
        [AuthorizePayment("read")]
        [Authorize(Policy = "PaymentAccess")]
        public async Task<IActionResult> GetTotalAmountPaidByStudent(string studentNumber)
        {
            _logger.LogInformation("GetTotalAmountPaidByStudent endpoint called for student: {StudentNumber}", studentNumber);
            var totalAmount = await _paymentService.GetTotalAmountPaidByStudentAsync(studentNumber);
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Total amount retrieved successfully",
                Data = new { StudentNumber = studentNumber, TotalAmount = totalAmount }
            });
        }

        // GET api/payments/student/{studentNumber}/summary
        // Gets payment summary for a student
        [HttpGet("student/{studentNumber}/summary")]
        [AuthorizePayment("read")]
        [Authorize(Policy = "ReportingAccess")]
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
        [AuthorizePayment("create")]
        [Authorize(Roles = "Admin,FinanceManager")]
        public async Task<IActionResult> ProcessBatchPayments([FromBody] BatchPaymentDto batchPaymentDto)
        {
            _logger.LogInformation("ProcessBatchPayments endpoint called with {Count} payments", batchPaymentDto.Payments.Count);
            
            // Convert DTOs to models
            var payments = _mapper.Map<List<PaymentNotification>>(batchPaymentDto.Payments);
            var result = await _paymentService.ProcessBatchPaymentsAsync(payments);
            
            var batchResultDto = _mapper.Map<BatchPaymentResultDto>(result);
            return Ok(new ApiResponseDto<BatchPaymentResultDto>
            {
                Success = true,
                Message = "Batch payment processing completed",
                Data = batchResultDto
            });
        }

        // POST api/payments/reconcile
        // Reconciles payments with bank data
        [HttpPost("reconcile")]
        [AuthorizePayment("update")]
        [Authorize(Roles = "Admin,FinanceManager,Accountant")]
        public async Task<IActionResult> ReconcilePayments([FromBody] List<BankPaymentDataDto> bankData)
        {
            _logger.LogInformation("ReconcilePayments endpoint called with {Count} bank records", bankData.Count);
            
            // Convert DTOs to models
            var bankPaymentData = _mapper.Map<List<BankPaymentData>>(bankData);
            var reconciliationResult = await _paymentService.ReconcilePaymentsAsync(bankPaymentData);
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Payment reconciliation completed",
                Data = new { 
                    TotalRecords = bankData.Count,
                    ReconciledCount = reconciliationResult.TotalReconciled,
                    UnreconciledCount = reconciliationResult.UnmatchedPayments.Count,
                    Errors = reconciliationResult.Discrepancies
                }
            });
        }

        // POST api/payments/test-messaging
        // Tests the messaging system
        [HttpPost("test-messaging")]
        [AuthorizeAdmin]
        public async Task<IActionResult> TestMessaging()
        {
            _logger.LogInformation("TestMessaging endpoint called");
            
            try
            {
                await _paymentService.TestMessagingAsync();
                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Messaging test completed successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during messaging test");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Messaging test failed",
                    Data = null
                });
            }
        }
    }
}    

        
  
