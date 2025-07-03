// Purpose: V1 Payment Controller - Original implementation
using Microsoft.AspNetCore.Mvc;
using xyz_university_payment_api.Core.Application.Services;
using xyz_university_payment_api.Core.Application.Interfaces;
using xyz_university_payment_api.Core.Domain.Entities;
using xyz_university_payment_api.Core.Application.DTOs;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;

namespace xyz_university_payment_api.Presentation.Controllers.V1
{
    [Authorize(Policy = "ApiScope")]
    [ApiController]
    [Route("api/v1/[controller]")]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
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

        // POST api/v1/payments/notify
        [HttpPost("notify")]
        public async Task<IActionResult> NotifyPayment([FromBody] CreatePaymentDto createPaymentDto)
        {
            _logger.LogInformation("V1 NotifyPayment endpoint called with reference: {PaymentReference}", createPaymentDto.PaymentReference);

            var payment = _mapper.Map<PaymentNotification>(createPaymentDto);
            var result = await _paymentService.ProcessPaymentAsync(payment);

            var paymentResponseDto = _mapper.Map<PaymentResponseDto>(result.ProcessedPayment);
            paymentResponseDto.Success = true;
            paymentResponseDto.Message = result.Message;
            paymentResponseDto.StudentExists = result.StudentExists;
            paymentResponseDto.StudentIsActive = result.StudentIsActive;

            return Ok(new ApiResponseDto<PaymentResponseDto>
            {
                Success = true,
                Message = "Payment processed successfully (V1)",
                Data = paymentResponseDto
            });
        }

        // GET api/v1/payments
        [HttpGet]
        public async Task<IActionResult> GetAllPayments([FromQuery] PaginationDto pagination)
        {
            _logger.LogInformation("V1 GetAllPayments endpoint called with page {PageNumber}", pagination.PageNumber);
            var payments = await _paymentService.GetAllPaymentsAsync();

            var paymentDtos = _mapper.Map<List<PaymentDto>>(payments);

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
                Message = "Payments retrieved successfully (V1)",
                Data = pagedResult
            });
        }

        // GET api/v1/payments/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            _logger.LogInformation("V1 GetPaymentById endpoint called with ID: {PaymentId}", id);
            var payment = await _paymentService.GetPaymentByIdAsync(id);

            var paymentDto = _mapper.Map<PaymentDto>(payment);
            return Ok(new ApiResponseDto<PaymentDto>
            {
                Success = true,
                Message = "Payment retrieved successfully (V1)",
                Data = paymentDto
            });
        }

        // GET api/v1/payments/student/{studentNumber}
        [HttpGet("student/{studentNumber}")]
        public async Task<IActionResult> GetPaymentsByStudent(string studentNumber, [FromQuery] PaginationDto pagination)
        {
            _logger.LogInformation("V1 GetPaymentsByStudent endpoint called for student: {StudentNumber}", studentNumber);
            var payments = await _paymentService.GetPaymentsByStudentAsync(studentNumber);

            var paymentDtos = _mapper.Map<List<PaymentDto>>(payments);

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
                Message = "Student payments retrieved successfully (V1)",
                Data = pagedResult
            });
        }

        // GET api/v1/payments/student/{studentNumber}/summary
        [HttpGet("student/{studentNumber}/summary")]
        public async Task<IActionResult> GetPaymentSummary(string studentNumber)
        {
            _logger.LogInformation("V1 GetPaymentSummary endpoint called for student: {StudentNumber}", studentNumber);
            var summary = await _paymentService.GetPaymentSummaryAsync(studentNumber);

            var summaryDto = _mapper.Map<PaymentSummaryDto>(summary);
            return Ok(new ApiResponseDto<PaymentSummaryDto>
            {
                Success = true,
                Message = "Payment summary retrieved successfully (V1)",
                Data = summaryDto
            });
        }
    }
}