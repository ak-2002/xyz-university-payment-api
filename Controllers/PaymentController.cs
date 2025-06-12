// Purpose: Handles payment notification requests from Family Bank
using Microsoft.AspNetCore.Mvc;
using xyz_university_payment_api.Services;
using xyz_university_payment_api.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace xyz_university_payment_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;


        public PaymentController(PaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        // POST api/payment/notify
        // Receives and processes payment notifications from Family Bank
        [HttpPost("notify")]
        public async Task<IActionResult> NotifyPayment([FromBody] PaymentNotification payment)
        {
            _logger.LogInformation("NotifyPayment endpoint called with refference: {PaymentReference}", payment.PaymentReference);
            var result = await _paymentService.ProcessPaymentAsync(payment);

            if (!result.Success)
            {
                _logger.LogWarning("Payment processed failed: {message}", result.Message);
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
                studentIsActive = result.StudentIsActive
            });
        }

        // GET api/payment
        // Retrieves all records of payments
        [HttpGet]
        public IActionResult GetAllPayments()
        {
             _logger.LogInformation("GetAllPayments endpoint called.");
            var payments = _paymentService.GetAllPayments();
            return Ok(payments);
        }
    }
}
