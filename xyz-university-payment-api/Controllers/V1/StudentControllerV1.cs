// Purpose: V1 Student Controller - Original implementation
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using xyz_university_payment_api.Models;
using xyz_university_payment_api.Services;
using xyz_university_payment_api.Interfaces;
using xyz_university_payment_api.DTOs;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace xyz_university_payment_api.Controllers.V1
{
    [Authorize(Policy = "ApiScope")]
    [ApiController]
    [Route("api/v1/[controller]")]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly ILogger<StudentController> _logger;
        private readonly IMapper _mapper;

        public StudentController(IStudentService studentService, ILogger<StudentController> logger, IMapper mapper)
        {
            _studentService = studentService;
            _logger = logger;
            _mapper = mapper;
        }

        // GET api/v1/students
        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
        {
            _logger.LogInformation("V1 GetAllStudents endpoint called");
            var students = await _studentService.GetAllStudentsAsync();
            var studentDtos = _mapper.Map<List<StudentDto>>(students);
            
            return Ok(new ApiResponseDto<List<StudentDto>>
            {
                Success = true,
                Message = "Students retrieved successfully (V1)",
                Data = studentDtos
            });
        }

        // GET api/v1/students/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            _logger.LogInformation("V1 GetStudentById endpoint called with ID: {StudentId}", id);
            var student = await _studentService.GetStudentByIdAsync(id);
            
            var studentDto = _mapper.Map<StudentDto>(student);
            return Ok(new ApiResponseDto<StudentDto>
            {
                Success = true,
                Message = "Student retrieved successfully (V1)",
                Data = studentDto
            });
        }

        // GET api/v1/students/number/{studentNumber}
        [HttpGet("number/{studentNumber}")]
        public async Task<IActionResult> GetStudentByNumber(string studentNumber)
        {
            _logger.LogInformation("V1 GetStudentByNumber endpoint called with number: {StudentNumber}", studentNumber);
            var student = await _studentService.GetStudentByNumberAsync(studentNumber);
            
            var studentDto = _mapper.Map<StudentDto>(student);
            return Ok(new ApiResponseDto<StudentDto>
            {
                Success = true,
                Message = "Student retrieved successfully (V1)",
                Data = studentDto
            });
        }

        // POST api/v1/students
        [HttpPost]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDto createStudentDto)
        {
            _logger.LogInformation("V1 CreateStudent endpoint called for student: {StudentNumber}", createStudentDto.StudentNumber);
            
            var student = _mapper.Map<Student>(createStudentDto);
            var createdStudent = await _studentService.CreateStudentAsync(student);
            var studentDto = _mapper.Map<StudentDto>(createdStudent);
            
            return CreatedAtAction(nameof(GetStudentById), new { id = createdStudent.Id }, new ApiResponseDto<StudentDto>
            {
                Success = true,
                Message = "Student created successfully (V1)",
                Data = studentDto
            });
        }

        // PUT api/v1/students/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentDto updateStudentDto)
        {
            _logger.LogInformation("V1 UpdateStudent endpoint called for student ID: {StudentId}", id);
            
            var student = _mapper.Map<Student>(updateStudentDto);
            student.Id = id;
            
            var updatedStudent = await _studentService.UpdateStudentAsync(student);
            var studentDto = _mapper.Map<StudentDto>(updatedStudent);
            
            return Ok(new ApiResponseDto<StudentDto>
            {
                Success = true,
                Message = "Student updated successfully (V1)",
                Data = studentDto
            });
        }

        // DELETE api/v1/students/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            _logger.LogInformation("V1 DeleteStudent endpoint called for student ID: {StudentId}", id);
            
            var result = await _studentService.DeleteStudentAsync(id);
            
            return Ok(new ApiResponseDto<object>
            {
                Success = result,
                Message = result ? "Student deleted successfully (V1)" : "Student not found (V1)",
                Data = new { Deleted = result }
            });
        }

        // GET api/v1/students/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveStudents()
        {
            _logger.LogInformation("V1 GetActiveStudents endpoint called");
            var students = await _studentService.GetActiveStudentsAsync();
            var studentDtos = _mapper.Map<List<StudentDto>>(students);
            
            return Ok(new ApiResponseDto<List<StudentDto>>
            {
                Success = true,
                Message = "Active students retrieved successfully (V1)",
                Data = studentDtos
            });
        }

        // GET api/v1/students/program/{program}
        [HttpGet("program/{program}")]
        public async Task<IActionResult> GetStudentsByProgram(string program)
        {
            _logger.LogInformation("V1 GetStudentsByProgram endpoint called for program: {Program}", program);
            var students = await _studentService.GetStudentsByProgramAsync(program);
            var studentDtos = _mapper.Map<List<StudentDto>>(students);
            
            return Ok(new ApiResponseDto<List<StudentDto>>
            {
                Success = true,
                Message = $"Students in {program} program retrieved successfully (V1)",
                Data = studentDtos
            });
        }

        // GET api/v1/students/search/{searchTerm}
        [HttpGet("search/{searchTerm}")]
        public async Task<IActionResult> SearchStudents(string searchTerm)
        {
            _logger.LogInformation("V1 SearchStudents endpoint called with term: {SearchTerm}", searchTerm);
            var students = await _studentService.SearchStudentsAsync(searchTerm);
            var studentDtos = _mapper.Map<List<StudentDto>>(students);
            
            return Ok(new ApiResponseDto<List<StudentDto>>
            {
                Success = true,
                Message = $"Search results for '{searchTerm}' (V1)",
                Data = studentDtos
            });
        }

        // PUT api/v1/students/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStudentStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            _logger.LogInformation("V1 UpdateStudentStatus endpoint called for student ID: {StudentId}", id);
            
            var updatedStudent = await _studentService.UpdateStudentStatusAsync(id, request.IsActive);
            var studentDto = _mapper.Map<StudentDto>(updatedStudent);
            
            return Ok(new ApiResponseDto<StudentDto>
            {
                Success = true,
                Message = "Student status updated successfully (V1)",
                Data = studentDto
            });
        }

        // POST api/v1/students/validate
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateStudent([FromBody] CreateStudentDto createStudentDto)
        {
            _logger.LogInformation("V1 ValidateStudent endpoint called for student: {StudentNumber}", createStudentDto.StudentNumber);
            
            var student = _mapper.Map<Student>(createStudentDto);
            var validation = await _studentService.ValidateStudentAsync(student);
            
            return Ok(new ApiResponseDto<object>
            {
                Success = validation.IsValid,
                Message = validation.IsValid ? "Student data is valid (V1)" : "Student validation failed (V1)",
                Data = new { IsValid = validation.IsValid, Errors = validation.Errors }
            });
        }

        // GET api/v1/students/{studentNumber}/eligible
        [HttpGet("{studentNumber}/eligible")]
        public async Task<IActionResult> CheckEnrollmentEligibility(string studentNumber)
        {
            _logger.LogInformation("V1 CheckEnrollmentEligibility endpoint called for student: {StudentNumber}", studentNumber);
            
            var isEligible = await _studentService.IsStudentEligibleForEnrollmentAsync(studentNumber);
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Enrollment eligibility checked successfully (V1)",
                Data = new { StudentNumber = studentNumber, IsEligible = isEligible }
            });
        }
    }
} 