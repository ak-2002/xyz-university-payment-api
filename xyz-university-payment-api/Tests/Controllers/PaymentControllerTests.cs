using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using xyz_university_payment_api.Presentation.Controllers;
using xyz_university_payment_api.Core.Application.Interfaces;
using xyz_university_payment_api.Core.Domain.Entities;
using xyz_university_payment_api.Core.Application.DTOs;
using AutoMapper;
using Xunit;

namespace xyz_university_payment_api.Tests.Controllers
{
    public class PaymentControllerTests
    {
        private readonly Mock<IPaymentService> _paymentServiceMock;
        private readonly Mock<ILogger<PaymentControllerV1>> _loggerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly PaymentControllerV1 _paymentController;

        public PaymentControllerTests()
        {
            _paymentServiceMock = new Mock<IPaymentService>();
            _loggerMock = new Mock<ILogger<PaymentControllerV1>>();
            _mapperMock = new Mock<IMapper>();
            _paymentController = new PaymentControllerV1(_paymentServiceMock.Object, _loggerMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task ProcessPayment_ShouldReturnOk_WhenPaymentIsSuccessful()
        {
            // Arrange
            var createPaymentDto = new CreatePaymentDto 
            { 
                PaymentReference = "REF123", 
                StudentNumber = "S001", 
                AmountPaid = 5000,
                PaymentDate = DateTime.UtcNow
            };

            var payment = new PaymentNotification 
            { 
                PaymentReference = "REF123", 
                StudentNumber = "S001", 
                AmountPaid = 5000,
                PaymentDate = DateTime.UtcNow
            };

            _mapperMock.Setup(m => m.Map<PaymentNotification>(createPaymentDto))
                .Returns(payment);

            _paymentServiceMock.Setup(service => service.ProcessPaymentAsync(payment))
                .ReturnsAsync(new PaymentProcessingResult
                {
                    Success = true,
                    StudentExists = true,
                    StudentIsActive = true,
                    ProcessedPayment = payment,
                    Message = "Payment processed successfully"
                });

            // Act
            var result = await _paymentController.NotifyPayment(createPaymentDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task ProcessPayment_ShouldReturnBadRequest_WhenPaymentFails()
        {
            // Arrange
            var createPaymentDto = new CreatePaymentDto 
            { 
                PaymentReference = "REF123", 
                StudentNumber = "S001", 
                AmountPaid = 5000,
                PaymentDate = DateTime.UtcNow
            };

            var payment = new PaymentNotification 
            { 
                PaymentReference = "REF123", 
                StudentNumber = "S001", 
                AmountPaid = 5000,
                PaymentDate = DateTime.UtcNow
            };

            _mapperMock.Setup(m => m.Map<PaymentNotification>(createPaymentDto))
                .Returns(payment);

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
            var result = await _paymentController.NotifyPayment(createPaymentDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
    }
}
