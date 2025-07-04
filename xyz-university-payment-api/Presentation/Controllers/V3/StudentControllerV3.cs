using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using xyz_university_payment_api.Core.Application.Services;
using xyz_university_payment_api.Core.Application.DTOs;
using xyz_university_payment_api.Core.Domain.Entities;
using xyz_university_payment_api.Presentation.Attributes;
using xyz_university_payment_api.Core.Application.Interfaces;
using AutoMapper;

namespace xyz_university_payment_api.Presentation.Controllers.V3
{
    /// <summary>
    /// V3 Student Controller with enhanced features
    /// </summary>
    [ApiController]
    [Route("api/v3/[controller]")]
    [ApiVersion("3.0")]
    [ApiExplorerSettings(GroupName = "v3")]
    [Authorize]
    public class StudentControllerV3 : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly ICacheService _cacheService;
        private readonly ILoggingService _loggingService;
        private readonly IMapper _mapper;
        private readonly ILogger<StudentControllerV3> _logger;

        public StudentControllerV3(
            IStudentService studentService,
            ICacheService cacheService,
            ILoggingService loggingService,
            IMapper mapper,
            ILogger<StudentControllerV3> logger)
        {
            _studentService = studentService;
            _cacheService = cacheService;
            _loggingService = loggingService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all students with advanced filtering and analytics
        /// </summary>
        [HttpGet]
        [AuthorizePermission("Students", "Read")]
        public async Task<IActionResult> GetStudents(
            [FromQuery] StudentFilterDtoV3 filter,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "StudentNumber",
            [FromQuery] string sortOrder = "asc",
            [FromQuery] bool includeAnalytics = false)
        {
            try
            {
                var cacheKey = $"students_v3_{filter.GetHashCode()}_{page}_{pageSize}_{sortBy}_{sortOrder}_{includeAnalytics}";
                
                // Try to get from cache first
                var cachedResult = await _cacheService.GetAsync<ApiResponse<PaginatedResponseDto<StudentDtoV3>>>(cacheKey);
                if (cachedResult != null)
                {
                    return Ok(cachedResult);
                }

                var result = await _studentService.GetStudentsV3Async(filter, page, pageSize, sortBy, sortOrder, includeAnalytics);
                
                if (result.Success)
                {
                    // Cache the result for 5 minutes
                    await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving students in V3");
                await _loggingService.LogErrorAsync("StudentControllerV3.GetStudents", ex);
                throw;
            }
        }

        /// <summary>
        /// Get student by ID with detailed information and payment history
        /// </summary>
        [HttpGet("{id}")]
        [AuthorizePermission("Students", "Read")]
        public async Task<IActionResult> GetStudent(int id, [FromQuery] bool includePaymentHistory = true)
        {
            try
            {
                var cacheKey = $"student_v3_{id}_{includePaymentHistory}";
                
                var cachedResult = await _cacheService.GetAsync<ApiResponse<StudentDetailDtoV3>>(cacheKey);
                if (cachedResult != null)
                {
                    return Ok(cachedResult);
                }

                var result = await _studentService.GetStudentV3Async(id, includePaymentHistory);
                
                if (result.Success)
                {
                    await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student {StudentId} in V3", id);
                await _loggingService.LogErrorAsync("StudentControllerV3.GetStudent", ex);
                throw;
            }
        }

        /// <summary>
        /// Create a new student with enhanced validation
        /// </summary>
        [HttpPost]
        [AuthorizePermission("Students", "Create")]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDtoV3 createStudentDto)
        {
            try
            {
                var result = await _studentService.CreateStudentV3Async(createStudentDto);
                
                if (result.Success)
                {
                    // Invalidate related caches
                    await _cacheService.RemoveByPatternAsync("students_v3_*");
                    await _cacheService.RemoveByPatternAsync("student_v3_*");
                }

                return CreatedAtAction(nameof(GetStudent), new { id = result.Data?.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating student in V3");
                await _loggingService.LogErrorAsync("StudentControllerV3.CreateStudent", ex);
                throw;
            }
        }

        /// <summary>
        /// Update student with partial updates support
        /// </summary>
        [HttpPut("{id}")]
        [AuthorizePermission("Students", "Update")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentDtoV3 updateStudentDto)
        {
            try
            {
                var result = await _studentService.UpdateStudentV3Async(id, updateStudentDto);
                
                if (result.Success)
                {
                    // Invalidate related caches
                    await _cacheService.RemoveByPatternAsync("students_v3_*");
                    await _cacheService.RemoveAsync($"student_v3_{id}_*");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student {StudentId} in V3", id);
                await _loggingService.LogErrorAsync("StudentControllerV3.UpdateStudent", ex);
                throw;
            }
        }

        /// <summary>
        /// Delete student with soft delete support
        /// </summary>
        [HttpDelete("{id}")]
        [AuthorizePermission("Students", "Delete")]
        public async Task<IActionResult> DeleteStudent(int id, [FromQuery] bool permanent = false)
        {
            try
            {
                var result = await _studentService.DeleteStudentV3Async(id, permanent);
                
                if (result.Success)
                {
                    // Invalidate related caches
                    await _cacheService.RemoveByPatternAsync("students_v3_*");
                    await _cacheService.RemoveAsync($"student_v3_{id}_*");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student {StudentId} in V3", id);
                await _loggingService.LogErrorAsync("StudentControllerV3.DeleteStudent", ex);
                throw;
            }
        }

        /// <summary>
        /// Get student analytics and statistics
        /// </summary>
        [HttpGet("analytics")]
        [AuthorizePermission("Students", "Read")]
        public async Task<IActionResult> GetStudentAnalytics([FromQuery] StudentAnalyticsFilterDto filter)
        {
            try
            {
                var cacheKey = $"student_analytics_v3_{filter.GetHashCode()}";
                
                var cachedResult = await _cacheService.GetAsync<ApiResponse<StudentAnalyticsDto>>(cacheKey);
                if (cachedResult != null)
                {
                    return Ok(cachedResult);
                }

                var result = await _studentService.GetStudentAnalyticsAsync(filter);
                
                if (result.Success)
                {
                    await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15));
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student analytics in V3");
                await _loggingService.LogErrorAsync("StudentControllerV3.GetStudentAnalytics", ex);
                throw;
            }
        }

        /// <summary>
        /// Bulk operations on students
        /// </summary>
        [HttpPost("bulk")]
        [AuthorizePermission("Students", "Create")]
        public async Task<IActionResult> BulkOperations([FromBody] BulkStudentOperationDto bulkOperation)
        {
            try
            {
                var result = await _studentService.BulkOperationsAsync(bulkOperation);
                
                if (result.Success)
                {
                    // Invalidate all student caches
                    await _cacheService.RemoveByPatternAsync("students_v3_*");
                    await _cacheService.RemoveByPatternAsync("student_v3_*");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk operations in V3");
                await _loggingService.LogErrorAsync("StudentControllerV3.BulkOperations", ex);
                throw;
            }
        }

        /// <summary>
        /// Export students data in various formats
        /// </summary>
        [HttpGet("export")]
        [AuthorizePermission("Students", "Read")]
        public async Task<IActionResult> ExportStudents(
            [FromQuery] StudentFilterDtoV3 filter,
            [FromQuery] string format = "json",
            [FromQuery] bool includePaymentHistory = false)
        {
            try
            {
                var result = await _studentService.ExportStudentsAsync(filter, format, includePaymentHistory);
                
                if (result.Success)
                {
                    var contentType = format.ToLower() switch
                    {
                        "csv" => "text/csv",
                        "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "pdf" => "application/pdf",
                        _ => "application/json"
                    };

                    var fileName = $"students_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{format}";
                    
                    Response.Headers["Content-Disposition"] = $"attachment; filename={fileName}";
                    return File((result.Data as byte[])!, contentType, fileName);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting students in V3");
                await _loggingService.LogErrorAsync("StudentControllerV3.ExportStudents", ex);
                throw;
            }
        }
    }
} 