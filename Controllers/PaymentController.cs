// Purpose: Handles payment notification requests from Family Bank
using Microsoft.AspNetCore.Mvc;
using xyz_university_payment_api.Services;
using xyz_university_payment_api.Models;
using System.Threading.Tasks;

namespace xyz_university_payment_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // POST api/payment/notify
        // Receives and processes payment notifications from Family Bank
        [HttpPost("notify")]
        public async Task<IActionResult> NotifyPayment([FromBody] PaymentNotification payment)
        {
            var result = await _paymentService.ProcessPaymentAsync(payment);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message,
                    studentExists = result.StudentExists,
                    studentIsActive = result.StudentIsActive
                });
            }

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
            var payments = _paymentService.GetAllPayments();
            return Ok(payments);
        }
    }
}
