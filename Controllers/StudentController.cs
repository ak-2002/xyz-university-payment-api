// Purpose: Handles student validation requests from Family Bank
using Microsoft.AspNetCore.Mvc;
using xyz_university_payment_api.Services;
using System.Threading.Tasks;

namespace xyz_university_payment_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly StudentService _studentService;

        public StudentController(StudentService studentService)
        {
            _studentService = studentService;
        }

        // POST api/student/validate
        // Validates if a student number provided by Family Bank is active
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateStudent([FromBody] StudentValidationRequest request)
        {
            var student = await _studentService.ValidateStudentAsync(request.StudentNumber);
            if (student == null)
                return NotFound(new { isValid = false, message = "Student not found." });

            return Ok(new { isValid = true, studentName = student.FullName, program = student.Program });
        }
    }

    // Request structure for student validation
    public class StudentValidationRequest
    {
        public string StudentNumber { get; set; }
    }
}