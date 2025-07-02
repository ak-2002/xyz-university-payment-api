using Microsoft.AspNetCore.Mvc;
using xyz_university_payment_api.Core.Application.Interfaces;

namespace xyz_university_payment_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet("{studentNumber}")]
        public async Task<IActionResult> GetStudentByNumber(string studentNumber)
        {
            var student = await _studentService.GetStudentByNumberAsync(studentNumber);
            if (student == null)
            {
                return NotFound($"Student with number {studentNumber} not found.");
            }

            return Ok(student);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _studentService.GetAllStudentsAsync();
            return Ok(students);
        }
    }
}
