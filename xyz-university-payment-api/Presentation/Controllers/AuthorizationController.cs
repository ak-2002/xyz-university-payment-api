// Purpose: Authorization Controller for user management, authentication, and role-based access control
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using xyz_university_payment_api.Presentation.Attributes;
using xyz_university_payment_api.Core.Application.DTOs;
using xyz_university_payment_api.Core.Application.Interfaces;
using xyz_university_payment_api.Core.Domain.Entities;
using xyz_university_payment_api.Core.Domain.Exceptions;
using xyz_university_payment_api.Core.Application.Services;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using xyz_university_payment_api.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

namespace xyz_university_payment_api.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all authorization endpoints
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [ApiVersion("3.0")]
    public class AuthorizationController : ControllerBase
    {
        private readonly xyz_university_payment_api.Core.Application.Interfaces.IAuthorizationService _authorizationService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AuthorizationController> _logger;
        private readonly IConfiguration _configuration;

        public AuthorizationController(
            xyz_university_payment_api.Core.Application.Interfaces.IAuthorizationService authorizationService,
            IJwtTokenService jwtTokenService,
            ILogger<AuthorizationController> logger,
            IConfiguration configuration)
        {
            _authorizationService = authorizationService;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
            _configuration = configuration;
        }

        #region Authentication Endpoints

        /// <summary>
        /// Authenticate user and return JWT token
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<UserLoginResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            try
            {
                var result = await _authorizationService.AuthenticateAsync(loginDto);
                if (result == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid username or password",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<UserLoginResponseDto>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = result
                });
            }
            catch (ForbiddenException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", loginDto.Username);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Login failed",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Refresh JWT token using refresh token
        /// </summary>
        /// <param name="refreshTokenDto">Refresh token</param>
        /// <returns>New JWT token</returns>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<UserLoginResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var result = await _authorizationService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
                if (result == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid refresh token",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<UserLoginResponseDto>
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Token refresh failed",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Revoke refresh token
        /// </summary>
        /// <param name="refreshTokenDto">Refresh token to revoke</param>
        /// <returns>Success status</returns>
        [HttpPost("revoke-token")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var result = await _authorizationService.RevokeTokenAsync(refreshTokenDto.RefreshToken);
                return Ok(new ApiResponse<object>
                {
                    Success = result,
                    Message = result ? "Token revoked successfully" : "Token revocation failed",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Token revocation failed",
                    Data = null
                });
            }
        }

        #endregion

        #region User Management Endpoints

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="createUserDto">User creation data</param>
        /// <returns>Created user information</returns>
        [HttpPost("users")]
        [AuthorizePermission("Users", "Create")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                var result = await _authorizationService.CreateUserAsync(createUserDto);
                return CreatedAtAction(nameof(GetUserById), new { userId = result.Id }, new ApiResponse<UserDto>
                {
                    Success = true,
                    Message = "User created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Username}", createUserDto.Username);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User information</returns>
        [HttpGet("users/{userId}")]
        [AuthorizePermission("Users", "Read")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetUserById(int userId)
        {
            try
            {
                var result = await _authorizationService.GetUserByIdAsync(userId);
                if (result == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"User with ID {userId} not found",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<UserDto>
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", userId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to retrieve user",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User information</returns>
        [HttpGet("users/by-username/{username}")]
        [AuthorizePermission("Users", "Read")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            try
            {
                var result = await _authorizationService.GetUserByUsernameAsync(username);
                if (result == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"User with username {username} not found",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<UserDto>
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {Username}", username);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to retrieve user",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>List of all users</returns>
        [HttpGet("users")]
        [AuthorizePermission("Users", "Read")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), 200)]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var result = await _authorizationService.GetAllUsersAsync();
                return Ok(new ApiResponse<IEnumerable<UserDto>>
                {
                    Success = true,
                    Message = "Users retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to retrieve users",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Update user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="updateUserDto">User update data</param>
        /// <returns>Updated user information</returns>
        [HttpPut("users/{userId}")]
        [AuthorizePermission("Users", "Update")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                var result = await _authorizationService.UpdateUserAsync(userId, updateUserDto);
                return Ok(new ApiResponse<UserDto>
                {
                    Success = true,
                    Message = "User updated successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("users/{userId}")]
        [AuthorizePermission("Users", "Delete")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                var result = await _authorizationService.DeleteUserAsync(userId);
                return Ok(new ApiResponse<object>
                {
                    Success = result,
                    Message = result ? "User deleted successfully" : "User deletion failed",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="changePasswordDto">Password change data</param>
        /// <returns>Success status</returns>
        [HttpPost("users/{userId}/change-password")]
        [AuthorizePermission("Users", "Update")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> ChangePassword(int userId, [FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var result = await _authorizationService.ChangePasswordAsync(userId, changePasswordDto);
                return Ok(new ApiResponse<object>
                {
                    Success = result,
                    Message = result ? "Password changed successfully" : "Password change failed",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        #endregion

        #region Role Management Endpoints

        /// <summary>
        /// Create a new role
        /// </summary>
        /// <param name="createRoleDto">Role creation data</param>
        /// <returns>Created role information</returns>
        [HttpPost("roles")]
        [AuthorizePermission("Roles", "Create")]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto createRoleDto)
        {
            try
            {
                var result = await _authorizationService.CreateRoleAsync(createRoleDto);
                return CreatedAtAction(nameof(GetRoleById), new { roleId = result.Id }, new ApiResponse<RoleDto>
                {
                    Success = true,
                    Message = "Role created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role {RoleName}", createRoleDto.Name);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns>Role information</returns>
        [HttpGet("roles/{roleId}")]
        [AuthorizePermission("Roles", "Read")]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetRoleById(int roleId)
        {
            try
            {
                var result = await _authorizationService.GetRoleByIdAsync(roleId);
                if (result == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Role with ID {roleId} not found",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<RoleDto>
                {
                    Success = true,
                    Message = "Role retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role {RoleId}", roleId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to retrieve role",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get all roles (Admin only)
        /// </summary>
        /// <returns>List of all roles</returns>
        [HttpGet("admin/roles")]
        [AuthorizeUserManagement("Read")]
        [Authorize(Roles = "Admin,UserManager")]
        public async Task<IActionResult> GetAllRolesAdmin()
        {
            try
            {
                var result = await _authorizationService.GetAllRolesAsync();
                return Ok(new ApiResponse<IEnumerable<RoleDto>>
                {
                    Success = true,
                    Message = "Roles retrieved successfully (Admin access)",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all roles (Admin access)");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to retrieve roles",
                    Data = null
                });
            }
        }

        #endregion

        #region Permission Management Endpoints

        /// <summary>
        /// Create a new permission
        /// </summary>
        /// <param name="createPermissionDto">Permission creation data</param>
        /// <returns>Created permission information</returns>
        [HttpPost("permissions")]
        [AuthorizePermission("Permissions", "Create")]
        [ProducesResponseType(typeof(ApiResponse<PermissionDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionDto createPermissionDto)
        {
            try
            {
                var result = await _authorizationService.CreatePermissionAsync(createPermissionDto);
                return CreatedAtAction(nameof(GetPermissionById), new { permissionId = result.Id }, new ApiResponse<PermissionDto>
                {
                    Success = true,
                    Message = "Permission created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating permission {PermissionName}", createPermissionDto.Name);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get permission by ID
        /// </summary>
        /// <param name="permissionId">Permission ID</param>
        /// <returns>Permission information</returns>
        [HttpGet("permissions/{permissionId}")]
        [AuthorizePermission("Permissions", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PermissionDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetPermissionById(int permissionId)
        {
            try
            {
                var result = await _authorizationService.GetPermissionByIdAsync(permissionId);
                if (result == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Permission with ID {permissionId} not found",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<PermissionDto>
                {
                    Success = true,
                    Message = "Permission retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permission {PermissionId}", permissionId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to retrieve permission",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get all permissions (Admin only)
        /// </summary>
        /// <returns>List of all permissions</returns>
        [HttpGet("admin/permissions")]
        [AuthorizeUserManagement("Read")]
        [Authorize(Roles = "Admin,UserManager")]
        public async Task<IActionResult> GetAllPermissionsAdmin()
        {
            try
            {
                var result = await _authorizationService.GetAllPermissionsAsync();
                return Ok(new ApiResponse<IEnumerable<PermissionDto>>
                {
                    Success = true,
                    Message = "Permissions retrieved successfully (Admin access)",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all permissions (Admin access)");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to retrieve permissions",
                    Data = null
                });
            }
        }

        #endregion

        #region Authorization Check Endpoints

        /// <summary>
        /// Check if user has specific permission
        /// </summary>
        /// <param name="checkPermissionDto">Permission check data</param>
        /// <returns>Permission check result</returns>
        [HttpPost("check-permission")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PermissionCheckResultDto>), 200)]
        public async Task<IActionResult> CheckPermission([FromBody] CheckPermissionDto checkPermissionDto)
        {
            try
            {
                var result = await _authorizationService.CheckPermissionAsync(checkPermissionDto);
                return Ok(new ApiResponse<PermissionCheckResultDto>
                {
                    Success = true,
                    Message = "Permission check completed",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission for user {Username}", checkPermissionDto.Username);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to check permission",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get user permissions
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>List of user permissions</returns>
        [HttpGet("users/{username}/permissions")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), 200)]
        public async Task<IActionResult> GetUserPermissions(string username)
        {
            try
            {
                var result = await _authorizationService.GetUserPermissionsAsync(username);
                return Ok(new ApiResponse<IEnumerable<string>>
                {
                    Success = true,
                    Message = "User permissions retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permissions for user {Username}", username);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to retrieve user permissions",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get user roles
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>List of user roles</returns>
        [HttpGet("users/{username}/roles")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), 200)]
        public async Task<IActionResult> GetUserRoles(string username)
        {
            try
            {
                var result = await _authorizationService.GetUserRolesAsync(username);
                return Ok(new ApiResponse<IEnumerable<string>>
                {
                    Success = true,
                    Message = "User roles retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles for user {Username}", username);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to retrieve user roles",
                    Data = null
                });
            }
        }

        #endregion

        #region System Endpoints

        /// <summary>
        /// Seed default roles and permissions (Admin only)
        /// </summary>
        /// <returns>Success status</returns>
        [HttpPost("seed-data")]
        [AuthorizeRole("Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> SeedDefaultData()
        {
            try
            {
                await _authorizationService.SeedDefaultRolesAndPermissionsAsync();
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Default roles and permissions seeded successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding default data");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to seed default data",
                    Data = null
                });
            }
        }

        #endregion

        /// <summary>
        /// Test endpoint for admin users only
        /// </summary>
        [HttpGet("admin-test")]
        [AuthorizeAdmin]
        public IActionResult AdminTest()
        {
            var user = User;
            var username = user.FindFirst(ClaimTypes.Name)?.Value;
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
            var permissions = user.FindAll("permission").Select(c => c.Value);

            return Ok(new
            {
                Message = "Admin access granted",
                Username = username,
                Roles = roles,
                Permissions = permissions,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Test endpoint for payment operations
        /// </summary>
        [HttpGet("payment-test")]
        [AuthorizePayment("Read")]
        [Authorize(Policy = "PaymentAccess")]
        public IActionResult PaymentTest()
        {
            var user = User;
            var username = user.FindFirst(ClaimTypes.Name)?.Value;
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
            var permissions = user.FindAll("permission").Select(c => c.Value);

            return Ok(new
            {
                Message = "Payment access granted",
                Username = username,
                Roles = roles,
                Permissions = permissions,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Test endpoint for student operations
        /// </summary>
        [HttpGet("student-test")]
        [AuthorizeStudent("Read")]
        [Authorize(Policy = "StudentAccess")]
        public IActionResult StudentTest()
        {
            var user = User;
            var username = user.FindFirst(ClaimTypes.Name)?.Value;
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
            var permissions = user.FindAll("permission").Select(c => c.Value);

            return Ok(new
            {
                Message = "Student access granted",
                Username = username,
                Roles = roles,
                Permissions = permissions,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Test endpoint for financial operations
        /// </summary>
        [HttpGet("financial-test")]
        [Authorize(Roles = "Admin,FinanceManager,Accountant")]
        [Authorize(Policy = "FinancialAccess")]
        public IActionResult FinancialTest()
        {
            var user = User;
            var username = user.FindFirst(ClaimTypes.Name)?.Value;
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);

            return Ok(new
            {
                Message = "Financial access granted",
                Username = username,
                Roles = roles,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Test endpoint for reporting operations
        /// </summary>
        [HttpGet("reporting-test")]
        [Authorize(Policy = "ReportingAccess")]
        public IActionResult ReportingTest()
        {
            var user = User;
            var username = user.FindFirst(ClaimTypes.Name)?.Value;
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
            var permissions = user.FindAll("permission").Select(c => c.Value);

            return Ok(new
            {
                Message = "Reporting access granted",
                Username = username,
                Roles = roles,
                Permissions = permissions,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Get current user's claims and permissions
        /// </summary>
        [HttpGet("my-info")]
        public IActionResult GetMyInfo()
        {
            var user = User;
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user.FindFirst(ClaimTypes.Name)?.Value;
            var email = user.FindFirst(ClaimTypes.Email)?.Value;
            var isActive = user.FindFirst("is_active")?.Value;
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var permissions = user.FindAll("permission").Select(c => c.Value).ToList();
            var userType = user.FindFirst("user_type")?.Value;
            var createdAt = user.FindFirst("created_at")?.Value;

            return Ok(new
            {
                UserId = userId,
                Username = username,
                Email = email,
                IsActive = isActive == "True",
                UserType = userType,
                CreatedAt = createdAt,
                Roles = roles,
                Permissions = permissions,
                TotalRoles = roles.Count,
                TotalPermissions = permissions.Count,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Test permission check for a specific resource and action
        /// </summary>
        [HttpPost("test-permission")]
        public async Task<IActionResult> TestPermission([FromBody] CheckPermissionDto checkPermissionDto)
        {
            try
            {
                var result = await _authorizationService.CheckPermissionAsync(checkPermissionDto);

                return Ok(new ApiResponse<PermissionCheckResultDto>
                {
                    Success = true,
                    Message = "Permission check completed",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission");
                return StatusCode(500, new ApiResponse<PermissionCheckResultDto>
                {
                    Success = false,
                    Message = "Error checking permission",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Create admin user for testing (public endpoint)
        /// </summary>
        /// <returns>Success status</returns>
        [HttpPost("create-admin")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> CreateAdminUser()
        {
            try
            {
                // Check if admin user already exists
                var existingAdmin = await _authorizationService.GetUserByUsernameAsync("admin");
                if (existingAdmin != null)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Admin user already exists",
                        Data = null
                    });
                }

                // Create admin user
                var createUserDto = new CreateUserDto
                {
                    Username = "admin",
                    Email = "admin@xyzuniversity.edu",
                    Password = "[REDACTED]"
                };

                var result = await _authorizationService.CreateUserAsync(createUserDto);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Admin user created successfully. Username: admin, Password: [REDACTED]",
                    Data = new { Username = "admin", Password = "[REDACTED]" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating admin user");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to create admin user",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Fix admin user roles and permissions (public endpoint for development)
        /// </summary>
        /// <returns>Success status</returns>
        [HttpPost("fix-admin")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> FixAdminUser()
        {
            try
            {
                // Get the admin user
                var adminUser = await _authorizationService.GetUserByUsernameAsync("admin");
                if (adminUser == null)
                {
                    // Create admin user if it doesn't exist
                    var createUserDto = new CreateUserDto
                    {
                        Username = "admin",
                        Email = "admin@xyzuniversity.edu",
                        Password = "Admin123!"
                    };
                    await _authorizationService.CreateUserAsync(createUserDto);
                    adminUser = await _authorizationService.GetUserByUsernameAsync("admin");
                }

                // Force re-seed the database to ensure roles and permissions exist
                await _authorizationService.SeedDefaultRolesAndPermissionsAsync();

                // Manually assign admin role to admin user
                var assignRolesDto = new AssignRolesToUserDto
                {
                    UserId = adminUser.Id,
                    RoleIds = new List<int> { 1 } // Admin role ID (should be 1 if seeded first)
                };
                await _authorizationService.AssignRolesToUserAsync(assignRolesDto);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Admin user roles and permissions fixed successfully. Please login again to get a new token.",
                    Data = new { 
                        Username = "admin", 
                        Password = "Admin123!",
                        Instructions = "Login with these credentials to get a new token with proper permissions"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing admin user");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to fix admin user",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Debug endpoint to check admin user status (public endpoint for development)
        /// </summary>
        /// <returns>Admin user information</returns>
        [HttpGet("debug-admin")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> DebugAdminUser()
        {
            try
            {
                var adminUser = await _authorizationService.GetUserByUsernameAsync("admin");
                
                if (adminUser == null)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Admin user does not exist",
                        Data = new { 
                            UserExists = false,
                            Instructions = "Use /api/Authorization/fix-admin to create the admin user"
                        }
                    });
                }

                // Get user roles
                var userRoles = await _authorizationService.GetUserRolesAsync(adminUser.Username);
                
                // Get user permissions
                var userPermissions = await _authorizationService.GetUserPermissionsAsync(adminUser.Username);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Admin user found",
                    Data = new { 
                        UserExists = true,
                        UserId = adminUser.Id,
                        Username = adminUser.Username,
                        Email = adminUser.Email,
                        IsActive = adminUser.IsActive,
                        CreatedAt = adminUser.CreatedAt,
                        LastLoginAt = adminUser.LastLoginAt,
                        Roles = userRoles,
                        Permissions = userPermissions,
                        RoleCount = userRoles.Count(),
                        PermissionCount = userPermissions.Count(),
                        Instructions = "Try logging in with username: admin, password: Admin123!"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error debugging admin user");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to debug admin user",
                    Data = new { Error = ex.Message }
                });
            }
        }

        /// <summary>
        /// Force fix admin user with proper role assignment (public endpoint for development)
        /// </summary>
        /// <returns>Success status</returns>
        [HttpPost("force-fix-admin")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> ForceFixAdminUser()
        {
            try
            {
                // First, ensure we have roles and permissions
                await _authorizationService.SeedDefaultRolesAndPermissionsAsync();

                // Get the admin user
                var adminUser = await _authorizationService.GetUserByUsernameAsync("admin");
                if (adminUser == null)
                {
                    // Create admin user if it doesn't exist
                    var createUserDto = new CreateUserDto
                    {
                        Username = "admin",
                        Email = "admin@xyzuniversity.edu",
                        Password = "Admin123!"
                    };
                    await _authorizationService.CreateUserAsync(createUserDto);
                    adminUser = await _authorizationService.GetUserByUsernameAsync("admin");
                }

                // Get the Admin role ID
                var adminRole = await _authorizationService.GetRoleByNameAsync("Admin");
                if (adminRole == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Admin role not found after seeding",
                        Data = null
                    });
                }

                // Manually assign admin role to admin user
                var assignRolesDto = new AssignRolesToUserDto
                {
                    UserId = adminUser.Id,
                    RoleIds = new List<int> { adminRole.Id }
                };
                await _authorizationService.AssignRolesToUserAsync(assignRolesDto);

                // Verify the assignment
                var userRoles = await _authorizationService.GetUserRolesAsync(adminUser.Id);
                var userPermissions = await _authorizationService.GetUserPermissionsAsync("admin");

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Admin user roles and permissions fixed successfully. Please login again to get a new token.",
                    Data = new { 
                        Username = "admin", 
                        Password = "Admin123!",
                        UserId = adminUser.Id,
                        AssignedRoles = userRoles.Select(r => r.Name).ToList(),
                        AssignedPermissions = userPermissions.ToList(),
                        Instructions = "Login with these credentials to get a new token with proper permissions"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error force fixing admin user");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to force fix admin user: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Assign admin role to admin user (public endpoint for development)
        /// </summary>
        /// <returns>Success status</returns>
        [HttpPost("assign-admin-role")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> AssignAdminRoleToAdminUser()
        {
            try
            {
                // Get the admin user
                var adminUser = await _authorizationService.GetUserByUsernameAsync("admin");
                if (adminUser == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Admin user not found",
                        Data = null
                    });
                }

                // Get the Admin role
                var adminRole = await _authorizationService.GetRoleByNameAsync("Admin");
                if (adminRole == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Admin role not found",
                        Data = null
                    });
                }

                // Assign admin role to admin user
                var assignRolesDto = new AssignRolesToUserDto
                {
                    UserId = adminUser.Id,
                    RoleIds = new List<int> { adminRole.Id }
                };
                await _authorizationService.AssignRolesToUserAsync(assignRolesDto);

                // Verify the assignment
                var userRoles = await _authorizationService.GetUserRolesAsync(adminUser.Id);
                var userPermissions = await _authorizationService.GetUserPermissionsAsync("admin");

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Admin role assigned successfully to admin user",
                    Data = new { 
                        Username = "admin", 
                        Password = "Admin123!",
                        UserId = adminUser.Id,
                        AssignedRoles = userRoles.Select(r => r.Name).ToList(),
                        AssignedPermissions = userPermissions.ToList(),
                        Instructions = "Login with these credentials to get a new token with proper permissions"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning admin role to admin user");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to assign admin role: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Check what roles exist in the database (public endpoint for development)
        /// </summary>
        /// <returns>List of roles</returns>
        [HttpGet("check-roles")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> CheckRoles()
        {
            try
            {
                var roles = await _authorizationService.GetAllRolesAsync();
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Roles found",
                    Data = new { 
                        RoleCount = roles.Count(),
                        Roles = roles.Select(r => new { 
                            Id = r.Id, 
                            Name = r.Name, 
                            Description = r.Description,
                            IsActive = r.IsActive,
                            PermissionCount = r.Permissions.Count
                        }).ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking roles");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to check roles: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Reset admin user password (public endpoint for development)
        /// </summary>
        /// <returns>Success status</returns>
        [HttpPost("reset-admin-password")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> ResetAdminPassword()
        {
            try
            {
                // Get the admin user
                var adminUser = await _authorizationService.GetUserByUsernameAsync("admin");
                if (adminUser == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Admin user not found",
                        Data = null
                    });
                }

                // Reset password to "Admin123!"
                var result = await _authorizationService.ChangePasswordAsync(adminUser.Id, new ChangePasswordDto
                {
                    CurrentPassword = "Admin123!", // This might fail, but we'll handle it
                    NewPassword = "Admin123!",
                    ConfirmPassword = "Admin123!"
                });

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Admin password reset successfully",
                    Data = new { 
                        Username = "admin",
                        Password = "Admin123!",
                        Instructions = "Try logging in with these credentials"
                    }
                });
            }
            catch (ValidationException ex) when (ex.Message.Contains("Current password is incorrect"))
            {
                // If current password is wrong, we need to force update the password hash
                try
                {
                    // Get the admin user directly from context to force update password
                    var adminUser = await _authorizationService.GetUserByUsernameAsync("admin");
                    if (adminUser != null)
                    {
                        // Force update the password hash in the database
                        var context = _authorizationService.GetDbContext(); // We'll need to add this method
                        var user = await context.Users.FindAsync(adminUser.Id);
                        if (user != null)
                        {
                            // Hash the password manually
                            using var sha256 = System.Security.Cryptography.SHA256.Create();
                            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes("Admin123!"));
                            var passwordHash = Convert.ToBase64String(hashedBytes);
                            
                            user.PasswordHash = passwordHash;
                            await context.SaveChangesAsync();
                            
                            return Ok(new ApiResponse<object>
                            {
                                Success = true,
                                Message = "Admin password force reset successfully",
                                Data = new { 
                                    Username = "admin",
                                    Password = "Admin123!",
                                    Instructions = "Try logging in with these credentials"
                                }
                            });
                        }
                    }
                }
                catch (Exception forceEx)
                {
                    _logger.LogError(forceEx, "Error force resetting admin password");
                }
                
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to reset admin password",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting admin password");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to reset admin password",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Test JWT configuration (public endpoint for development)
        /// </summary>
        /// <returns>JWT configuration information</returns>
        [HttpGet("test-jwt-config")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public IActionResult TestJwtConfig()
        {
            try
            {
                var jwtKey = _configuration["Jwt:Key"];
                var jwtIssuer = _configuration["Jwt:Issuer"];
                var jwtAudience = _configuration["Jwt:Audience"];
                var jwtExpiry = _configuration["Jwt:ExpiryInHours"];

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "JWT Configuration",
                    Data = new { 
                        Key = jwtKey, // Show the full key for debugging
                        KeyLength = jwtKey?.Length ?? 0,
                        Issuer = jwtIssuer,
                        Audience = jwtAudience,
                        ExpiryInHours = jwtExpiry,
                        Instructions = "Check if the JWT key matches the one in appsettings.json"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing JWT configuration");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to test JWT configuration",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Test JWT token validation (public endpoint for development)
        /// </summary>
        /// <returns>Token validation result</returns>
        [HttpPost("test-token-validation")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> TestTokenValidation([FromBody] object request)
        {
            try
            {
                // Get the token from the request
                var token = "";
                if (request is System.Text.Json.JsonElement element && element.TryGetProperty("token", out var tokenElement))
                {
                    token = tokenElement.GetString() ?? "";
                }

                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Token is required",
                        Data = null
                    });
                }

                // Manually validate the token using the same configuration
                var jwtKey = _configuration["Jwt:Key"];
                var jwtIssuer = _configuration["Jwt:Issuer"];
                var jwtAudience = _configuration["Jwt:Audience"];

                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var key = System.Text.Encoding.UTF8.GetBytes(jwtKey);

                var validationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var result = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Token validation completed",
                    Data = new { 
                        IsValid = validatedToken != null,
                        Claims = result.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList(),
                        TokenType = validatedToken?.GetType().Name ?? "Invalid"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing token validation");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to validate token",
                    Data = new { Error = ex.Message }
                });
            }
        }

        /// <summary>
        /// Create test users with different roles (public endpoint for development)
        /// </summary>
        /// <returns>Success status</returns>
        [HttpPost("create-test-users")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> CreateTestUsers()
        {
            try
            {
                var results = new List<object>();
                var context = _authorizationService.GetDbContext();

                // Helper function to create or update user
                async Task<object> CreateOrUpdateUser(string username, string email, string password, string roleName)
                {
                    var existingUser = await _authorizationService.GetUserByUsernameAsync(username);
                    
                    if (existingUser == null)
                    {
                        // Create new user
                        var createUserDto = new CreateUserDto
                        {
                            Username = username,
                            Email = email,
                            Password = password
                        };
                        await _authorizationService.CreateUserAsync(createUserDto);
                        existingUser = await _authorizationService.GetUserByUsernameAsync(username);
                    }
                    else
                    {
                        // Update password for existing user
                        using var sha256 = System.Security.Cryptography.SHA256.Create();
                        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                        var passwordHash = Convert.ToBase64String(hashedBytes);
                        
                        var user = await context.Users.FindAsync(existingUser.Id);
                        if (user != null)
                        {
                            user.PasswordHash = passwordHash;
                            await context.SaveChangesAsync();
                        }
                    }

                    // Assign role
                    var role = await _authorizationService.GetRoleByNameAsync(roleName);
                    if (existingUser != null && role != null)
                    {
                        // Remove existing roles first
                        var existingRoles = await _authorizationService.GetUserRolesAsync(existingUser.Id);
                        if (existingRoles.Any())
                        {
                            await _authorizationService.RemoveRolesFromUserAsync(new RemoveRolesFromUserDto
                            {
                                UserId = existingUser.Id,
                                RoleIds = existingRoles.Select(r => r.Id).ToList()
                            });
                        }

                        // Assign new role
                        await _authorizationService.AssignRolesToUserAsync(new AssignRolesToUserDto
                        {
                            UserId = existingUser.Id,
                            RoleIds = new List<int> { role.Id }
                        });
                    }

                    return new { Username = username, Password = password, Role = roleName, Status = existingUser == null ? "Created" : "Updated" };
                }

                // Create/Update Admin user
                var adminResult = await CreateOrUpdateUser("admin", "admin@xyzuniversity.edu", "Admin123!", "Admin");
                results.Add(adminResult);

                // Create/Update Manager user
                var managerResult = await CreateOrUpdateUser("manager", "manager@xyzuniversity.edu", "Manager123!", "Manager");
                results.Add(managerResult);

                // Create/Update Staff user
                var staffResult = await CreateOrUpdateUser("staff", "staff@xyzuniversity.edu", "Staff123!", "Staff");
                results.Add(staffResult);

                // Create/Update Student user
                var studentResult = await CreateOrUpdateUser("student", "student@xyzuniversity.edu", "Student123!", "Student");
                results.Add(studentResult);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Test users created/updated successfully",
                    Data = new { 
                        Users = results,
                        Instructions = "Use these credentials to test different permission levels"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test users");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to create test users: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get user permissions and roles for testing (public endpoint for development)
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns>User permissions and roles</returns>
        [HttpGet("test-user-info/{username}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> GetTestUserInfo(string username)
        {
            try
            {
                var user = await _authorizationService.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"User '{username}' not found",
                        Data = null
                    });
                }

                var userRoles = await _authorizationService.GetUserRolesAsync(user.Id);
                var userPermissions = await _authorizationService.GetUserPermissionsAsync(username);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"User info for {username}",
                    Data = new { 
                        Username = username,
                        UserId = user.Id,
                        IsActive = user.IsActive,
                        Roles = userRoles.Select(r => r.Name).ToList(),
                        Permissions = userPermissions.ToList(),
                        PermissionCount = userPermissions.Count()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting test user info");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to get user info: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Test endpoint for Manager role
        /// </summary>
        [HttpGet("manager-test")]
        [Authorize(Roles = "Manager")]
        public IActionResult ManagerTest()
        {
            var user = User;
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
            var permissions = user.FindAll("permission").Select(c => c.Value);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Manager access granted",
                Data = new
                {
                    Username = user.FindFirst(ClaimTypes.Name)?.Value,
                    Roles = roles.ToList(),
                    Permissions = permissions.ToList(),
                    Timestamp = DateTime.UtcNow
                }
            });
        }

        /// <summary>
        /// Test endpoint for Staff role
        /// </summary>
        [HttpGet("staff-test")]
        [Authorize(Roles = "Staff")]
        public IActionResult StaffTest()
        {
            var user = User;
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
            var permissions = user.FindAll("permission").Select(c => c.Value);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Staff access granted",
                Data = new
                {
                    Username = user.FindFirst(ClaimTypes.Name)?.Value,
                    Roles = roles.ToList(),
                    Permissions = permissions.ToList(),
                    Timestamp = DateTime.UtcNow
                }
            });
        }

        /// <summary>
        /// Test endpoint for Student role
        /// </summary>
        [HttpGet("student-role-test")]
        [Authorize(Roles = "Student")]
        public IActionResult StudentRoleTest()
        {
            var user = User;
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
            var permissions = user.FindAll("permission").Select(c => c.Value);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Student role access granted",
                Data = new
                {
                    Username = user.FindFirst(ClaimTypes.Name)?.Value,
                    Roles = roles.ToList(),
                    Permissions = permissions.ToList(),
                    Timestamp = DateTime.UtcNow
                }
            });
        }

        /// <summary>
        /// Test payment creation permission
        /// </summary>
        [HttpGet("test-payment-create")]
        [AuthorizePayment("Create")]
        public IActionResult TestPaymentCreate()
        {
            var user = User;
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
            var permissions = user.FindAll("permission").Select(c => c.Value);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Payment create permission granted",
                Data = new
                {
                    Username = user.FindFirst(ClaimTypes.Name)?.Value,
                    Roles = roles.ToList(),
                    Permissions = permissions.ToList(),
                    Timestamp = DateTime.UtcNow
                }
            });
        }

        /// <summary>
        /// Test student update permission
        /// </summary>
        [HttpGet("test-student-update")]
        [AuthorizeStudent("Update")]
        public IActionResult TestStudentUpdate()
        {
            var user = User;
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
            var permissions = user.FindAll("permission").Select(c => c.Value);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Student update permission granted",
                Data = new
                {
                    Username = user.FindFirst(ClaimTypes.Name)?.Value,
                    Roles = roles.ToList(),
                    Permissions = permissions.ToList(),
                    Timestamp = DateTime.UtcNow
                }
            });
        }

        /// <summary>
        /// Test user management permission
        /// </summary>
        [HttpGet("test-user-management")]
        [AuthorizeUserManagement("Read")]
        public IActionResult TestUserManagement()
        {
            var user = User;
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
            var permissions = user.FindAll("permission").Select(c => c.Value);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "User management permission granted",
                Data = new
                {
                    Username = user.FindFirst(ClaimTypes.Name)?.Value,
                    Roles = roles.ToList(),
                    Permissions = permissions.ToList(),
                    Timestamp = DateTime.UtcNow
                }
            });
        }

        /// <summary>
        /// Clean up corrupted data (public endpoint for development)
        /// </summary>
        /// <returns>Success status</returns>
        [HttpPost("cleanup-corrupted-data")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> CleanupCorruptedData()
        {
            try
            {
                var context = _authorizationService.GetDbContext();
                var cleanupResults = new List<string>();

                // Clean up roles with empty names
                var emptyRoles = await context.Roles.Where(r => string.IsNullOrWhiteSpace(r.Name)).ToListAsync();
                if (emptyRoles.Any())
                {
                    context.Roles.RemoveRange(emptyRoles);
                    cleanupResults.Add($"Removed {emptyRoles.Count} roles with empty names");
                }

                // Clean up permissions with empty resource or action
                var emptyPermissions = await context.Permissions.Where(p => 
                    string.IsNullOrWhiteSpace(p.Resource) || 
                    string.IsNullOrWhiteSpace(p.Action)).ToListAsync();
                if (emptyPermissions.Any())
                {
                    context.Permissions.RemoveRange(emptyPermissions);
                    cleanupResults.Add($"Removed {emptyPermissions.Count} permissions with empty resource/action");
                }

                // Clean up users with empty usernames
                var emptyUsers = await context.Users.Where(u => string.IsNullOrWhiteSpace(u.Username)).ToListAsync();
                if (emptyUsers.Any())
                {
                    context.Users.RemoveRange(emptyUsers);
                    cleanupResults.Add($"Removed {emptyUsers.Count} users with empty usernames");
                }

                // Clean up orphaned UserRoles
                var orphanedUserRoles = await context.UserRoles
                    .Where(ur => !context.Users.Any(u => u.Id == ur.UserId) || 
                                !context.Roles.Any(r => r.Id == ur.RoleId))
                    .ToListAsync();
                if (orphanedUserRoles.Any())
                {
                    context.UserRoles.RemoveRange(orphanedUserRoles);
                    cleanupResults.Add($"Removed {orphanedUserRoles.Count} orphaned user roles");
                }

                // Clean up orphaned RolePermissions
                var orphanedRolePermissions = await context.RolePermissions
                    .Where(rp => !context.Roles.Any(r => r.Id == rp.RoleId) || 
                                !context.Permissions.Any(p => p.Id == rp.PermissionId))
                    .ToListAsync();
                if (orphanedRolePermissions.Any())
                {
                    context.RolePermissions.RemoveRange(orphanedRolePermissions);
                    cleanupResults.Add($"Removed {orphanedRolePermissions.Count} orphaned role permissions");
                }

                await context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Corrupted data cleaned up successfully",
                    Data = new { 
                        CleanupResults = cleanupResults,
                        Instructions = "Now try creating test users again"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up corrupted data");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to cleanup corrupted data: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Check database state (public endpoint for development)
        /// </summary>
        /// <returns>Database state</returns>
        [HttpGet("check-database-state")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> CheckDatabaseState()
        {
            try
            {
                var context = _authorizationService.GetDbContext();
                
                var roles = await context.Roles.ToListAsync();
                var permissions = await context.Permissions.ToListAsync();
                var users = await context.Users.ToListAsync();
                var userRoles = await context.UserRoles.ToListAsync();
                var rolePermissions = await context.RolePermissions.ToListAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Database state retrieved",
                    Data = new { 
                        Roles = roles.Select(r => new { r.Id, r.Name, r.Description, r.IsActive }).ToList(),
                        Permissions = permissions.Select(p => new { p.Id, p.Resource, p.Action, p.IsActive }).ToList(),
                        Users = users.Select(u => new { u.Id, u.Username, u.Email, u.IsActive }).ToList(),
                        UserRoles = userRoles.Select(ur => new { ur.UserId, ur.RoleId }).ToList(),
                        RolePermissions = rolePermissions.Select(rp => new { rp.RoleId, rp.PermissionId }).ToList(),
                        Summary = new {
                            TotalRoles = roles.Count,
                            TotalPermissions = permissions.Count,
                            TotalUsers = users.Count,
                            TotalUserRoles = userRoles.Count,
                            TotalRolePermissions = rolePermissions.Count
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking database state");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to check database state: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Assign roles to existing users (public endpoint for development)
        /// </summary>
        /// <returns>Success status</returns>
        [HttpPost("assign-roles-to-existing-users")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> AssignRolesToExistingUsers()
        {
            try
            {
                var results = new List<object>();

                // Assign Manager role to manager user
                var managerUser = await _authorizationService.GetUserByUsernameAsync("manager");
                var managerRole = await _authorizationService.GetRoleByNameAsync("Manager");
                if (managerUser != null && managerRole != null)
                {
                    await _authorizationService.AssignRolesToUserAsync(new AssignRolesToUserDto
                    {
                        UserId = managerUser.Id,
                        RoleIds = new List<int> { managerRole.Id }
                    });
                    results.Add(new { Username = "manager", Role = "Manager", Status = "Assigned" });
                }

                // Assign Staff role to staff user
                var staffUser = await _authorizationService.GetUserByUsernameAsync("staff");
                var staffRole = await _authorizationService.GetRoleByNameAsync("Staff");
                if (staffUser != null && staffRole != null)
                {
                    await _authorizationService.AssignRolesToUserAsync(new AssignRolesToUserDto
                    {
                        UserId = staffUser.Id,
                        RoleIds = new List<int> { staffRole.Id }
                    });
                    results.Add(new { Username = "staff", Role = "Staff", Status = "Assigned" });
                }

                // Create and assign Student role to student user
                var studentUser = await _authorizationService.GetUserByUsernameAsync("student");
                if (studentUser != null)
                {
                    // Create student user if it doesn't exist
                    if (studentUser == null)
                    {
                        var createStudentDto = new CreateUserDto
                        {
                            Username = "student",
                            Email = "student@xyzuniversity.edu",
                            Password = "Student123!"
                        };
                        await _authorizationService.CreateUserAsync(createStudentDto);
                        studentUser = await _authorizationService.GetUserByUsernameAsync("student");
                    }

                    var studentRole = await _authorizationService.GetRoleByNameAsync("Student");
                    if (studentRole != null)
                    {
                        await _authorizationService.AssignRolesToUserAsync(new AssignRolesToUserDto
                        {
                            UserId = studentUser.Id,
                            RoleIds = new List<int> { studentRole.Id }
                        });
                        results.Add(new { Username = "student", Role = "Student", Status = "Assigned" });
                    }
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Roles assigned to existing users successfully",
                    Data = new { 
                        Results = results,
                        Instructions = "Users are now ready for testing"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning roles to existing users");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to assign roles: {ex.Message}",
                    Data = null
                });
            }
        }
    }
}
