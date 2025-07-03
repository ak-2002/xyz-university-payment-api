// Purpose: V2 Payment Controller - Enhanced implementation
using Microsoft.AspNetCore.Mvc;
using xyz_university_payment_api.Core.Application.Services;
using xyz_university_payment_api.Core.Application.Interfaces;
using xyz_university_payment_api.Core.Domain.Entities;
using xyz_university_payment_api.Core.Application.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;

namespace xyz_university_payment_api.Presentation.Controllers.V2
{
    [Authorize(Policy = "ApiScope")]
    [ApiController]
    [Route("api/v2/[controller]")]
    [ApiVersion("2.0")]
    [ApiExplorerSettings(GroupName = "v2")]
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

        // POST api/v2/payments/notify
        [HttpPost("notify")]
        public async Task<IActionResult> NotifyPayment([FromBody] CreatePaymentDto createPaymentDto)
        {
            _logger.LogInformation("V2 NotifyPayment endpoint called with reference: {PaymentReference}", createPaymentDto.PaymentReference);

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
                Message = "Payment processed successfully (V2)",
                Data = paymentResponseDto
            });
        }

        // GET api/v2/payments
        [HttpGet]
        public async Task<IActionResult> GetAllPayments([FromQuery] PaginationDto pagination)
        {
            _logger.LogInformation("V2 GetAllPayments endpoint called with page {PageNumber}", pagination.PageNumber);
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
                Message = "Payments retrieved successfully (V2)",
                Data = pagedResult
            });
        }

        // NEW V2 ENDPOINT: GET api/v2/payments/analytics
        [HttpGet("analytics")]
        public async Task<IActionResult> GetPaymentAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            _logger.LogInformation("V2 GetPaymentAnalytics endpoint called");

            var payments = await _paymentService.GetAllPaymentsAsync();
            var filteredPayments = payments.AsEnumerable();

            if (startDate.HasValue)
                filteredPayments = filteredPayments.Where(p => p.PaymentDate >= startDate.Value);

            if (endDate.HasValue)
                filteredPayments = filteredPayments.Where(p => p.PaymentDate <= endDate.Value);

            var analytics = new
            {
                TotalPayments = filteredPayments.Count(),
                TotalAmount = filteredPayments.Sum(p => p.AmountPaid),
                AverageAmount = filteredPayments.Any() ? filteredPayments.Average(p => p.AmountPaid) : 0,
                DateRange = new { StartDate = startDate, EndDate = endDate }
            };

            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Payment analytics retrieved successfully (V2)",
                Data = analytics
            });
        }
    }
}