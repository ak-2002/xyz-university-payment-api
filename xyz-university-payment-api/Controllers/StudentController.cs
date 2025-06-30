// Purpose: Handles student operations with comprehensive CRUD functionality
using Microsoft.AspNetCore.Mvc;
using xyz_university_payment_api.Services;
using xyz_university_payment_api.Interfaces;
using xyz_university_payment_api.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using xyz_university_payment_api.Attributes;
using xyz_university_payment_api.DTOs;

namespace xyz_university_payment_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all student endpoints
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly ILogger<StudentController> _logger;

        public StudentController(IStudentService studentService, ILogger<StudentController> logger)
        {
            _studentService = studentService;
            _logger = logger;
        }

        // GET api/students
        // Retrieves all students
        [HttpGet]
        [AuthorizeStudent("read")]
        [Authorize(Policy = "StudentAccess")]
        public async Task<IActionResult> GetAllStudents()
        {
            _logger.LogInformation("GetAllStudents endpoint called");
            var students = await _studentService.GetAllStudentsAsync();
            return Ok(students);
        }

        // GET api/students/{id}
        // Retrieves a student by ID
        [HttpGet("{id}")]
        [AuthorizeStudent("read")]
        [Authorize(Policy = "StudentAccess")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            _logger.LogInformation("GetStudentById endpoint called with ID: {StudentId}", id);
            var student = await _studentService.GetStudentByIdAsync(id);
            
            if (student == null)
            {
                _logger.LogWarning("Student not found with ID: {StudentId}", id);
                return NotFound(new { message = "Student not found" });
            }
            
            return Ok(student);
        }

        // GET api/students/number/{studentNumber}
        // Retrieves a student by student number
        [HttpGet("number/{studentNumber}")]
        [AuthorizeStudent("read")]
        [Authorize(Policy = "StudentAccess")]
        public async Task<IActionResult> GetStudentByNumber(string studentNumber)
        {
            _logger.LogInformation("GetStudentByNumber endpoint called with number: {StudentNumber}", studentNumber);
            var student = await _studentService.GetStudentByNumberAsync(studentNumber);
            
            if (student == null)
            {
                _logger.LogWarning("Student not found with number: {StudentNumber}", studentNumber);
                return NotFound(new { message = "Student not found" });
            }
            
            return Ok(student);
        }

        // POST api/students
        // Creates a new student
        [HttpPost]
        [AuthorizeStudent("create")]
        [Authorize(Roles = "Admin,UserManager")]
        public async Task<IActionResult> CreateStudent([FromBody] Student student)
        {
            _logger.LogInformation("CreateStudent endpoint called for student: {StudentNumber}", student.StudentNumber);
            
            try
            {
                var createdStudent = await _studentService.CreateStudentAsync(student);
                return CreatedAtAction(nameof(GetStudentById), new { id = createdStudent.Id }, createdStudent);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Student creation failed: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT api/students/{id}
        // Updates an existing student
        [HttpPut("{id}")]
        [AuthorizeStudent("update")]
        [Authorize(Roles = "Admin,UserManager")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] Student student)
        {
            _logger.LogInformation("UpdateStudent endpoint called for student ID: {StudentId}", id);
            
            if (id != student.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }
            
            try
            {
                var updatedStudent = await _studentService.UpdateStudentAsync(student);
                return Ok(updatedStudent);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Student update failed: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE api/students/{id}
        // Deletes a student
        [HttpDelete("{id}")]
        [AuthorizeStudent("delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            _logger.LogInformation("DeleteStudent endpoint called for student ID: {StudentId}", id);
            
            try
            {
                var result = await _studentService.DeleteStudentAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Student not found" });
                }
                
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Student deletion failed: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET api/students/active
        // Retrieves all active students
        [HttpGet("active")]
        [AuthorizeStudent("read")]
        [Authorize(Policy = "StudentAccess")]
        public async Task<IActionResult> GetActiveStudents()
        {
            _logger.LogInformation("GetActiveStudents endpoint called");
            var students = await _studentService.GetActiveStudentsAsync();
            return Ok(students);
        }

        // GET api/students/program/{program}
        // Retrieves students by program
        [HttpGet("program/{program}")]
        [AuthorizeStudent("read")]
        [Authorize(Policy = "StudentAccess")]
        public async Task<IActionResult> GetStudentsByProgram(string program)
        {
            _logger.LogInformation("GetStudentsByProgram endpoint called for program: {Program}", program);
            var students = await _studentService.GetStudentsByProgramAsync(program);
            return Ok(students);
        }

        // GET api/students/search/{searchTerm}
        // Searches students by name
        [HttpGet("search/{searchTerm}")]
        [AuthorizeStudent("read")]
        [Authorize(Policy = "StudentAccess")]
        public async Task<IActionResult> SearchStudents(string searchTerm)
        {
            _logger.LogInformation("SearchStudents endpoint called with term: {SearchTerm}", searchTerm);
            var students = await _studentService.SearchStudentsAsync(searchTerm);
            return Ok(students);
        }

        // PUT api/students/{id}/status
        // Updates student active status
        [HttpPut("{id}/status")]
        [AuthorizeStudent("update")]
        [Authorize(Roles = "Admin,UserManager")]
        public async Task<IActionResult> UpdateStudentStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            _logger.LogInformation("UpdateStudentStatus endpoint called for student ID: {StudentId}", id);
            
            try
            {
                var updatedStudent = await _studentService.UpdateStudentStatusAsync(id, request.IsActive);
                return Ok(updatedStudent);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Student status update failed: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST api/students/validate
        // Validates student data
        [HttpPost("validate")]
        [AuthorizeStudent("read")]
        [Authorize(Policy = "StudentAccess")]
        public async Task<IActionResult> ValidateStudent([FromBody] Student student)
        {
            _logger.LogInformation("ValidateStudent endpoint called for student: {StudentNumber}", student.StudentNumber);
            
            var validation = await _studentService.ValidateStudentAsync(student);
            return Ok(new { isValid = validation.IsValid, errors = validation.Errors });
        }

        // GET api/students/{studentNumber}/eligible
        // Checks if student is eligible for enrollment
        [HttpGet("{studentNumber}/eligible")]
        [AuthorizeStudent("read")]
        [Authorize(Policy = "StudentAccess")]
        public async Task<IActionResult> CheckEnrollmentEligibility(string studentNumber)
        {
            _logger.LogInformation("CheckEnrollmentEligibility endpoint called for student: {StudentNumber}", studentNumber);
            
            var eligibility = await _studentService.CheckEnrollmentEligibilityAsync(studentNumber);
            return Ok(new { 
                studentNumber = studentNumber, 
                isEligible = eligibility.IsEligible, 
                reasons = eligibility.Reasons 
            });
        }
    }
}
