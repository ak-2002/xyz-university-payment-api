using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using xyz_university_payment_api.Core.Application.Interfaces;
using xyz_university_payment_api.Core.Domain.Entities;
using xyz_university_payment_api.Core.Application.Services;
using Xunit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.Http;

namespace xyz_university_payment_api.Tests.Services
{
    public class StudentServiceTests
    {
        [Fact]
        public async Task GetStudentsAsync_ShouldReturnAllStudents()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<StudentService>>();
            var mockCacheService = new Mock<ICacheService>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            var students = new List<Student>
            {
                new Student { Id = 1, StudentNumber = "S12345", FullName = "John Doe", Program = "CS", IsActive = true },
                new Student { Id = 2, StudentNumber = "S67890", FullName = "Jane Smith", Program = "EE", IsActive = true }
            };

            mockUnitOfWork.Setup(uow => uow.Students.GetAllAsync()).ReturnsAsync(students);

            var studentService = new StudentService(mockUnitOfWork.Object, mockLogger.Object, mockCacheService.Object, mockHttpContextAccessor.Object);

            // Act
            var result = await studentService.GetAllStudentsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().FullName.Should().Be("John Doe");
        }

        [Fact]
        public async Task GetStudentByIdAsync_ShouldReturnStudent()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<StudentService>>();
            var mockCacheService = new Mock<ICacheService>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            var student = new Student { Id = 1, StudentNumber = "S12345", FullName = "John Doe", Program = "CS", IsActive = true };

            mockUnitOfWork.Setup(uow => uow.Students.GetByIdAsync(1)).ReturnsAsync(student);

            var studentService = new StudentService(mockUnitOfWork.Object, mockLogger.Object, mockCacheService.Object, mockHttpContextAccessor.Object);

            // Act
            var result = await studentService.GetStudentByNumberAsync("S54321");

            // Assert
            result.Should().NotBeNull();
            result.StudentNumber.Should().Be("S54321");
            result.FullName.Should().Be("Jane Doe");
        }

        [Fact]
        public async Task GetStudentByNumberAsync_ShouldReturnNull_WhenStudentDoesNotExist()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<StudentService>>();
            var mockCacheService = new Mock<ICacheService>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            mockUnitOfWork.Setup(uow => uow.Students.GetAllAsync()).ReturnsAsync(new List<Student>());

            var studentService = new StudentService(mockUnitOfWork.Object, mockLogger.Object, mockCacheService.Object, mockHttpContextAccessor.Object);

            // Act
            var result = await studentService.GetStudentByNumberAsync("S00000");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetStudentByNumberAsync_ShouldReturnInactiveStudent_WhenStudentIsInactive()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<StudentService>>();
            var mockCacheService = new Mock<ICacheService>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            var student = new Student
            {
                Id = 2,
                FullName = "Inactive Student",
                StudentNumber = "S99999",
                Program = "Business",
                IsActive = false
            };

            mockUnitOfWork.Setup(uow => uow.Students.GetAllAsync()).ReturnsAsync(new List<Student> { student });

            var studentService = new StudentService(mockUnitOfWork.Object, mockLogger.Object, mockCacheService.Object, mockHttpContextAccessor.Object);

            // Act
            var result = await studentService.GetStudentByNumberAsync("S99999");

            // Assert
            result.Should().NotBeNull();
            result!.StudentNumber.Should().Be("S99999");
            result.IsActive.Should().BeFalse();
        }
    }
}
