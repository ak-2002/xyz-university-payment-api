using Moq;
using Xunit;
using xyz_university_payment_api.Services;
using xyz_university_payment_api.Interfaces;
using xyz_university_payment_api.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace xyz_university_payment_api.Tests.Services
{
    public class PaymentServiceTests
    {
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
        private readonly Mock<IStudentRepository> _studentRepositoryMock;
        private readonly Mock<ILogger<PaymentService>> _loggerMock;
        private readonly PaymentService _paymentService;

        public PaymentServiceTests()
        {
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _studentRepositoryMock = new Mock<IStudentRepository>();
            _loggerMock = new Mock<ILogger<PaymentService>>();

            _paymentService = new PaymentService(
                _paymentRepositoryMock.Object,
                _studentRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task ProcessPaymentAsync_Should_Return_Success_When_Valid()
        {
            // Arrange
            var payment = new PaymentNotification
            {
                PaymentReference = "REF123456",
                StudentNumber = "STU001",
                AmountPaid = 5000,
                PaymentDate = DateTime.UtcNow
            };

            var student = new Student 
            {
                StudentNumber = "STU001", 
                FullName = "Test Student",  // Required
                Program = "Computer Science",  // Required
                IsActive = true 
             };

            _paymentRepositoryMock.Setup(repo => repo.PaymentReferenceExistsAsync(payment.PaymentReference))
                .ReturnsAsync(false);
            _studentRepositoryMock.Setup(repo => repo.GetByStudentNumberAsync(payment.StudentNumber))
                .ReturnsAsync(student);
            _paymentRepositoryMock.Setup(repo => repo.AddAsync(payment))
                .ReturnsAsync(payment);

            // Act
            var result = await _paymentService.ProcessPaymentAsync(payment);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.StudentExists);
            Assert.True(result.StudentIsActive);
            Assert.NotNull(result.ProcessedPayment);
        }

        [Fact]
        public async Task ProcessPaymentAsync_Should_Fail_When_DuplicateReference()
        {
            // Arrange
            var payment = new PaymentNotification
            {
                PaymentReference = "REF123456",
                StudentNumber = "STU001",
                AmountPaid = 5000,
                PaymentDate = DateTime.UtcNow
            };

            _paymentRepositoryMock.Setup(repo => repo.PaymentReferenceExistsAsync(payment.PaymentReference))
                .ReturnsAsync(true);

            // Act
            var result = await _paymentService.ProcessPaymentAsync(payment);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("already exists", result.Message);
        }

        [Fact]
        public async Task ProcessPaymentAsync_Should_Fail_When_StudentNotFound()
        {
            // Arrange
            var payment = new PaymentNotification
            {
                PaymentReference = "REF123456",
                StudentNumber = "STU001",
                AmountPaid = 5000,
                PaymentDate = DateTime.UtcNow
            };

            _paymentRepositoryMock.Setup(repo => repo.PaymentReferenceExistsAsync(payment.PaymentReference))
                .ReturnsAsync(false);
            _studentRepositoryMock.Setup(repo => repo.GetByStudentNumberAsync(payment.StudentNumber))
                .ReturnsAsync((Student?)null);

            // Act
            var result = await _paymentService.ProcessPaymentAsync(payment);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("student not found", result.Message);
        }

        [Fact]
        public async Task ValidatePaymentAsync_Should_Fail_When_InvalidReference()
        {
            // Arrange
            var payment = new PaymentNotification
            {
                PaymentReference = "123",
                StudentNumber = "STU001",
                AmountPaid = 5000,
                PaymentDate = DateTime.UtcNow
            };

            // Act
            var (isValid, errors) = await _paymentService.ValidatePaymentAsync(payment);

            // Assert
            Assert.False(isValid);
            Assert.Contains("Payment reference must start with 'REF'", errors[0]);
        }
    }
}
