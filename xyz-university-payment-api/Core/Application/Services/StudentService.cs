// Purpose: Student service implementation with business logic
// Provides concrete implementation of IStudentService interface
using xyz_university_payment_api.Core.Application.Interfaces;
using xyz_university_payment_api.Core.Domain.Entities;
using xyz_university_payment_api.Core.Domain.Exceptions;
using xyz_university_payment_api.Core.Application.DTOs;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;
using static System.Math;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace xyz_university_payment_api.Core.Application.Services
{
    /// <summary>
    /// Student service implementation
    /// Provides concrete implementation of IStudentService interface with business logic
    /// </summary>
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StudentService> _logger;
        private readonly ICacheService _cacheService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StudentService(IUnitOfWork unitOfWork, ILogger<StudentService> logger, ICacheService cacheService, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cacheService = cacheService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all students");
                return await _unitOfWork.Students.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all students");
                throw new DatabaseException("Failed to retrieve students", ex);
            }
        }

        public async Task<Student?> GetStudentByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving student with ID: {StudentId}", id);

                // Try to get from cache first
                var cacheKey = _cacheService.GetStudentCacheKey($"id:{id}");
                var cachedStudent = await _cacheService.GetAsync<Student>(cacheKey);

                if (cachedStudent != null)
                {
                    _logger.LogInformation("Student retrieved from cache: {StudentId}", id);
                    return cachedStudent;
                }

                // If not in cache, get from database
                var student = await _unitOfWork.Students.GetByIdAsync(id);

                if (student == null)
                {
                    throw new StudentNotFoundException(id);
                }

                // Cache the student
                await _cacheService.SetAsync(cacheKey, student, TimeSpan.FromMinutes(120));
                _logger.LogInformation("Student cached: {StudentId}", id);

                return student;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student with ID: {StudentId}", id);
                throw new DatabaseException($"Failed to retrieve student with ID {id}", ex);
            }
        }

        public async Task<Student?> GetStudentByNumberAsync(string studentNumber)
        {
            try
            {
                _logger.LogInformation("Retrieving student with number: {StudentNumber}", studentNumber);

                // Try to get from cache first
                var cacheKey = _cacheService.GetStudentCacheKey($"number:{studentNumber}");
                var cachedStudent = await _cacheService.GetAsync<Student>(cacheKey);

                if (cachedStudent != null)
                {
                    _logger.LogInformation("Student retrieved from cache: {StudentNumber}", studentNumber);
                    return cachedStudent;
                }

                // If not in cache, get from database
                var student = await _unitOfWork.Students.FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);

                if (student == null)
                {
                    throw new StudentNotFoundException(studentNumber);
                }

                // Cache the student
                await _cacheService.SetAsync(cacheKey, student, TimeSpan.FromMinutes(120));
                _logger.LogInformation("Student cached: {StudentNumber}", studentNumber);

                return student;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student with number: {StudentNumber}", studentNumber);
                throw new DatabaseException($"Failed to retrieve student with number {studentNumber}", ex);
            }
        }

        public async Task<Student> CreateStudentAsync(Student student)
        {
            try
            {
                _logger.LogInformation("Creating new student: {StudentNumber}", student.StudentNumber);

                // Business validation
                var validation = await ValidateStudentAsync(student);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Student validation failed: {Errors}", string.Join(", ", validation.Errors));
                    throw new ValidationException(validation.Errors);
                }

                // Check for duplicate student number
                if (await _unitOfWork.Students.AnyAsync(s => s.StudentNumber == student.StudentNumber))
                {
                    _logger.LogWarning("Student number already exists: {StudentNumber}", student.StudentNumber);
                    throw new DuplicateStudentException(student.StudentNumber);
                }

                var createdStudent = await _unitOfWork.Students.AddAsync(student);
                _logger.LogInformation("Student created successfully: {StudentNumber}", student.StudentNumber);

                // Invalidate related caches
                await InvalidateStudentCachesAsync(student.StudentNumber);

                return createdStudent;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating student: {StudentNumber}", student.StudentNumber);
                throw new DatabaseException($"Failed to create student {student.StudentNumber}", ex);
            }
        }

        public async Task<Student> UpdateStudentAsync(Student student)
        {
            try
            {
                _logger.LogInformation("Updating student: {StudentNumber}", student.StudentNumber);

                // Business validation
                var validation = await ValidateStudentAsync(student);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Student validation failed: {Errors}", string.Join(", ", validation.Errors));
                    throw new ValidationException(validation.Errors);
                }

                // Check if student exists
                var existingStudent = await _unitOfWork.Students.GetByIdAsync(student.Id);
                if (existingStudent == null)
                {
                    _logger.LogWarning("Student not found for update: {StudentId}", student.Id);
                    throw new StudentNotFoundException(student.Id);
                }

                var updatedStudent = await _unitOfWork.Students.UpdateAsync(student);
                _logger.LogInformation("Student updated successfully: {StudentNumber}", student.StudentNumber);
                return updatedStudent;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student: {StudentNumber}", student.StudentNumber);
                throw new DatabaseException($"Failed to update student {student.StudentNumber}", ex);
            }
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting student with ID: {StudentId}", id);

                var student = await _unitOfWork.Students.GetByIdAsync(id);
                if (student == null)
                {
                    _logger.LogWarning("Student not found for deletion: {StudentId}", id);
                    return false;
                }

                // Business rule: Check if student can be deleted
                if (student.IsActive)
                {
                    _logger.LogWarning("Cannot delete active student: {StudentNumber}", student.StudentNumber);
                    throw new InvalidOperationException($"Cannot delete active student {student.StudentNumber}");
                }

                await _unitOfWork.Students.DeleteAsync(student);
                _logger.LogInformation("Student deleted successfully: {StudentId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student with ID: {StudentId}", id);
                throw new DatabaseException($"Failed to delete student with ID {id}", ex);
            }
        }

        public async Task<IEnumerable<Student>> GetActiveStudentsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving active students");
                return await _unitOfWork.Students.FindAsync(s => s.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active students");
                throw new DatabaseException("Failed to retrieve active students", ex);
            }
        }

        public async Task<IEnumerable<Student>> GetStudentsByProgramAsync(string program)
        {
            try
            {
                _logger.LogInformation("Retrieving students by program: {Program}", program);
                return await _unitOfWork.Students.FindAsync(s => s.Program.ToUpper() == program.ToUpper());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving students by program: {Program}", program);
                throw new DatabaseException($"Failed to retrieve students by program {program}", ex);
            }
        }

        public async Task<IEnumerable<Student>> SearchStudentsAsync(string searchTerm)
        {
            try
            {
                _logger.LogInformation("Searching students with term: {SearchTerm}", searchTerm);
                return await _unitOfWork.Students.FindAsync(s =>
                    s.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    s.StudentNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching students with term: {SearchTerm}", searchTerm);
                throw new DatabaseException($"Failed to search students with term {searchTerm}", ex);
            }
        }

        public async Task<Student> UpdateStudentStatusAsync(int studentId, bool isActive)
        {
            try
            {
                _logger.LogInformation("Updating student status: {StudentId} to {IsActive}", studentId, isActive);

                var student = await _unitOfWork.Students.GetByIdAsync(studentId);
                if (student == null)
                {
                    throw new StudentNotFoundException(studentId);
                }

                student.IsActive = isActive;
                var updatedStudent = await _unitOfWork.Students.UpdateAsync(student);
                _logger.LogInformation("Student status updated successfully: {StudentId}", studentId);
                return updatedStudent;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student status: {StudentId}", studentId);
                throw new DatabaseException($"Failed to update student status for ID {studentId}", ex);
            }
        }

        public Task<(bool IsValid, List<string> Errors)> ValidateStudentAsync(Student student)
        {
            var errors = new List<string>();

            // Validate student number format
            if (string.IsNullOrWhiteSpace(student.StudentNumber))
            {
                errors.Add("Student number is required");
            }
            else if (!student.StudentNumber.StartsWith("S") || student.StudentNumber.Length < 6)
            {
                errors.Add("Student number must start with 'S' and be at least 6 characters long");
            }

            // Validate full name
            if (string.IsNullOrWhiteSpace(student.FullName))
            {
                errors.Add("Full name is required");
            }
            else if (student.FullName.Length < 2)
            {
                errors.Add("Full name must be at least 2 characters long");
            }

            // Validate program
            if (string.IsNullOrWhiteSpace(student.Program))
            {
                errors.Add("Program is required");
            }

            // Validate program against allowed programs
            var allowedPrograms = new[] { "CS", "IT", "ACC", "SOCIOLOGY", "ENGINEERING", "MEDICINE" };
            if (!allowedPrograms.Contains(student.Program.ToUpper()))
            {
                errors.Add($"Program must be one of: {string.Join(", ", allowedPrograms)}");
            }

            return Task.FromResult((errors.Count == 0, errors));
        }

        public async Task<bool> IsStudentEligibleForEnrollmentAsync(string studentNumber)
        {
            try
            {
                _logger.LogInformation("Checking enrollment eligibility for student: {StudentNumber}", studentNumber);

                var student = await _unitOfWork.Students.FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);
                if (student == null)
                {
                    _logger.LogWarning("Student not found for enrollment check: {StudentNumber}", studentNumber);
                    return false;
                }

                // Business rule: Student must be active to be eligible for enrollment
                return student.IsActive;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking enrollment eligibility for student: {StudentNumber}", studentNumber);
                throw new DatabaseException($"Failed to check enrollment eligibility for student {studentNumber}", ex);
            }
        }

        public async Task<EnrollmentEligibilityResult> CheckEnrollmentEligibilityAsync(string studentNumber)
        {
            try
            {
                _logger.LogInformation("Checking detailed enrollment eligibility for student: {StudentNumber}", studentNumber);

                var result = new EnrollmentEligibilityResult();
                var student = await _unitOfWork.Students.FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);

                if (student == null)
                {
                    result.IsEligible = false;
                    result.Reasons.Add("Student not found in the system");
                    return result;
                }

                // Check if student is active
                if (!student.IsActive)
                {
                    result.Reasons.Add("Student account is not active");
                }

                // Check if student has completed required prerequisites (placeholder for future implementation)
                // This could include checking academic standing, completed courses, etc.

                // Check if student has any outstanding financial obligations (placeholder)
                // This could include checking payment status, outstanding fees, etc.

                // For now, eligibility is based on active status
                result.IsEligible = student.IsActive;

                if (result.IsEligible)
                {
                    result.Reasons.Add("Student meets all enrollment requirements");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking detailed enrollment eligibility for student: {StudentNumber}", studentNumber);
                throw new DatabaseException($"Failed to check enrollment eligibility for student {studentNumber}", ex);
            }
        }

        private async Task InvalidateStudentCachesAsync(string studentNumber)
        {
            try
            {
                var cacheKeys = new[]
                {
                    _cacheService.GetStudentCacheKey($"number:{studentNumber}"),
                    _cacheService.GetStudentCacheKey("all"),
                    _cacheService.GetStudentCacheKey("active")
                };

                foreach (var key in cacheKeys)
                {
                    await _cacheService.RemoveAsync(key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to invalidate student caches for: {StudentNumber}", studentNumber);
            }
        }

        private string GetCurrentUserRole()
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    // Check for role claims in order of priority
                    if (user.IsInRole("Admin")) return "Admin";
                    if (user.IsInRole("Manager")) return "Manager";
                    if (user.IsInRole("Staff")) return "Staff";
                    if (user.IsInRole("Student")) return "Student";
                }
                return "Student"; // Default to student role
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user role");
                return "Student"; // Default to student role on error
            }
        }

        private int? GetCurrentUserId()
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(userIdClaim, out int userId))
                    {
                        return userId;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user ID");
                return null;
            }
        }

        // V3 Methods Implementation
        public async Task<ApiResponse<PaginatedResponseDto<StudentDtoV3>>> GetStudentsV3Async(
            StudentFilterDtoV3 filter, int page, int pageSize, string sortBy, string sortOrder, bool includeAnalytics)
        {
            try
            {
                _logger.LogInformation("Retrieving students V3 with filter: {@Filter}", filter);

                var query = _unitOfWork.Students.Query();

                // Apply role-based filtering
                var currentUserRole = GetCurrentUserRole();
                var currentUserId = GetCurrentUserId();

                _logger.LogInformation("Current user role: {Role}, User ID: {UserId}", currentUserRole, currentUserId);

                // Apply role-based data access rules
                switch (currentUserRole)
                {
                    case "Admin":
                        // Admin can see all students
                        break;
                    case "Manager":
                        // Manager can see all students
                        break;
                    case "Staff":
                        // Staff can see all students
                        break;
                    case "Student":
                        // Student can only see their own data
                        if (currentUserId.HasValue)
                        {
                            query = query.Where(s => s.Id == currentUserId.Value);
                        }
                        else
                        {
                            // If we can't determine the user ID, return empty result
                            return new ApiResponse<PaginatedResponseDto<StudentDtoV3>>
                            {
                                Success = true,
                                Data = new PaginatedResponseDto<StudentDtoV3>
                                {
                                    Data = new List<StudentDtoV3>(),
                                    PageNumber = page,
                                    PageSize = pageSize,
                                    TotalCount = 0,
                                    TotalPages = 0
                                },
                                Message = "No students found for current user"
                            };
                        }
                        break;
                    default:
                        // Default to student access
                        if (currentUserId.HasValue)
                        {
                            query = query.Where(s => s.Id == currentUserId.Value);
                        }
                        break;
                }

                // Apply filters
                if (!string.IsNullOrEmpty(filter.StudentNumber))
                    query = query.Where(s => s.StudentNumber.Contains(filter.StudentNumber));

                if (!string.IsNullOrEmpty(filter.FullName))
                    query = query.Where(s => s.FullName.Contains(filter.FullName));

                if (!string.IsNullOrEmpty(filter.Program))
                    query = query.Where(s => s.Program == filter.Program);

                if (filter.IsActive.HasValue)
                    query = query.Where(s => s.IsActive == filter.IsActive.Value);

                if (filter.CreatedFrom.HasValue)
                    query = query.Where(s => s.CreatedAt >= filter.CreatedFrom.Value);

                if (filter.CreatedTo.HasValue)
                    query = query.Where(s => s.CreatedAt <= filter.CreatedTo.Value);

                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    query = query.Where(s => s.StudentNumber.Contains(filter.SearchTerm) ||
                                           s.FullName.Contains(filter.SearchTerm) ||
                                           s.Program.Contains(filter.SearchTerm));
                }

                // Apply sorting
                query = sortBy.ToLower() switch
                {
                    "name" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(s => s.FullName) : query.OrderBy(s => s.FullName),
                    "program" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(s => s.Program) : query.OrderBy(s => s.Program),
                    "created" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
                    _ => sortOrder.ToLower() == "desc" ? query.OrderByDescending(s => s.Id) : query.OrderBy(s => s.Id)
                };

                var totalCount = await query.CountAsync();
                var students = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                // Calculate analytics values if needed
                int activeStudents = 0;
                int inactiveStudents = 0;
                Dictionary<string, int> studentsByProgram = new();

                if (includeAnalytics)
                {
                    activeStudents = await query.CountAsync(st => st.IsActive);
                    inactiveStudents = await query.CountAsync(st => !st.IsActive);
                    studentsByProgram = await query.GroupBy(st => st.Program)
                        .ToDictionaryAsync(g => g.Key, g => g.Count());
                }

                var studentDtos = students.Select(s => new StudentDtoV3
                {
                    Id = s.Id,
                    StudentNumber = s.StudentNumber,
                    FullName = s.FullName,
                    Program = s.Program,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    Tags = new List<string>(), // TODO: Implement tags
                    Metadata = new Dictionary<string, object>(), // TODO: Implement metadata
                    Statistics = includeAnalytics ? new StudentStatisticsDto
                    {
                        TotalStudents = totalCount,
                        ActiveStudents = activeStudents,
                        InactiveStudents = inactiveStudents,
                        StudentsByProgram = studentsByProgram
                    } : new StudentStatisticsDto()
                }).ToList();

                var paginatedResponse = new PaginatedResponseDto<StudentDtoV3>
                {
                    Data = studentDtos,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return new ApiResponse<PaginatedResponseDto<StudentDtoV3>>
                {
                    Success = true,
                    Data = paginatedResponse,
                    Message = "Students retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving students V3");
                return new ApiResponse<PaginatedResponseDto<StudentDtoV3>>
                {
                    Success = false,
                    Message = "Failed to retrieve students",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<StudentDetailDtoV3>> GetStudentV3Async(int id, bool includePaymentHistory)
        {
            try
            {
                _logger.LogInformation("Retrieving student V3 with ID: {StudentId}", id);

                var student = await GetStudentByIdAsync(id);
                if (student == null)
                {
                    return new ApiResponse<StudentDetailDtoV3>
                    {
                        Success = false,
                        Message = "Student not found"
                    };
                }

                var studentDetail = new StudentDetailDtoV3
                {
                    Id = student.Id,
                    StudentNumber = student.StudentNumber,
                    FullName = student.FullName,
                    Program = student.Program,
                    IsActive = student.IsActive,
                    CreatedAt = student.CreatedAt,
                    UpdatedAt = student.UpdatedAt,
                    Tags = new List<string>(), // TODO: Implement tags
                    Metadata = new Dictionary<string, object>(), // TODO: Implement metadata
                    Profile = new StudentProfileDto
                    {
                        Email = student.Email,
                        PhoneNumber = student.PhoneNumber,
                        DateOfBirth = student.DateOfBirth,
                        Address = student.Address,
                        EmergencyContact = "", // TODO: Add to model
                        EmergencyPhone = "" // TODO: Add to model
                    },
                    Preferences = new StudentPreferencesDto(),
                    RecentActivity = new List<StudentActivityDto>(),
                    PaymentHistory = includePaymentHistory ? new List<PaymentDto>() : new List<PaymentDto>() // TODO: Get payment history
                };

                return new ApiResponse<StudentDetailDtoV3>
                {
                    Success = true,
                    Data = studentDetail,
                    Message = "Student details retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student V3 with ID: {StudentId}", id);
                return new ApiResponse<StudentDetailDtoV3>
                {
                    Success = false,
                    Message = "Failed to retrieve student details",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<StudentDtoV3>> CreateStudentV3Async(CreateStudentDtoV3 createStudentDto)
        {
            try
            {
                _logger.LogInformation("Creating student V3: {StudentNumber}", createStudentDto.StudentNumber);

                var student = new Student
                {
                    StudentNumber = createStudentDto.StudentNumber,
                    FullName = createStudentDto.FullName,
                    Program = createStudentDto.Program,
                    Email = createStudentDto.Email,
                    PhoneNumber = createStudentDto.PhoneNumber,
                    DateOfBirth = createStudentDto.DateOfBirth,
                    Address = createStudentDto.Address,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createdStudent = await CreateStudentAsync(student);

                var studentDto = new StudentDtoV3
                {
                    Id = createdStudent.Id,
                    StudentNumber = createdStudent.StudentNumber,
                    FullName = createdStudent.FullName,
                    Program = createdStudent.Program,
                    IsActive = createdStudent.IsActive,
                    CreatedAt = createdStudent.CreatedAt,
                    UpdatedAt = createdStudent.UpdatedAt,
                    Tags = createStudentDto.Tags,
                    Metadata = createStudentDto.Metadata,
                    Statistics = new StudentStatisticsDto()
                };

                return new ApiResponse<StudentDtoV3>
                {
                    Success = true,
                    Data = studentDto,
                    Message = "Student created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating student V3: {StudentNumber}", createStudentDto.StudentNumber);
                return new ApiResponse<StudentDtoV3>
                {
                    Success = false,
                    Message = "Failed to create student",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<StudentDtoV3>> UpdateStudentV3Async(int id, UpdateStudentDtoV3 updateStudentDto)
        {
            try
            {
                _logger.LogInformation("Updating student V3 with ID: {StudentId}", id);

                var existingStudent = await GetStudentByIdAsync(id);
                if (existingStudent == null)
                {
                    return new ApiResponse<StudentDtoV3>
                    {
                        Success = false,
                        Message = "Student not found"
                    };
                }

                // Update only provided fields
                if (!string.IsNullOrEmpty(updateStudentDto.FullName))
                    existingStudent.FullName = updateStudentDto.FullName;

                if (!string.IsNullOrEmpty(updateStudentDto.Program))
                    existingStudent.Program = updateStudentDto.Program;

                if (!string.IsNullOrEmpty(updateStudentDto.Email))
                    existingStudent.Email = updateStudentDto.Email;

                if (!string.IsNullOrEmpty(updateStudentDto.PhoneNumber))
                    existingStudent.PhoneNumber = updateStudentDto.PhoneNumber;

                if (!string.IsNullOrEmpty(updateStudentDto.Address))
                    existingStudent.Address = updateStudentDto.Address;

                if (updateStudentDto.IsActive.HasValue)
                    existingStudent.IsActive = updateStudentDto.IsActive.Value;

                existingStudent.UpdatedAt = DateTime.UtcNow;

                var updatedStudent = await UpdateStudentAsync(existingStudent);

                var studentDto = new StudentDtoV3
                {
                    Id = updatedStudent.Id,
                    StudentNumber = updatedStudent.StudentNumber,
                    FullName = updatedStudent.FullName,
                    Program = updatedStudent.Program,
                    IsActive = updatedStudent.IsActive,
                    CreatedAt = updatedStudent.CreatedAt,
                    UpdatedAt = updatedStudent.UpdatedAt,
                    Tags = updateStudentDto.Tags ?? new List<string>(),
                    Metadata = updateStudentDto.Metadata ?? new Dictionary<string, object>(),
                    Statistics = new StudentStatisticsDto()
                };

                return new ApiResponse<StudentDtoV3>
                {
                    Success = true,
                    Data = studentDto,
                    Message = "Student updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student V3 with ID: {StudentId}", id);
                return new ApiResponse<StudentDtoV3>
                {
                    Success = false,
                    Message = "Failed to update student",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteStudentV3Async(int id, bool permanent)
        {
            try
            {
                _logger.LogInformation("Deleting student V3 with ID: {StudentId}, Permanent: {Permanent}", id, permanent);

                var result = await DeleteStudentAsync(id);

                return new ApiResponse<bool>
                {
                    Success = result,
                    Data = result,
                    Message = result ? "Student deleted successfully" : "Failed to delete student"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student V3 with ID: {StudentId}", id);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Failed to delete student",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<StudentAnalyticsDto>> GetStudentAnalyticsAsync(StudentAnalyticsFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Retrieving student analytics with filter: {@Filter}", filter);

                var query = _unitOfWork.Students.Query();

                // Apply filters
                if (filter.FromDate.HasValue)
                    query = query.Where(s => s.CreatedAt >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(s => s.CreatedAt <= filter.ToDate.Value);

                if (!string.IsNullOrEmpty(filter.Program))
                    query = query.Where(s => s.Program == filter.Program);

                if (!filter.IncludeInactive)
                    query = query.Where(s => s.IsActive);

                var totalStudents = await query.CountAsync();
                var activeStudents = await query.CountAsync(s => s.IsActive);

                var analytics = new StudentAnalyticsDto
                {
                    TotalStudents = totalStudents,
                    ActiveStudents = activeStudents,
                    TotalRevenue = 0, // TODO: Calculate from payments
                    AverageRevenuePerStudent = 0, // TODO: Calculate from payments
                    StudentsByProgram = await query.GroupBy(s => s.Program)
                        .ToDictionaryAsync(g => g.Key, g => g.Count()),
                    RevenueByMonth = new Dictionary<string, decimal>(), // TODO: Calculate from payments
                    Trends = new List<StudentTrendDto>(), // TODO: Calculate trends
                    CustomMetrics = new Dictionary<string, object>()
                };

                return new ApiResponse<StudentAnalyticsDto>
                {
                    Success = true,
                    Data = analytics,
                    Message = "Student analytics retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student analytics");
                return new ApiResponse<StudentAnalyticsDto>
                {
                    Success = false,
                    Message = "Failed to retrieve student analytics",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<BulkOperationResultDto>> BulkOperationsAsync(BulkStudentOperationDto bulkOperation)
        {
            try
            {
                _logger.LogInformation("Performing bulk operation: {Operation} on {Count} students",
                    bulkOperation.Operation, bulkOperation.StudentIds.Count);

                var result = new BulkOperationResultDto
                {
                    TotalProcessed = bulkOperation.StudentIds.Count,
                    SuccessCount = 0,
                    FailureCount = 0,
                    Errors = new List<string>(),
                    Details = new List<BulkOperationDetailDto>()
                };

                foreach (var studentId in bulkOperation.StudentIds)
                {
                    try
                    {
                        var detail = new BulkOperationDetailDto
                        {
                            StudentId = studentId,
                            Success = false,
                            Changes = new Dictionary<string, object>()
                        };

                        switch (bulkOperation.Operation.ToLower())
                        {
                            case "activate":
                                await UpdateStudentStatusAsync(studentId, true);
                                detail.Success = true;
                                detail.Changes["Status"] = "Activated";
                                break;

                            case "deactivate":
                                await UpdateStudentStatusAsync(studentId, false);
                                detail.Success = true;
                                detail.Changes["Status"] = "Deactivated";
                                break;

                            case "delete":
                                await DeleteStudentAsync(studentId);
                                detail.Success = true;
                                detail.Changes["Action"] = "Deleted";
                                break;

                            default:
                                detail.ErrorMessage = $"Unknown operation: {bulkOperation.Operation}";
                                break;
                        }

                        if (detail.Success)
                            result.SuccessCount++;
                        else
                            result.FailureCount++;

                        result.Details.Add(detail);
                    }
                    catch (Exception ex)
                    {
                        result.FailureCount++;
                        result.Errors.Add($"Student {studentId}: {ex.Message}");
                        result.Details.Add(new BulkOperationDetailDto
                        {
                            StudentId = studentId,
                            Success = false,
                            ErrorMessage = ex.Message
                        });
                    }
                }

                return new ApiResponse<BulkOperationResultDto>
                {
                    Success = true,
                    Data = result,
                    Message = $"Bulk operation completed. Success: {result.SuccessCount}, Failed: {result.FailureCount}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk operation");
                return new ApiResponse<BulkOperationResultDto>
                {
                    Success = false,
                    Message = "Failed to perform bulk operation",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<byte[]>> ExportStudentsAsync(StudentFilterDtoV3 filter, string format, bool includePaymentHistory)
        {
            try
            {
                _logger.LogInformation("Exporting students with format: {Format}", format);

                // For now, return empty byte array as placeholder
                // TODO: Implement actual export logic
                var exportData = new byte[0];

                return new ApiResponse<byte[]>
                {
                    Success = true,
                    Data = exportData,
                    Message = "Students exported successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting students");
                return new ApiResponse<byte[]>
                {
                    Success = false,
                    Message = "Failed to export students",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}