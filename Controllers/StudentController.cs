// Purpose: Handles student validation requests from Family Bank
using Microsoft.AspNetCore.Mvc;
using xyz_university_payment_api.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace xyz_university_payment_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

   

    public class StudentController : ControllerBase
    {
        private readonly StudentService _studentService;
        private readonly ILogger<StudentController> _logger;

        public StudentController(StudentService studentService, ILogger<StudentController> logger)
        {
            _studentService = studentService;
            _logger = logger;
        }

        // POST api/student/validate
        // Validates if a student number provided by Family Bank is active
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateStudent([FromBody] StudentValidationRequest request)
        {
            _logger.LogInformation("ValidateStudent endpoint called with student number: {StudentNumber}", request.StudentNumber);

            var student = await _studentService.ValidateStudentAsync(request.StudentNumber);
            if (student == null)
            {

                _logger.LogWarning("Student not found: {StudentNumber}", request.StudentNumber);
                return NotFound(new { isValid = false, message = "Student not found." });
            }
            
            _logger.LogInformation("Student Validated:{StudentNumber}", request.StudentNumber);    
            return Ok(new { isValid = true, studentName = student.FullName, program = student.Program });
            
            
        }
    }

    // Request structure for student validation
    public class StudentValidationRequest
    {
        public string StudentNumber { get; set; }
    }
}