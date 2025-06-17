using Microsoft.AspNetCore.Mvc;
using xyz_university_payment_api.Interfaces;
using xyz_university_payment_api.Models;

namespace xyz_university_payment_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentNotification payment)
        {
            var result = await _paymentService.ProcessPaymentAsync(payment);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return Ok(payments);
        }

        [HttpGet("student/{studentNumber}")]
        public async Task<IActionResult> GetPaymentsByStudent(string studentNumber)
        {
            var payments = await _paymentService.GetPaymentsByStudentAsync(studentNumber);
            return Ok(payments);
        }

        [HttpGet("{paymentReference}")]
        public async Task<IActionResult> GetPaymentByReference(string paymentReference)
        {
            var payment = await _paymentService.GetPaymentByReferenceAsync(paymentReference);
            if (payment == null)
            {
                return NotFound($"Payment with reference {paymentReference} not found.");
            }

            return Ok(payment);
        }

        [HttpGet("summary/{studentNumber}")]
        public async Task<IActionResult> GetPaymentSummary(string studentNumber)
        {
            var summary = await _paymentService.GetPaymentSummaryAsync(studentNumber);
            return Ok(summary);
        }
    }
}
