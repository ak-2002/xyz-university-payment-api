using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Make sure this is added
using Moq;
using xyz_university_payment_api.Controllers;
using xyz_university_payment_api.Interfaces;
using xyz_university_payment_api.Models;
using Xunit;

namespace xyz_university_payment_api.Tests.Controllers
{
    public class PaymentControllerTests
    {
        private readonly Mock<IPaymentService> _paymentServiceMock;
        private readonly Mock<ILogger<PaymentController>> _loggerMock; // ✅ Add this
        private readonly PaymentController _paymentController;

        public PaymentControllerTests()
        {
            _paymentServiceMock = new Mock<IPaymentService>();
            _loggerMock = new Mock<ILogger<PaymentController>>(); // ✅ Add this
            _paymentController = new PaymentController(_paymentServiceMock.Object, _loggerMock.Object); // ✅ Fix this
        }

        [Fact]
        public async Task ProcessPayment_ShouldReturnOk_WhenPaymentIsSuccessful()
        {
            // Arrange
            var payment = new PaymentNotification { PaymentReference = "REF123", StudentNumber = "S001", AmountPaid = 5000 };

            _paymentServiceMock.Setup(service => service.ProcessPaymentAsync(payment))
                .ReturnsAsync(new PaymentProcessingResult
                {
                    Success = true,
                    StudentExists = true,
                    StudentIsActive = true,
                    ProcessedPayment = payment
                });

            // Act
            var result = await _paymentController.NotifyPayment(payment);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task ProcessPayment_ShouldReturnBadRequest_WhenPaymentFails()
        {
            // Arrange
            var payment = new PaymentNotification { PaymentReference = "REF123", StudentNumber = "S001", AmountPaid = 5000 };

            _paymentServiceMock.Setup(service => service.ProcessPaymentAsync(payment))
                .ReturnsAsync(new PaymentProcessingResult
                {
                    Success = false,
                    StudentExists = true,
                    StudentIsActive = true,
                    ProcessedPayment = null,
                    Message = "Duplicate payment reference"
                });

            // Act
            var result = await _paymentController.NotifyPayment(payment);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
    }
}
