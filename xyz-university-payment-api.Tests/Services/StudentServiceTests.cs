using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xyz_university_payment_api.Interfaces;
using xyz_university_payment_api.Models;
using xyz_university_payment_api.Services;
using Xunit;

namespace xyz_university_payment_api.Tests.Services
{
    public class StudentServiceTests
    {
        [Fact]
        public async Task GetAllStudentsAsync_ShouldReturnAllStudents()
        {
            // Arrange
            var studentRepositoryMock = new Mock<IStudentRepository>();
            var loggerMock = new Mock<ILogger<StudentService>>();

            var students = new List<Student>
            {
                new Student { Id = 1, FullName = "John Doe", StudentNumber = "S12345", Program = "CS", IsActive = true }
            };

            studentRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(students);

            var studentService = new StudentService(studentRepositoryMock.Object, loggerMock.Object);

            // Act
            var result = await studentService.GetAllStudentsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().FullName.Should().Be("John Doe");
        }
    }
}
