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


        [Fact]
public async Task GetStudentByNumberAsync_ShouldReturnStudent_WhenStudentExists()
{
    // Arrange
    var studentRepositoryMock = new Mock<IStudentRepository>();
    var loggerMock = new Mock<ILogger<StudentService>>();

    var student = new Student
    {
        Id = 1,
        FullName = "Jane Doe",
        StudentNumber = "S54321",
        Program = "IT",
        IsActive = true
    };

    studentRepositoryMock.Setup(repo => repo.GetByStudentNumberAsync("S54321"))
        .ReturnsAsync(student);

    var studentService = new StudentService(studentRepositoryMock.Object, loggerMock.Object);

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
    var studentRepositoryMock = new Mock<IStudentRepository>();
    var loggerMock = new Mock<ILogger<StudentService>>();

    studentRepositoryMock.Setup(repo => repo.GetByStudentNumberAsync("S00000"))
        .ReturnsAsync((Student)null);

    var studentService = new StudentService(studentRepositoryMock.Object, loggerMock.Object);

    // Act
    var result = await studentService.GetStudentByNumberAsync("S00000");

    // Assert
    result.Should().BeNull();
}
[Fact]
public async Task GetStudentByNumberAsync_ShouldReturnInactiveStudent_WhenStudentIsInactive()
{
    // Arrange
    var studentRepositoryMock = new Mock<IStudentRepository>();
    var loggerMock = new Mock<ILogger<StudentService>>();

    var student = new Student
    {
        Id = 2,
        FullName = "Inactive Student",
        StudentNumber = "S99999",
        Program = "Business",
        IsActive = false
    };

    studentRepositoryMock.Setup(repo => repo.GetByStudentNumberAsync("S99999"))
        .ReturnsAsync(student);

    var studentService = new StudentService(studentRepositoryMock.Object, loggerMock.Object);

    // Act
    var result = await studentService.GetStudentByNumberAsync("S99999");

    // Assert
    result.Should().NotBeNull();
    result!.StudentNumber.Should().Be("S99999");
    result.IsActive.Should().BeFalse();
}


    }
}
