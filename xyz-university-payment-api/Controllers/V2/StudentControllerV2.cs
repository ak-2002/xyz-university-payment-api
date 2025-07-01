// Purpose: V2 Student Controller - Enhanced implementation
using Microsoft.AspNetCore.Mvc;
using xyz_university_payment_api.Services;
using xyz_university_payment_api.Interfaces;
using xyz_university_payment_api.Models;
using xyz_university_payment_api.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;

namespace xyz_university_payment_api.Controllers.V2
{
    [Authorize(Policy = "ApiScope")]
    [ApiController]
    [Route("api/v2/[controller]")]
    [ApiVersion("2.0")]
    [ApiExplorerSettings(GroupName = "v2")]
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

        // GET api/v2/students
        [HttpGet]
        public async Task<IActionResult> GetAllStudents([FromQuery] PaginationDto pagination)
        {
            _logger.LogInformation("V2 GetAllStudents endpoint called");
            var students = await _studentService.GetAllStudentsAsync();
            
            var studentDtos = _mapper.Map<List<StudentDto>>(students);
            
            var totalCount = studentDtos.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / pagination.PageSize);
            var hasPreviousPage = pagination.PageNumber > 1;
            var hasNextPage = pagination.PageNumber < totalPages;
            
            var pagedStudents = studentDtos
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();

            var pagedResult = new PagedResultDto<StudentDto>
            {
                Items = pagedStudents,
                TotalCount = totalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = hasPreviousPage,
                HasNextPage = hasNextPage,
                SortBy = pagination.SortBy,
                SortOrder = pagination.SortOrder
            };

            return Ok(new ApiResponseDto<PagedResultDto<StudentDto>>
            {
                Success = true,
                Message = "Students retrieved successfully (V2)",
                Data = pagedResult,
                Metadata = new Dictionary<string, object>
                {
                    ["ApiVersion"] = "2.0",
                    ["ActiveStudents"] = studentDtos.Count(s => s.IsActive),
                    ["InactiveStudents"] = studentDtos.Count(s => !s.IsActive)
                }
            });
        }

        // GET api/v2/students/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            _logger.LogInformation("V2 GetStudentById endpoint called with ID: {StudentId}", id);
            var student = await _studentService.GetStudentByIdAsync(id);
            
            var studentDto = _mapper.Map<StudentDto>(student);
            return Ok(new ApiResponseDto<StudentDto>
            {
                Success = true,
                Message = "Student retrieved successfully (V2)",
                Data = studentDto,
                Metadata = new Dictionary<string, object>
                {
                    ["ApiVersion"] = "2.0",
                    ["RetrievedAt"] = DateTime.UtcNow
                }
            });
        }

        // GET api/v2/students/number/{studentNumber}
        [HttpGet("number/{studentNumber}")]
        public async Task<IActionResult> GetStudentByNumber(string studentNumber)
        {
            _logger.LogInformation("V2 GetStudentByNumber endpoint called with number: {StudentNumber}", studentNumber);
            var student = await _studentService.GetStudentByNumberAsync(studentNumber);
            
            var studentDto = _mapper.Map<StudentDto>(student);
            return Ok(new ApiResponseDto<StudentDto>
            {
                Success = true,
                Message = "Student retrieved successfully (V2)",
                Data = studentDto
            });
        }

        // POST api/v2/students
        [HttpPost]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDto createStudentDto)
        {
            _logger.LogInformation("V2 CreateStudent endpoint called for student: {StudentNumber}", createStudentDto.StudentNumber);
            
            var student = _mapper.Map<Student>(createStudentDto);
            var createdStudent = await _studentService.CreateStudentAsync(student);
            var studentDto = _mapper.Map<StudentDto>(createdStudent);
            
            return CreatedAtAction(nameof(GetStudentById), new { id = createdStudent.Id }, new ApiResponseDto<StudentDto>
            {
                Success = true,
                Message = "Student created successfully (V2)",
                Data = studentDto
            });
        }

        // PUT api/v2/students/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentDto updateStudentDto)
        {
            _logger.LogInformation("V2 UpdateStudent endpoint called for student ID: {StudentId}", id);
            
            var student = _mapper.Map<Student>(updateStudentDto);
            student.Id = id;
            
            var updatedStudent = await _studentService.UpdateStudentAsync(student);
            var studentDto = _mapper.Map<StudentDto>(updatedStudent);
            
            return Ok(new ApiResponseDto<StudentDto>
            {
                Success = true,
                Message = "Student updated successfully (V2)",
                Data = studentDto
            });
        }

        // DELETE api/v2/students/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            _logger.LogInformation("V2 DeleteStudent endpoint called for student ID: {StudentId}", id);
            
            var result = await _studentService.DeleteStudentAsync(id);
            
            return Ok(new ApiResponseDto<object>
            {
                Success = result,
                Message = result ? "Student deleted successfully (V2)" : "Student not found (V2)",
                Data = new { Deleted = result }
            });
        }

        // GET api/v2/students/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveStudents([FromQuery] PaginationDto pagination)
        {
            _logger.LogInformation("V2 GetActiveStudents endpoint called");
            var students = await _studentService.GetActiveStudentsAsync();
            
            var studentDtos = _mapper.Map<List<StudentDto>>(students);
            
            var totalCount = studentDtos.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / pagination.PageSize);
            var hasPreviousPage = pagination.PageNumber > 1;
            var hasNextPage = pagination.PageNumber < totalPages;
            
            var pagedStudents = studentDtos
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();

            var pagedResult = new PagedResultDto<StudentDto>
            {
                Items = pagedStudents,
                TotalCount = totalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = hasPreviousPage,
                HasNextPage = hasNextPage,
                SortBy = pagination.SortBy,
                SortOrder = pagination.SortOrder
            };

            return Ok(new ApiResponseDto<PagedResultDto<StudentDto>>
            {
                Success = true,
                Message = "Active students retrieved successfully (V2)",
                Data = pagedResult
            });
        }

        // NEW V2 ENDPOINT: GET api/v2/students/analytics
        [HttpGet("analytics")]
        public async Task<IActionResult> GetStudentAnalytics()
        {
            _logger.LogInformation("V2 GetStudentAnalytics endpoint called");
            
            var students = await _studentService.GetAllStudentsAsync();
            
            var analytics = new
            {
                TotalStudents = students.Count(),
                ActiveStudents = students.Count(s => s.IsActive),
                InactiveStudents = students.Count(s => !s.IsActive),
                ProgramBreakdown = students
                    .GroupBy(s => s.Program)
                    .Select(g => new { Program = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count),
                EnrollmentRate = students.Count() > 0 ? (double)students.Count(s => s.IsActive) / students.Count() * 100 : 0
            };

            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Student analytics retrieved successfully (V2)",
                Data = analytics,
                Metadata = new Dictionary<string, object>
                {
                    ["ApiVersion"] = "2.0",
                    ["GeneratedAt"] = DateTime.UtcNow
                }
            });
        }

        // NEW V2 ENDPOINT: GET api/v2/students/search
        [HttpGet("search")]
        public async Task<IActionResult> SearchStudents([FromQuery] string q, [FromQuery] PaginationDto pagination)
        {
            _logger.LogInformation("V2 SearchStudents endpoint called with query: {Query}", q);
            
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Search query is required (V2)"
                });
            }
            
            var students = await _studentService.SearchStudentsAsync(q);
            var studentDtos = _mapper.Map<List<StudentDto>>(students);
            
            var totalCount = studentDtos.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / pagination.PageSize);
            var hasPreviousPage = pagination.PageNumber > 1;
            var hasNextPage = pagination.PageNumber < totalPages;
            
            var pagedStudents = studentDtos
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();

            var pagedResult = new PagedResultDto<StudentDto>
            {
                Items = pagedStudents,
                TotalCount = totalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = hasPreviousPage,
                HasNextPage = hasNextPage,
                SortBy = pagination.SortBy,
                SortOrder = pagination.SortOrder
            };

            return Ok(new ApiResponseDto<PagedResultDto<StudentDto>>
            {
                Success = true,
                Message = $"Search results for '{q}' (V2)",
                Data = pagedResult,
                Metadata = new Dictionary<string, object>
                {
                    ["ApiVersion"] = "2.0",
                    ["SearchQuery"] = q,
                    ["ResultsCount"] = totalCount
                }
            });
        }
    }
}