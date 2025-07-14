using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using xyz_university_payment_api.Core.Application.Interfaces;
using xyz_university_payment_api.Core.Application.DTOs;
using xyz_university_payment_api.Core.Domain.Entities;
using xyz_university_payment_api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace xyz_university_payment_api.Presentation.Controllers.V3
{
    [ApiController]
    [Route("api/v3/user")]
    [Authorize]
    public class UserControllerV3 : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserControllerV3> _logger;

        public UserControllerV3(AppDbContext context, ILogger<UserControllerV3> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserManagementDto>>> GetUsers()
        {
            try
            {
                _logger.LogInformation("GetUsers endpoint called by user: {User}", User.Identity?.Name);
                
                // First, let's check how many users exist in total
                var totalUsers = await _context.Users.CountAsync();
                _logger.LogInformation("Total users in database: {TotalUsers}", totalUsers);
                
                var users = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .Select(u => new UserManagementDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Email = u.Email,
                        FullName = u.Username, // Using username as full name for now
                        IsActive = u.IsActive,
                        CurrentRoles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                        CreatedAt = u.CreatedAt,
                        IsStudent = _context.Students.Any(s => s.StudentNumber == u.Username) // Check if username matches a student number
                    })
                    .ToListAsync();

                _logger.LogInformation("Returning {UserCount} users", users.Count);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserManagementDto>> GetUser(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var userDto = new UserManagementDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.Username, // Using username as full name for now
                    IsActive = user.IsActive,
                    CurrentRoles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                    CreatedAt = user.CreatedAt
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("users")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserManagementDto>> CreateUser([FromBody] UserCreateDto createUserDto)
        {
            try
            {
                _logger.LogInformation("Creating user with username: {Username}", createUserDto.Username);

                // Validate required fields
                if (string.IsNullOrEmpty(createUserDto.Username) || string.IsNullOrEmpty(createUserDto.Email) || string.IsNullOrEmpty(createUserDto.Password))
                {
                    return BadRequest(new { message = "Username, email, and password are required" });
                }

                // Check if username already exists
                if (await _context.Users.AnyAsync(u => u.Username == createUserDto.Username))
                {
                    return BadRequest(new { message = "Username already exists" });
                }

                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == createUserDto.Email))
                {
                    return BadRequest(new { message = "Email already exists" });
                }

                // Hash password
                var passwordHash = HashPassword(createUserDto.Password);

                // Create user
                var user = new User
                {
                    Username = createUserDto.Username,
                    Email = createUserDto.Email,
                    PasswordHash = passwordHash,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Automatically assign role based on default role or username pattern
                string roleToAssign = createUserDto.DefaultRole;
                
                // If it's a student user (username starts with S and is 6 characters), force Student role
                if (createUserDto.Username.StartsWith("S") && createUserDto.Username.Length == 6)
                {
                    roleToAssign = "Student";
                }

                // Get the role
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleToAssign);
                if (role != null)
                {
                    // Assign role using raw SQL to avoid tracking issues
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO UserRoles (UserId, RoleId, AssignedAt, AssignedBy) VALUES ({0}, {1}, {2}, {3})",
                        user.Id, role.Id, DateTime.UtcNow, "System");

                    _logger.LogInformation("Automatically assigned role '{RoleName}' to user {Username}", roleToAssign, createUserDto.Username);
                }
                else
                {
                    _logger.LogWarning("Role '{RoleName}' not found for user {Username}", roleToAssign, createUserDto.Username);
                }

                // Get the created user with roles
                var createdUser = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == user.Id);

                var userDto = new UserManagementDto
                {
                    Id = createdUser.Id,
                    Username = createdUser.Username,
                    Email = createdUser.Email,
                    FullName = createUserDto.FullName,
                    IsActive = createdUser.IsActive,
                    CreatedAt = createdUser.CreatedAt,
                    CurrentRoles = createdUser.UserRoles.Select(ur => ur.Role.Name).ToList(),
                    IsStudent = createdUser.UserRoles.Any(ur => ur.Role.Name == "Student")
                };

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("users/{id}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AssignRole(int id, [FromBody] AssignRoleDto assignRoleDto)
        {
            try
            {
                _logger.LogInformation("Assigning role to user {UserId}. RoleName: '{RoleName}' (Length: {Length})", 
                    id, assignRoleDto.RoleName, assignRoleDto.RoleName?.Length ?? 0);

                // Validate role name
                if (string.IsNullOrEmpty(assignRoleDto.RoleName) || string.IsNullOrWhiteSpace(assignRoleDto.RoleName))
                {
                    _logger.LogWarning("Empty or null role name provided for user {UserId}", id);
                    return BadRequest(new { message = "Role name cannot be empty" });
                }

                // Use raw SQL to avoid entity tracking issues
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Check if user exists using raw SQL
                    var userExists = await _context.Users
                        .FromSqlRaw("SELECT Id FROM Users WHERE Id = {0}", id)
                        .AnyAsync();
                    
                    if (!userExists)
                    {
                        await transaction.RollbackAsync();
                        return NotFound(new { message = "User not found" });
                    }

                    // Check if role exists using raw SQL
                    var roleExists = await _context.Roles
                        .FromSqlRaw("SELECT Id FROM Roles WHERE Name = {0}", assignRoleDto.RoleName)
                        .AnyAsync();
                    
                    if (!roleExists)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning("Role '{RoleName}' not found in database", assignRoleDto.RoleName);
                        return BadRequest(new { message = $"Role '{assignRoleDto.RoleName}' not found" });
                    }

                    // Get role ID
                    var role = await _context.Roles
                        .FromSqlRaw("SELECT Id FROM Roles WHERE Name = {0}", assignRoleDto.RoleName)
                        .FirstOrDefaultAsync();

                    // Remove existing roles using raw SQL
                    await _context.Database.ExecuteSqlRawAsync(
                        "DELETE FROM UserRoles WHERE UserId = {0}", id);

                    // Add new role using raw SQL
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO UserRoles (UserId, RoleId, AssignedAt, AssignedBy) VALUES ({0}, {1}, {2}, {3})",
                        id, role.Id, DateTime.UtcNow, User.Identity?.Name ?? "System");

                    await transaction.CommitAsync();

                    _logger.LogInformation("Role '{RoleName}' successfully assigned to user {UserId}", assignRoleDto.RoleName, id);
                    return Ok(new { message = "Role assigned successfully" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role to user {UserId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("users/{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ToggleUserStatus(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = $"User {(user.IsActive ? "activated" : "deactivated")} successfully",
                    isActive = user.IsActive
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling user status for user {UserId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("roles")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserRoleDto>>> GetRoles()
        {
            try
            {
                // Clean up any empty role names first
                var emptyRoles = await _context.Roles.Where(r => string.IsNullOrEmpty(r.Name) || string.IsNullOrWhiteSpace(r.Name)).ToListAsync();
                if (emptyRoles.Any())
                {
                    _logger.LogWarning("Found {Count} empty role names, removing them", emptyRoles.Count);
                    _context.Roles.RemoveRange(emptyRoles);
                    await _context.SaveChangesAsync();
                }

                var roles = await _context.Roles
                    .Select(r => new UserRoleDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Description = r.Description
                    })
                    .ToListAsync();

                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("cleanup-database")]
        [AllowAnonymous]
        public async Task<ActionResult> CleanupDatabase()
        {
            try
            {
                _logger.LogInformation("Starting database cleanup...");

                // Clean up empty roles
                var emptyRoles = await _context.Roles.Where(r => string.IsNullOrEmpty(r.Name) || string.IsNullOrWhiteSpace(r.Name)).ToListAsync();
                if (emptyRoles.Any())
                {
                    _logger.LogWarning("Found {Count} empty role names, removing them", emptyRoles.Count);
                    _context.Roles.RemoveRange(emptyRoles);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Removed {Count} empty roles", emptyRoles.Count);
                }

                // Clean up orphaned UserRoles
                var orphanedUserRoles = await _context.UserRoles
                    .Where(ur => !_context.Users.Any(u => u.Id == ur.UserId) || 
                                !_context.Roles.Any(r => r.Id == ur.RoleId))
                    .ToListAsync();
                if (orphanedUserRoles.Any())
                {
                    _logger.LogWarning("Found {Count} orphaned user roles, removing them", orphanedUserRoles.Count);
                    _context.UserRoles.RemoveRange(orphanedUserRoles);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Removed {Count} orphaned user roles", orphanedUserRoles.Count);
                }

                // Fix empty emails
                var usersWithEmptyEmails = await _context.Users
                    .Where(u => string.IsNullOrEmpty(u.Email) || string.IsNullOrWhiteSpace(u.Email))
                    .ToListAsync();
                
                if (usersWithEmptyEmails.Any())
                {
                    _logger.LogWarning("Found {Count} users with empty emails, fixing them", usersWithEmptyEmails.Count);
                    foreach (var user in usersWithEmptyEmails)
                    {
                        user.Email = $"{user.Username}@xyzuniversity.edu";
                    }
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Fixed {Count} empty emails", usersWithEmptyEmails.Count);
                }

                // Show current state
                var roles = await _context.Roles.ToListAsync();
                var userRoles = await _context.UserRoles
                    .Include(ur => ur.User)
                    .Include(ur => ur.Role)
                    .ToListAsync();

                return Ok(new { 
                    message = "Database cleanup completed successfully",
                    roles = roles.Select(r => new { r.Id, r.Name, r.Description }),
                    userRoles = userRoles.Select(ur => new { 
                        UserId = ur.UserId, 
                        Username = ur.User?.Username, 
                        RoleId = ur.RoleId, 
                        RoleName = ur.Role?.Name 
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database cleanup");
                return StatusCode(500, new { message = "Database cleanup failed", error = ex.Message });
            }
        }

        [HttpGet("debug-users")]
        [AllowAnonymous]
        public async Task<ActionResult> DebugUsers()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .ToListAsync();

                var roles = await _context.Roles.ToListAsync();

                var result = new
                {
                    TotalUsers = users.Count,
                    TotalRoles = roles.Count,
                    Roles = roles.Select(r => new { r.Id, r.Name, r.Description }),
                    Users = users.Select(u => new
                    {
                        u.Id,
                        u.Username,
                        u.Email,
                        u.IsActive,
                        UserRoles = u.UserRoles.Select(ur => new { ur.RoleId, RoleName = ur.Role?.Name })
                    })
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting debug info");
                return StatusCode(500, new { message = "Error getting debug info", error = ex.Message });
            }
        }

        [HttpPost("test-assign-role")]
        [AllowAnonymous]
        public async Task<ActionResult> TestAssignRole([FromBody] TestAssignRoleDto testDto)
        {
            try
            {
                _logger.LogInformation("Testing role assignment for user {UserId} with role {RoleName}", testDto.UserId, testDto.RoleName);

                // Check if user exists
                var user = await _context.Users.FindAsync(testDto.UserId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Check if role exists
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == testDto.RoleName);
                if (role == null)
                {
                    return BadRequest(new { message = $"Role '{testDto.RoleName}' not found" });
                }

                // Remove existing roles
                var existingUserRoles = await _context.UserRoles.Where(ur => ur.UserId == testDto.UserId).ToListAsync();
                _context.UserRoles.RemoveRange(existingUserRoles);

                // Assign new role
                var userRole = new UserRole
                {
                    UserId = testDto.UserId,
                    RoleId = role.Id,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = "Test"
                };

                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Role assigned successfully",
                    userId = testDto.UserId,
                    roleId = role.Id,
                    roleName = role.Name
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test role assignment");
                return StatusCode(500, new { message = "Test role assignment failed", error = ex.Message });
            }
        }

        [HttpPost("fix-student-roles")]
        [AllowAnonymous]
        public async Task<ActionResult> FixStudentRoles()
        {
            try
            {
                _logger.LogInformation("Starting student role fix...");

                // Get the Student role
                var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Student");
                if (studentRole == null)
                {
                    return BadRequest(new { message = "Student role not found in database" });
                }

                // Get all users with student number usernames (S12345, S67890, etc.)
                var allUsers = await _context.Users.ToListAsync();
                var studentUsers = allUsers.Where(u => u.Username.StartsWith("S") && u.Username.Length == 6).ToList();

                var fixedUsers = new List<object>();

                foreach (var user in studentUsers)
                {
                    try
                    {
                        // Check if user already has Student role
                        var existingUserRoles = await _context.UserRoles
                            .Where(ur => ur.UserId == user.Id)
                            .Include(ur => ur.Role)
                            .ToListAsync();
                        
                        var hasStudentRole = existingUserRoles.Any(ur => ur.RoleId == studentRole.Id);
                        
                        if (!hasStudentRole)
                        {
                            // Remove any existing roles
                            _context.UserRoles.RemoveRange(existingUserRoles);

                            // Assign Student role
                            var userRole = new UserRole
                            {
                                UserId = user.Id,
                                RoleId = studentRole.Id,
                                AssignedAt = DateTime.UtcNow,
                                AssignedBy = "System"
                            };

                            _context.UserRoles.Add(userRole);
                            await _context.SaveChangesAsync();

                            fixedUsers.Add(new { 
                                Username = user.Username, 
                                Action = "Assigned Student role",
                                PreviousRoles = existingUserRoles.Select(ur => ur.Role?.Name ?? "Unknown").ToList()
                            });

                            _logger.LogInformation("Assigned Student role to user {Username}", user.Username);
                        }
                        else
                        {
                            fixedUsers.Add(new { 
                                Username = user.Username, 
                                Action = "Already has Student role",
                                CurrentRoles = existingUserRoles.Select(ur => ur.Role?.Name ?? "Unknown").ToList()
                            });
                        }
                    }
                    catch (Exception userEx)
                    {
                        _logger.LogError(userEx, "Error processing user {Username}", user.Username);
                        fixedUsers.Add(new { 
                            Username = user.Username, 
                            Action = "Error processing user",
                            Error = userEx.Message
                        });
                    }
                }

                return Ok(new { 
                    message = "Student role fix completed",
                    fixedUsers = fixedUsers,
                    totalStudentUsers = studentUsers.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing student roles");
                return StatusCode(500, new { message = "Student role fix failed", error = ex.Message });
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }

    public class UserManagementDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public List<string> CurrentRoles { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public bool IsStudent { get; set; }
    }

    public class UserCreateDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string DefaultRole { get; set; } = "Student";
        public int? SelectedStudentId { get; set; }
    }

    public class AssignRoleDto
    {
        public string RoleName { get; set; }
    }

    public class UserRoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class TestAssignRoleDto
    {
        public int UserId { get; set; }
        public string RoleName { get; set; }
    }
} 