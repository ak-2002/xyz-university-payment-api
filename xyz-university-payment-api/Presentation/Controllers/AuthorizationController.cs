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

        public AuthorizationController(
            xyz_university_payment_api.Core.Application.Interfaces.IAuthorizationService authorizationService,
            IJwtTokenService jwtTokenService,
            ILogger<AuthorizationController> logger)
        {
            _authorizationService = authorizationService;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
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
        [AuthorizeUserManagement("read")]
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
        [AuthorizeUserManagement("read")]
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
        [AuthorizePayment("read")]
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
        [AuthorizeStudent("read")]
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
                    Password = "Admin123!"
                };

                var result = await _authorizationService.CreateUserAsync(createUserDto);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Admin user created successfully. Username: admin, Password: Admin123!",
                    Data = new { Username = "admin", Password = "Admin123!" }
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
    }
} 
