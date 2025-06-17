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

        [Fact]
        public async Task ValidatePaymentAsync_Should_Fail_When_AmountIsZero()
        {
            // Arrange
            var payment = new PaymentNotification
            {
                PaymentReference = "REF123456",
                StudentNumber = "STU001",
                AmountPaid = 0, // Invalid amount
                PaymentDate = DateTime.UtcNow
            };

            // Act
            var (isValid, errors) = await _paymentService.ValidatePaymentAsync(payment);

            // Assert
            Assert.False(isValid); // The payment should not be valid
            Assert.Contains("Payment amount must be greater than zero", errors[0]); // The error should mention this issue
        }

        [Fact]
        public async Task ValidatePaymentAsync_Should_Fail_When_DateIsInFuture()
        {
            // Arrange
            var payment = new PaymentNotification
            {
                PaymentReference = "REF123456",
                StudentNumber = "STU001",
                AmountPaid = 5000,
                PaymentDate = DateTime.UtcNow.AddDays(1) // Future date
            };

            // Act
            var (isValid, errors) = await _paymentService.ValidatePaymentAsync(payment);

            // Assert
            Assert.False(isValid); // The payment should not be valid
            Assert.Contains("Payment date cannot be in the future", errors[0]); // The error should mention this issue
        }

        [Fact]
        public async Task ValidatePaymentAsync_Should_Fail_When_StudentNumberIsMissing()
        {
            // Arrange
            var payment = new PaymentNotification
            {
                PaymentReference = "REF123456",
                StudentNumber = "", // Missing student number
                AmountPaid = 5000,
                PaymentDate = DateTime.UtcNow
            };

            // Act
            var (isValid, errors) = await _paymentService.ValidatePaymentAsync(payment);

            // Assert
            Assert.False(isValid); // The payment should not be valid
            Assert.Contains("Student number is required", errors[0]); // The error should mention this issue
        }
        [Fact]
        public async Task ProcessBatchPaymentsAsync_Should_Process_All_Successful_Payments()
        {
            // Arrange
            var payments = new List<PaymentNotification>
            {
                new PaymentNotification
                {
                    PaymentReference = "REF123456",
                    StudentNumber = "STU001",
                    AmountPaid = 5000,
                    PaymentDate = DateTime.UtcNow
                },
                new PaymentNotification
                {
                    PaymentReference = "REF654321",
                    StudentNumber = "STU002",
                    AmountPaid = 7000,
                    PaymentDate = DateTime.UtcNow
                }
            };

            _paymentRepositoryMock.Setup(repo => repo.PaymentReferenceExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _studentRepositoryMock.Setup(repo => repo.GetByStudentNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Student
                {
                    StudentNumber = "STU001",
                    FullName = "Test Student",
                    Program = "Test Program",
                    IsActive = true
                });

            _paymentRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<PaymentNotification>()))
                .ReturnsAsync((PaymentNotification p) => p);

            // Act
            var result = await _paymentService.ProcessBatchPaymentsAsync(payments);

            // Assert
            Assert.Equal(2, result.Successful);
            Assert.Equal(2, result.TotalProcessed);
            Assert.Equal(0, result.Failed);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task ProcessBatchPaymentsAsync_Should_Handle_Failed_Payments()
        {
            // Arrange
            var payments = new List<PaymentNotification>
            {
                new PaymentNotification
                {
                    PaymentReference = "REF123456",
                    StudentNumber = "STU001",
                    AmountPaid = 5000,
                    PaymentDate = DateTime.UtcNow
                },
                new PaymentNotification
                {
                    PaymentReference = "INVALID",
                    StudentNumber = "STU002",
                    AmountPaid = 0, // Invalid amount
                    PaymentDate = DateTime.UtcNow
                }
            };

            _paymentRepositoryMock.Setup(repo => repo.PaymentReferenceExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _studentRepositoryMock.Setup(repo => repo.GetByStudentNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(new Student
                {
                    StudentNumber = "STU001",
                    FullName = "Test Student",
                    Program = "Test Program",
                    IsActive = true
                });

            _paymentRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<PaymentNotification>()))
                .ReturnsAsync((PaymentNotification p) => p);

            // Act
            var result = await _paymentService.ProcessBatchPaymentsAsync(payments);

            // Assert
            Assert.Equal(1, result.Successful); // One succeeded
            Assert.Equal(1, result.Failed);     // One failed
            Assert.Equal(2, result.TotalProcessed); // Total processed
            Assert.NotEmpty(result.Errors); // Errors should be listed
        }



    }
}
