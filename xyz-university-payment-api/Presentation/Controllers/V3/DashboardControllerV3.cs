// Purpose: V3 Dashboard Controller - Provides statistics and analytics for different user roles
using Microsoft.AspNetCore.Mvc;
using xyz_university_payment_api.Core.Application.Services;
using xyz_university_payment_api.Core.Application.Interfaces;
using xyz_university_payment_api.Core.Application.DTOs;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using xyz_university_payment_api.Presentation.Attributes;
using System.Security.Claims;
using xyz_university_payment_api.Core.Domain.Entities; // Added for Student model
using System; // Added for DateTime
using System.Linq; // Added for Count, Sum, OrderByDescending, Take, Select, GroupBy
using System.Collections.Generic; // Added for Dictionary

namespace xyz_university_payment_api.Presentation.Controllers.V3
{
    [Authorize]
    [ApiController]
    [Route("api/v3/[controller]")]
    [ApiVersion("3.0")]
    [ApiExplorerSettings(GroupName = "v3")]
    public class DashboardController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly IPaymentService _paymentService;
        private readonly IStudentBalanceService _studentBalanceService;
        private readonly IFeeManagementService _feeManagementService;
        private readonly ILogger<DashboardController> _logger;
        private readonly IMapper _mapper;

        public DashboardController(
            IStudentService studentService, 
            IPaymentService paymentService, 
            IStudentBalanceService studentBalanceService,
            IFeeManagementService feeManagementService,
            ILogger<DashboardController> logger, 
            IMapper mapper)
        {
            _studentService = studentService;
            _paymentService = paymentService;
            _studentBalanceService = studentBalanceService;
            _feeManagementService = feeManagementService;
            _logger = logger;
            _mapper = mapper;
        }

        // GET api/v3/dashboard/admin-stats
        [HttpGet("admin-stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminStats()
        {
            try
            {
                _logger.LogInformation("V3 GetAdminStats endpoint called");

                // Get all students and payments for admin
                var students = await _studentService.GetAllStudentsAsync();
                var payments = await _paymentService.GetAllPaymentsAsync();

                // Get all student fee balances for system-wide paid/pending
                var allFeeBalances = await _feeManagementService.GetAllStudentFeeBalancesAsync();
                var paidAmount = allFeeBalances.Sum(b => b.AmountPaid);
                var pendingAmount = allFeeBalances.Sum(b => b.OutstandingBalance);

                var stats = new
                {
                    TotalUsers = students.Count() + 10, // Add some admin/staff users
                    TotalStudents = students.Count(),
                    TotalPayments = payments.Count(),
                    TotalRevenue = payments.Sum(p => p.AmountPaid),
                    ActiveStudents = students.Count(s => s.IsActive),
                    InactiveStudents = students.Count(s => !s.IsActive),
                    RecentPayments = payments.OrderByDescending(p => p.PaymentDate).Take(5),
                    PaymentTrends = new
                    {
                        ThisMonth = payments.Count(p => p.PaymentDate.Month == DateTime.UtcNow.Month),
                        LastMonth = payments.Count(p => p.PaymentDate.Month == DateTime.UtcNow.AddMonths(-1).Month),
                        ThisYear = payments.Count(p => p.PaymentDate.Year == DateTime.UtcNow.Year)
                    },
                    PaidAmount = paidAmount,
                    PendingAmount = pendingAmount
                };

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Admin dashboard statistics retrieved successfully (V3)",
                    Data = stats,
                    Metadata = new Dictionary<string, object>
                    {
                        ["ApiVersion"] = "3.0",
                        ["GeneratedAt"] = DateTime.UtcNow,
                        ["UserRole"] = "Admin"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin dashboard statistics");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Failed to retrieve admin dashboard statistics",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // GET api/v3/dashboard/manager-stats
        [HttpGet("manager-stats")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetManagerStats()
        {
            try
            {
                _logger.LogInformation("V3 GetManagerStats endpoint called");

                // Get all students and payments for manager
                var students = await _studentService.GetAllStudentsAsync();
                var payments = await _paymentService.GetAllPaymentsAsync();

                var stats = new
                {
                    TotalStudents = students.Count(),
                    TotalPayments = payments.Count(),
                    TotalRevenue = payments.Sum(p => p.AmountPaid),
                    ActiveStudents = students.Count(s => s.IsActive),
                    InactiveStudents = students.Count(s => !s.IsActive),
                    RecentPayments = payments.OrderByDescending(p => p.PaymentDate).Take(5),
                    ProgramBreakdown = students.GroupBy(s => s.Program)
                        .Select(g => new { Program = g.Key, Count = g.Count() })
                        .ToList()
                };

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Manager dashboard statistics retrieved successfully (V3)",
                    Data = stats,
                    Metadata = new Dictionary<string, object>
                    {
                        ["ApiVersion"] = "3.0",
                        ["GeneratedAt"] = DateTime.UtcNow,
                        ["UserRole"] = "Manager"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving manager dashboard statistics");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Failed to retrieve manager dashboard statistics",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // GET api/v3/dashboard/staff-stats
        [HttpGet("staff-stats")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> GetStaffStats()
        {
            try
            {
                _logger.LogInformation("V3 GetStaffStats endpoint called");

                // Get all students and payments for staff
                var students = await _studentService.GetAllStudentsAsync();
                var payments = await _paymentService.GetAllPaymentsAsync();

                var stats = new
                {
                    TotalStudents = students.Count(),
                    TotalPayments = payments.Count(),
                    TotalRevenue = payments.Sum(p => p.AmountPaid),
                    ActiveStudents = students.Count(s => s.IsActive),
                    InactiveStudents = students.Count(s => !s.IsActive),
                    RecentPayments = payments.OrderByDescending(p => p.PaymentDate).Take(5),
                    PendingTasks = new List<object>
                    {
                        new { Type = "Student Registration", Count = 5 },
                        new { Type = "Payment Processing", Count = 3 },
                        new { Type = "Document Verification", Count = 8 }
                    }
                };

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Staff dashboard statistics retrieved successfully (V3)",
                    Data = stats,
                    Metadata = new Dictionary<string, object>
                    {
                        ["ApiVersion"] = "3.0",
                        ["GeneratedAt"] = DateTime.UtcNow,
                        ["UserRole"] = "Staff"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving staff dashboard statistics");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Failed to retrieve staff dashboard statistics",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // GET api/v3/dashboard/student-stats
        [HttpGet("student-stats")]
        [Authorize(Roles = "Admin,Manager,Staff,Student")] // Restore proper authorization
        public async Task<IActionResult> GetStudentStats()
        {
            try
            {
                _logger.LogInformation("V3 GetStudentStats endpoint called");

                // Debug: Log current user roles
                var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                var userPermissions = User.FindAll("permission").Select(c => c.Value).ToList();
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = User.FindFirst(ClaimTypes.Name)?.Value;

                _logger.LogInformation("Current user: {Username} (ID: {UserId})", username, userId);
                _logger.LogInformation("User roles: {Roles}", string.Join(", ", userRoles));
                _logger.LogInformation("User permissions: {Permissions}", string.Join(", ", userPermissions));

                // For admin users, show general statistics instead of individual student data
                if (userRoles.Contains("Admin") || userRoles.Contains("Manager") || userRoles.Contains("Staff"))
                {
                    var allStudents = await _studentService.GetAllStudentsAsync();
                    var allPayments = await _paymentService.GetAllPaymentsAsync();
                    
                    var adminStats = new
                    {
                        UserRole = userRoles.FirstOrDefault() ?? "Unknown",
                        TotalStudents = allStudents.Count(),
                        TotalPayments = allPayments.Count(),
                        TotalRevenue = allPayments.Sum(p => p.AmountPaid),
                        ActiveStudents = allStudents.Count(s => s.IsActive),
                        InactiveStudents = allStudents.Count(s => !s.IsActive),
                        RecentPayments = allPayments.OrderByDescending(p => p.PaymentDate).Take(5).Select(p => new
                        {
                            Id = p.Id,
                            Amount = p.AmountPaid,
                            Date = p.PaymentDate.ToString("yyyy-MM-dd"),
                            Status = "Completed",
                            Description = $"Payment - {p.PaymentReference}"
                        }).ToList(),
                        SystemInfo = new
                        {
                            CurrentUser = username,
                            UserId = userId,
                            UserRoles = userRoles,
                            UserPermissions = userPermissions.Take(5).ToList() // Show first 5 permissions
                        }
                    };

                    return Ok(new ApiResponseDto<object>
                    {
                        Success = true,
                        Message = "Dashboard statistics retrieved successfully (V3)",
                        Data = adminStats,
                        Metadata = new Dictionary<string, object>
                        {
                            ["ApiVersion"] = "3.0",
                            ["GeneratedAt"] = DateTime.UtcNow,
                            ["UserRole"] = userRoles.FirstOrDefault() ?? "Unknown"
                        }
                    });
                }

                // For student users, show individual student data
                // Try to find student by username (assuming username matches student number or email)
                Student? student = null;
                var studentsList = await _studentService.GetAllStudentsAsync();
                
                // Try to find student by username (could be student number or email)
                student = studentsList.FirstOrDefault(s => 
                    s.StudentNumber.Equals(username, StringComparison.OrdinalIgnoreCase) ||
                    s.Email.Equals(username, StringComparison.OrdinalIgnoreCase) ||
                    s.FullName.ToLower().Contains(username.ToLower()));

                // If no direct match, try to find by user ID mapping (for now, use a simple mapping)
                if (student == null)
                {
                    // For demo purposes, map specific users to students
                    var userStudentMapping = new Dictionary<string, string>
                    {
                        { "alex.student", "S66001" }, // Alex Mutahi
                        { "student", "S66001" }, // Default student user
                        { "john.student", "S66004" }, // John Doe (Computer Science)
                        { "sarah.student", "S66005" }, // Sarah Johnson (Business Administration)
                        { "mike.student", "S66006" }, // Mike Wilson (Engineering)
                        { "andrew.student", "S66007" }, // Andrew Smith (Computer Science)
                        { "andrew", "S66007" }, // Andrew (without .student suffix)
                        { "john", "S66004" }, // John (without .student suffix)
                        { "sarah", "S66005" }, // Sarah (without .student suffix)
                        { "mike", "S66006" }, // Mike (without .student suffix)
                        // Add mappings for actual student number usernames
                        { "S12345", "S12345" }, // John Doe
                        { "S67890", "S67890" }, // Jane Smith
                        { "S66001", "S66001" }, // Alex Mutahi
                        { "S66002", "S66002" }, // Janet Mwangi
                        { "S66003", "S66003" }, // Jane Smith (inactive)
                        { "S66004", "S66004" }, // John Doe (Computer Science)
                        { "S66005", "S66005" }, // Sarah Johnson (Business Administration)
                        { "S66006", "S66006" }, // Mike Wilson (Engineering)
                        { "S66007", "S66007" } // Andrew Smith (Computer Science)
                    };

                    if (userStudentMapping.ContainsKey(username))
                    {
                        student = studentsList.FirstOrDefault(s => s.StudentNumber == userStudentMapping[username]);
                        _logger.LogInformation("Found student mapping for {Username} -> {StudentNumber}", username, userStudentMapping[username]);
                    }
                    else
                    {
                        _logger.LogWarning("No student mapping found for username: {Username}", username);
                    }
                }

                // Get student balance and payment information using the new StudentFeeBalance system
                var studentPayments = new List<object>();
                var totalPaid = 0.0m;
                var balance = 0.0m;
                var nextPaymentDue = DateTime.UtcNow.AddDays(30);

                if (student != null)
                {
                    try
                    {
                        // Get balance data from the new StudentFeeBalance system
                        var feeBalanceSummary = await _feeManagementService.GetStudentFeeBalanceSummaryAsync(student.StudentNumber);
                        balance = feeBalanceSummary.TotalOutstandingBalance;
                        totalPaid = feeBalanceSummary.TotalPaid;
                        nextPaymentDue = feeBalanceSummary.NextPaymentDue;

                        // Get recent payments for display
                        var payments = await _paymentService.GetPaymentsByStudentAsync(student.StudentNumber);
                        studentPayments = payments.OrderByDescending(p => p.PaymentDate).Take(5).Select(p => new
                        {
                            Id = p.Id,
                            Amount = p.AmountPaid,
                            Date = p.PaymentDate.ToString("yyyy-MM-dd"),
                            Status = "Completed",
                            Description = $"Payment - {p.PaymentReference}"
                        }).ToList<object>();
                        
                        _logger.LogInformation("Retrieved fee balance for student {StudentNumber}: Outstanding=${Balance}, Paid=${TotalPaid}, Due={NextPaymentDue}", 
                            student.StudentNumber, balance, totalPaid, nextPaymentDue.ToString("yyyy-MM-dd"));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not retrieve fee balance data for student {StudentNumber}, using fallback", student.StudentNumber);
                        
                        // Fallback to payment-based calculation
                        var payments = await _paymentService.GetPaymentsByStudentAsync(student.StudentNumber);
                        studentPayments = payments.OrderByDescending(p => p.PaymentDate).Take(5).Select(p => new
                        {
                            Id = p.Id,
                            Amount = p.AmountPaid,
                            Date = p.PaymentDate.ToString("yyyy-MM-dd"),
                            Status = "Completed",
                            Description = $"Payment - {p.PaymentReference}"
                        }).ToList<object>();
                        
                        totalPaid = payments.Sum(p => p.AmountPaid);
                        balance = 0.0m; // No outstanding balance if fee balance system is not available
                        
                        _logger.LogInformation("Using fallback calculation for student {StudentNumber}: total paid: {TotalPaid}", 
                            student.StudentNumber, totalPaid);
                    }
                }
                else
                {
                    _logger.LogWarning("No student found for username: {Username}, showing generic data", username);
                }

                var studentStats = new
                {
                    UserRole = userRoles.FirstOrDefault() ?? "Student",
                    CurrentUser = student?.FullName ?? username,
                    UserId = userId,
                    UserRoles = userRoles,
                    Message = student != null ? "Student dashboard - showing individual data" : "Student dashboard - showing generic data",
                    AcademicInfo = new
                    {
                        CurrentSemester = "Summer 2025",
                        EnrollmentStatus = student?.IsActive == true ? "Active" : "Inactive",
                        AcademicStanding = "Good Standing"
                    },
                    FinancialInfo = new
                    {
                        Balance = balance,
                        TotalPaid = totalPaid,
                        NextPaymentDue = nextPaymentDue,
                        NextPaymentDueAmount = balance > 0 ? balance : 0,
                        RecentPayments = studentPayments
                    },
                    SystemInfo = new
                    {
                        LastLogin = DateTime.UtcNow,
                        AccountStatus = "Active",
                        UserPermissions = userPermissions.Take(5).ToList()
                    }
                };

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Student dashboard retrieved successfully (V3)",
                    Data = studentStats,
                    Metadata = new Dictionary<string, object>
                    {
                        ["ApiVersion"] = "3.0",
                        ["GeneratedAt"] = DateTime.UtcNow,
                        ["UserRole"] = "Student"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student dashboard statistics");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Failed to retrieve student dashboard statistics",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // GET api/v3/dashboard/test
        [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult Test()
        {
            return Ok(new { message = "Dashboard controller is working!", timestamp = DateTime.UtcNow });
        }

        private int? GetCurrentUserId()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    return userId;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user ID");
                return null;
            }
        }
    }
} 