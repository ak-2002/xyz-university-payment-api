// Purpose: Authorization Service implementation for role-based access control
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using xyz_university_payment_api.Data;
using xyz_university_payment_api.DTOs;
using xyz_university_payment_api.Exceptions;
using xyz_university_payment_api.Interfaces;
using xyz_university_payment_api.Models;

namespace xyz_university_payment_api.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly AppDbContext _context;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AuthorizationService> _logger;
        private readonly IConfiguration _configuration;

        public AuthorizationService(
            AppDbContext context,
            IJwtTokenService jwtTokenService,
            ILogger<AuthorizationService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
            _configuration = configuration;
        }

        #region User Management

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == createUserDto.Username))
            {
                throw new ValidationException("Username already exists", new List<string> { "Username must be unique" });
            }

            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == createUserDto.Email))
            {
                throw new ValidationException("Email already exists", new List<string> { "Email must be unique" });
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

            // Assign roles if provided
            if (createUserDto.RoleIds.Any())
            {
                await AssignRolesToUserAsync(new AssignRolesToUserDto
                {
                    UserId = user.Id,
                    RoleIds = createUserDto.RoleIds
                });
            }

            // Log the action
            await LogAuthorizationActionAsync("UserCreated", "User", user.Id.ToString(), "System");

            return await GetUserByIdAsync(user.Id) ?? throw new DatabaseException("Failed to retrieve created user");
        }

        public async Task<UserDto> UpdateUserAsync(int userId, UpdateUserDto updateUserDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new NotFoundException($"User with ID {userId} not found");
            }

            // Check if username is being changed and if it already exists
            if (updateUserDto.Username != user.Username && 
                await _context.Users.AnyAsync(u => u.Username == updateUserDto.Username))
            {
                throw new ValidationException("Username already exists", new List<string> { "Username must be unique" });
            }

            // Check if email is being changed and if it already exists
            if (updateUserDto.Email != user.Email && 
                await _context.Users.AnyAsync(u => u.Email == updateUserDto.Email))
            {
                throw new ValidationException("Email already exists", new List<string> { "Email must be unique" });
            }

            // Update user properties
            user.Username = updateUserDto.Username;
            user.Email = updateUserDto.Email;
            user.IsActive = updateUserDto.IsActive;

            await _context.SaveChangesAsync();

            // Update roles if provided
            if (updateUserDto.RoleIds.Any())
            {
                // Remove existing roles
                var existingUserRoles = await _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .ToListAsync();
                _context.UserRoles.RemoveRange(existingUserRoles);

                // Assign new roles
                await AssignRolesToUserAsync(new AssignRolesToUserDto
                {
                    UserId = userId,
                    RoleIds = updateUserDto.RoleIds
                });
            }

            // Log the action
            await LogAuthorizationActionAsync("UserUpdated", "User", userId.ToString(), "System");

            return await GetUserByIdAsync(userId) ?? throw new DatabaseException("Failed to retrieve updated user");
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = user.UserRoles.Select(ur => new RoleDto
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name,
                    Description = ur.Role.Description,
                    IsActive = ur.Role.IsActive,
                    CreatedAt = ur.Role.CreatedAt,
                    Permissions = new List<PermissionDto>() // Will be populated separately if needed
                }).ToList()
            };
        }

        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = user.UserRoles.Select(ur => new RoleDto
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name,
                    Description = ur.Role.Description,
                    IsActive = ur.Role.IsActive,
                    CreatedAt = ur.Role.CreatedAt,
                    Permissions = new List<PermissionDto>()
                }).ToList()
            };
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();

            return users.Select(user => new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = user.UserRoles.Select(ur => new RoleDto
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name,
                    Description = ur.Role.Description,
                    IsActive = ur.Role.IsActive,
                    CreatedAt = ur.Role.CreatedAt,
                    Permissions = new List<PermissionDto>()
                }).ToList()
            });
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new NotFoundException($"User with ID {userId} not found");
            }

            // Remove user roles
            var userRoles = await _context.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
            _context.UserRoles.RemoveRange(userRoles);

            // Remove user
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            // Log the action
            await LogAuthorizationActionAsync("UserDeleted", "User", userId.ToString(), "System");

            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new NotFoundException($"User with ID {userId} not found");
            }

            // Verify current password
            if (!VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                throw new ValidationException("Current password is incorrect", new List<string> { "CurrentPassword" });
            }

            // Verify new password confirmation
            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
            {
                throw new ValidationException("New password and confirmation do not match", new List<string> { "ConfirmPassword" });
            }

            // Hash new password
            user.PasswordHash = HashPassword(changePasswordDto.NewPassword);
            await _context.SaveChangesAsync();

            // Log the action
            await LogAuthorizationActionAsync("PasswordChanged", "User", userId.ToString(), "System");

            return true;
        }

        #endregion

        #region Authentication

        public async Task<UserLoginResponseDto?> AuthenticateAsync(UserLoginDto loginDto)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return null;
            }

            if (!user.IsActive)
            {
                throw new ForbiddenException("User account is deactivated");
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate tokens
            var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user);
            var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync();

            // Get user permissions
            var permissions = await GetUserPermissionsAsync(user.Username);

            return new UserLoginResponseDto
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = await GetUserByIdAsync(user.Id) ?? new UserDto(),
                Permissions = permissions.ToList()
            };
        }

        public async Task<UserLoginResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            // In a real application, you would validate the refresh token against the database
            // For now, we'll throw an exception as this requires additional implementation
            throw new NotImplementedException("Refresh token functionality requires database implementation");
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            return await _jwtTokenService.RevokeRefreshTokenAsync(refreshToken);
        }

        #endregion

        #region Role Management

        public async Task<RoleDto> CreateRoleAsync(CreateRoleDto createRoleDto)
        {
            // Check if role name already exists
            if (await _context.Roles.AnyAsync(r => r.Name == createRoleDto.Name))
            {
                throw new ValidationException("Role name already exists", new List<string> { "Name must be unique" });
            }

            var role = new Role
            {
                Name = createRoleDto.Name,
                Description = createRoleDto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            // Assign permissions if provided
            if (createRoleDto.PermissionIds.Any())
            {
                await AssignPermissionsToRoleAsync(new AssignPermissionsToRoleDto
                {
                    RoleId = role.Id,
                    PermissionIds = createRoleDto.PermissionIds
                });
            }

            // Log the action
            await LogAuthorizationActionAsync("RoleCreated", "Role", role.Id.ToString(), "System");

            return await GetRoleByIdAsync(role.Id) ?? throw new DatabaseException("Failed to retrieve created role");
        }

        public async Task<RoleDto> UpdateRoleAsync(int roleId, UpdateRoleDto updateRoleDto)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
            {
                throw new NotFoundException($"Role with ID {roleId} not found");
            }

            // Check if role name is being changed and if it already exists
            if (updateRoleDto.Name != role.Name && 
                await _context.Roles.AnyAsync(r => r.Name == updateRoleDto.Name))
            {
                throw new ValidationException("Role name already exists", new List<string> { "Name must be unique" });
            }

            role.Name = updateRoleDto.Name;
            role.Description = updateRoleDto.Description;
            role.IsActive = updateRoleDto.IsActive;

            await _context.SaveChangesAsync();

            // Update permissions if provided
            if (updateRoleDto.PermissionIds.Any())
            {
                // Remove existing permissions
                var existingRolePermissions = await _context.RolePermissions
                    .Where(rp => rp.RoleId == roleId)
                    .ToListAsync();
                _context.RolePermissions.RemoveRange(existingRolePermissions);

                // Assign new permissions
                await AssignPermissionsToRoleAsync(new AssignPermissionsToRoleDto
                {
                    RoleId = roleId,
                    PermissionIds = updateRoleDto.PermissionIds
                });
            }

            // Log the action
            await LogAuthorizationActionAsync("RoleUpdated", "Role", roleId.ToString(), "System");

            return await GetRoleByIdAsync(roleId) ?? throw new DatabaseException("Failed to retrieve updated role");
        }

        public async Task<RoleDto?> GetRoleByIdAsync(int roleId)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null) return null;

            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsActive = role.IsActive,
                CreatedAt = role.CreatedAt,
                Permissions = role.RolePermissions.Select(rp => new PermissionDto
                {
                    Id = rp.Permission.Id,
                    Name = rp.Permission.Name,
                    Description = rp.Permission.Description,
                    Resource = rp.Permission.Resource,
                    Action = rp.Permission.Action,
                    IsActive = rp.Permission.IsActive,
                    CreatedAt = rp.Permission.CreatedAt
                }).ToList()
            };
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _context.Roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .ToListAsync();

            return roles.Select(role => new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsActive = role.IsActive,
                CreatedAt = role.CreatedAt,
                Permissions = role.RolePermissions.Select(rp => new PermissionDto
                {
                    Id = rp.Permission.Id,
                    Name = rp.Permission.Name,
                    Description = rp.Permission.Description,
                    Resource = rp.Permission.Resource,
                    Action = rp.Permission.Action,
                    IsActive = rp.Permission.IsActive,
                    CreatedAt = rp.Permission.CreatedAt
                }).ToList()
            });
        }

        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
            {
                throw new NotFoundException($"Role with ID {roleId} not found");
            }

            // Remove role permissions
            var rolePermissions = await _context.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync();
            _context.RolePermissions.RemoveRange(rolePermissions);

            // Remove user roles
            var userRoles = await _context.UserRoles.Where(ur => ur.RoleId == roleId).ToListAsync();
            _context.UserRoles.RemoveRange(userRoles);

            // Remove role
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            // Log the action
            await LogAuthorizationActionAsync("RoleDeleted", "Role", roleId.ToString(), "System");

            return true;
        }

        #endregion

        #region Permission Management

        public async Task<PermissionDto> CreatePermissionAsync(CreatePermissionDto createPermissionDto)
        {
            // Check if permission already exists for the same resource and action
            if (await _context.Permissions.AnyAsync(p => 
                p.Resource == createPermissionDto.Resource && 
                p.Action == createPermissionDto.Action))
            {
                throw new ValidationException("Permission already exists for this resource and action", 
                    new List<string> { "Resource and Action combination must be unique" });
            }

            var permission = new Permission
            {
                Name = createPermissionDto.Name,
                Description = createPermissionDto.Description,
                Resource = createPermissionDto.Resource,
                Action = createPermissionDto.Action,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            // Log the action
            await LogAuthorizationActionAsync("PermissionCreated", "Permission", permission.Id.ToString(), "System");

            return await GetPermissionByIdAsync(permission.Id) ?? throw new DatabaseException("Failed to retrieve created permission");
        }

        public async Task<PermissionDto> UpdatePermissionAsync(int permissionId, UpdatePermissionDto updatePermissionDto)
        {
            var permission = await _context.Permissions.FindAsync(permissionId);
            if (permission == null)
            {
                throw new NotFoundException($"Permission with ID {permissionId} not found");
            }

            // Check if permission is being changed and if it already exists
            if ((updatePermissionDto.Resource != permission.Resource || updatePermissionDto.Action != permission.Action) &&
                await _context.Permissions.AnyAsync(p => 
                    p.Resource == updatePermissionDto.Resource && 
                    p.Action == updatePermissionDto.Action &&
                    p.Id != permissionId))
            {
                throw new ValidationException("Permission already exists for this resource and action", 
                    new List<string> { "Resource and Action combination must be unique" });
            }

            permission.Name = updatePermissionDto.Name;
            permission.Description = updatePermissionDto.Description;
            permission.Resource = updatePermissionDto.Resource;
            permission.Action = updatePermissionDto.Action;
            permission.IsActive = updatePermissionDto.IsActive;

            await _context.SaveChangesAsync();

            // Log the action
            await LogAuthorizationActionAsync("PermissionUpdated", "Permission", permissionId.ToString(), "System");

            return await GetPermissionByIdAsync(permissionId) ?? throw new DatabaseException("Failed to retrieve updated permission");
        }

        public async Task<PermissionDto?> GetPermissionByIdAsync(int permissionId)
        {
            var permission = await _context.Permissions.FindAsync(permissionId);
            if (permission == null) return null;

            return new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Description = permission.Description,
                Resource = permission.Resource,
                Action = permission.Action,
                IsActive = permission.IsActive,
                CreatedAt = permission.CreatedAt
            };
        }

        public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
        {
            var permissions = await _context.Permissions.ToListAsync();

            return permissions.Select(permission => new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Description = permission.Description,
                Resource = permission.Resource,
                Action = permission.Action,
                IsActive = permission.IsActive,
                CreatedAt = permission.CreatedAt
            });
        }

        public async Task<bool> DeletePermissionAsync(int permissionId)
        {
            var permission = await _context.Permissions.FindAsync(permissionId);
            if (permission == null)
            {
                throw new NotFoundException($"Permission with ID {permissionId} not found");
            }

            // Remove role permissions
            var rolePermissions = await _context.RolePermissions.Where(rp => rp.PermissionId == permissionId).ToListAsync();
            _context.RolePermissions.RemoveRange(rolePermissions);

            // Remove permission
            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();

            // Log the action
            await LogAuthorizationActionAsync("PermissionDeleted", "Permission", permissionId.ToString(), "System");

            return true;
        }

        #endregion

        #region User-Role Assignments

        public async Task<bool> AssignRolesToUserAsync(AssignRolesToUserDto assignRolesDto)
        {
            var user = await _context.Users.FindAsync(assignRolesDto.UserId);
            if (user == null)
            {
                throw new NotFoundException($"User with ID {assignRolesDto.UserId} not found");
            }

            // Verify all roles exist
            var roles = await _context.Roles
                .Where(r => assignRolesDto.RoleIds.Contains(r.Id))
                .ToListAsync();

            if (roles.Count != assignRolesDto.RoleIds.Count)
            {
                throw new ValidationException("One or more roles not found", new List<string> { "RoleIds" });
            }

            // Remove existing roles for this user
            var existingUserRoles = await _context.UserRoles
                .Where(ur => ur.UserId == assignRolesDto.UserId)
                .ToListAsync();
            _context.UserRoles.RemoveRange(existingUserRoles);

            // Add new role assignments
            var userRoles = assignRolesDto.RoleIds.Select(roleId => new UserRole
            {
                UserId = assignRolesDto.UserId,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = "System"
            });

            _context.UserRoles.AddRange(userRoles);
            await _context.SaveChangesAsync();

            // Log the action
            await LogAuthorizationActionAsync("RolesAssignedToUser", "User", assignRolesDto.UserId.ToString(), "System");

            return true;
        }

        public async Task<bool> RemoveRolesFromUserAsync(RemoveRolesFromUserDto removeRolesDto)
        {
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == removeRolesDto.UserId && removeRolesDto.RoleIds.Contains(ur.RoleId))
                .ToListAsync();

            _context.UserRoles.RemoveRange(userRoles);
            await _context.SaveChangesAsync();

            // Log the action
            await LogAuthorizationActionAsync("RolesRemovedFromUser", "User", removeRolesDto.UserId.ToString(), "System");

            return true;
        }

        public async Task<IEnumerable<RoleDto>> GetUserRolesAsync(int userId)
        {
            var userRoles = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId)
                .ToListAsync();

            return userRoles.Select(ur => new RoleDto
            {
                Id = ur.Role.Id,
                Name = ur.Role.Name,
                Description = ur.Role.Description,
                IsActive = ur.Role.IsActive,
                CreatedAt = ur.Role.CreatedAt,
                Permissions = new List<PermissionDto>()
            });
        }

        #endregion

        #region Role-Permission Assignments

        public async Task<bool> AssignPermissionsToRoleAsync(AssignPermissionsToRoleDto assignPermissionsDto)
        {
            var role = await _context.Roles.FindAsync(assignPermissionsDto.RoleId);
            if (role == null)
            {
                throw new NotFoundException($"Role with ID {assignPermissionsDto.RoleId} not found");
            }

            // Verify all permissions exist
            var permissions = await _context.Permissions
                .Where(p => assignPermissionsDto.PermissionIds.Contains(p.Id))
                .ToListAsync();

            if (permissions.Count != assignPermissionsDto.PermissionIds.Count)
            {
                throw new ValidationException("One or more permissions not found", new List<string> { "PermissionIds" });
            }

            // Remove existing permissions for this role
            var existingRolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == assignPermissionsDto.RoleId)
                .ToListAsync();
            _context.RolePermissions.RemoveRange(existingRolePermissions);

            // Add new permission assignments
            var rolePermissions = assignPermissionsDto.PermissionIds.Select(permissionId => new RolePermission
            {
                RoleId = assignPermissionsDto.RoleId,
                PermissionId = permissionId,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = "System"
            });

            _context.RolePermissions.AddRange(rolePermissions);
            await _context.SaveChangesAsync();

            // Log the action
            await LogAuthorizationActionAsync("PermissionsAssignedToRole", "Role", assignPermissionsDto.RoleId.ToString(), "System");

            return true;
        }

        public async Task<bool> RemovePermissionsFromRoleAsync(RemovePermissionsFromRoleDto removePermissionsDto)
        {
            var rolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == removePermissionsDto.RoleId && removePermissionsDto.PermissionIds.Contains(rp.PermissionId))
                .ToListAsync();

            _context.RolePermissions.RemoveRange(rolePermissions);
            await _context.SaveChangesAsync();

            // Log the action
            await LogAuthorizationActionAsync("PermissionsRemovedFromRole", "Role", removePermissionsDto.RoleId.ToString(), "System");

            return true;
        }

        public async Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(int roleId)
        {
            var rolePermissions = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            return rolePermissions.Select(rp => new PermissionDto
            {
                Id = rp.Permission.Id,
                Name = rp.Permission.Name,
                Description = rp.Permission.Description,
                Resource = rp.Permission.Resource,
                Action = rp.Permission.Action,
                IsActive = rp.Permission.IsActive,
                CreatedAt = rp.Permission.CreatedAt
            });
        }

        #endregion

        #region Authorization Checks

        public async Task<bool> HasPermissionAsync(string username, string resource, string action)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !user.IsActive)
            {
                return false;
            }

            // Check if user has the required permission through any of their roles
            return user.UserRoles.Any(ur => 
                ur.Role.IsActive && 
                ur.Role.RolePermissions.Any(rp => 
                    rp.Permission.IsActive && 
                    rp.Permission.Resource == resource && 
                    rp.Permission.Action == action));
        }

        public async Task<PermissionCheckResultDto> CheckPermissionAsync(CheckPermissionDto checkPermissionDto)
        {
            var hasPermission = await HasPermissionAsync(
                checkPermissionDto.Username, 
                checkPermissionDto.Resource, 
                checkPermissionDto.Action);

            var userRoles = await GetUserRolesAsync(checkPermissionDto.Username);
            var userPermissions = await GetUserPermissionsAsync(checkPermissionDto.Username);

            return new PermissionCheckResultDto
            {
                HasPermission = hasPermission,
                Username = checkPermissionDto.Username,
                Resource = checkPermissionDto.Resource,
                Action = checkPermissionDto.Action,
                UserRoles = userRoles.ToList(),
                UserPermissions = userPermissions.ToList()
            };
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(string username)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !user.IsActive)
            {
                return Enumerable.Empty<string>();
            }

            return user.UserRoles
                .Where(ur => ur.Role.IsActive)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Where(rp => rp.Permission.IsActive)
                .Select(rp => $"{rp.Permission.Resource}.{rp.Permission.Action}")
                .Distinct()
                .ToList();
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(string username)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !user.IsActive)
            {
                return Enumerable.Empty<string>();
            }

            return user.UserRoles
                .Where(ur => ur.Role.IsActive)
                .Select(ur => ur.Role.Name)
                .ToList();
        }

        #endregion

        #region Audit Logging

        public async Task LogAuthorizationActionAsync(string action, string entityType, string entityId, string performedBy, string? oldValues = null, string? newValues = null)
        {
            var auditLog = new AuthorizationAuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                PerformedBy = performedBy,
                Timestamp = DateTime.UtcNow,
                OldValues = oldValues,
                NewValues = newValues
            };

            _context.AuthorizationAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuthorizationAuditLogDto>> GetAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null, string? action = null, string? entityType = null)
        {
            var query = _context.AuthorizationAuditLogs.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(log => log.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(log => log.Timestamp <= endDate.Value);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(log => log.Action == action);

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(log => log.EntityType == entityType);

            var logs = await query
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();

            return logs.Select(log => new AuthorizationAuditLogDto
            {
                Id = log.Id,
                Action = log.Action,
                EntityType = log.EntityType,
                EntityId = log.EntityId,
                PerformedBy = log.PerformedBy,
                Timestamp = log.Timestamp,
                OldValues = log.OldValues,
                NewValues = log.NewValues,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent
            });
        }

        #endregion

        #region Seed Data

        public async Task SeedDefaultRolesAndPermissionsAsync()
        {
            // Check if data already exists
            if (await _context.Roles.AnyAsync() || await _context.Permissions.AnyAsync())
            {
                return; // Data already seeded
            }

            // Create default permissions
            var permissions = new List<Permission>
            {
                // Payment permissions
                new Permission { Name = "View Payments", Description = "Can view payment records", Resource = "Payments", Action = "Read", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Create Payments", Description = "Can create new payments", Resource = "Payments", Action = "Create", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Update Payments", Description = "Can update payment records", Resource = "Payments", Action = "Update", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Delete Payments", Description = "Can delete payment records", Resource = "Payments", Action = "Delete", IsActive = true, CreatedAt = DateTime.UtcNow },
                
                // Student permissions
                new Permission { Name = "View Students", Description = "Can view student records", Resource = "Students", Action = "Read", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Create Students", Description = "Can create new students", Resource = "Students", Action = "Create", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Update Students", Description = "Can update student records", Resource = "Students", Action = "Update", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Delete Students", Description = "Can delete student records", Resource = "Students", Action = "Delete", IsActive = true, CreatedAt = DateTime.UtcNow },
                
                // User management permissions
                new Permission { Name = "View Users", Description = "Can view user records", Resource = "Users", Action = "Read", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Create Users", Description = "Can create new users", Resource = "Users", Action = "Create", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Update Users", Description = "Can update user records", Resource = "Users", Action = "Update", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Delete Users", Description = "Can delete user records", Resource = "Users", Action = "Delete", IsActive = true, CreatedAt = DateTime.UtcNow },
                
                // Role management permissions
                new Permission { Name = "View Roles", Description = "Can view role records", Resource = "Roles", Action = "Read", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Create Roles", Description = "Can create new roles", Resource = "Roles", Action = "Create", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Update Roles", Description = "Can update role records", Resource = "Roles", Action = "Update", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Name = "Delete Roles", Description = "Can delete role records", Resource = "Roles", Action = "Delete", IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            _context.Permissions.AddRange(permissions);
            await _context.SaveChangesAsync();

            // Create default roles
            var roles = new List<Role>
            {
                new Role { Name = "Admin", Description = "Full system administrator", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Role { Name = "Manager", Description = "Department manager with limited admin rights", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Role { Name = "Staff", Description = "Regular staff member", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Role { Name = "Student", Description = "Student user with limited access", IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            _context.Roles.AddRange(roles);
            await _context.SaveChangesAsync();

            // Assign permissions to roles
            var adminRole = await _context.Roles.FirstAsync(r => r.Name == "Admin");
            var managerRole = await _context.Roles.FirstAsync(r => r.Name == "Manager");
            var staffRole = await _context.Roles.FirstAsync(r => r.Name == "Staff");
            var studentRole = await _context.Roles.FirstAsync(r => r.Name == "Student");

            // Admin gets all permissions
            var adminRolePermissions = permissions.Select(p => new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = p.Id,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = "System"
            }).ToList();

            // Manager gets most permissions except user/role management
            var managerPermissions = permissions.Where(p => 
                !p.Resource.Equals("Users", StringComparison.OrdinalIgnoreCase) && 
                !p.Resource.Equals("Roles", StringComparison.OrdinalIgnoreCase)).ToList();
            var managerRolePermissions = managerPermissions.Select(p => new RolePermission
            {
                RoleId = managerRole.Id,
                PermissionId = p.Id,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = "System"
            }).ToList();

            // Staff gets read permissions for payments and students
            var staffPermissions = permissions.Where(p => 
                (p.Resource.Equals("Payments", StringComparison.OrdinalIgnoreCase) || 
                 p.Resource.Equals("Students", StringComparison.OrdinalIgnoreCase)) && 
                p.Action.Equals("Read", StringComparison.OrdinalIgnoreCase)).ToList();
            var staffRolePermissions = staffPermissions.Select(p => new RolePermission
            {
                RoleId = staffRole.Id,
                PermissionId = p.Id,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = "System"
            }).ToList();

            // Student gets read permissions for their own data only
            var studentPermissions = permissions.Where(p => 
                p.Resource.Equals("Students", StringComparison.OrdinalIgnoreCase) && 
                p.Action.Equals("Read", StringComparison.OrdinalIgnoreCase)).ToList();
            var studentRolePermissions = studentPermissions.Select(p => new RolePermission
            {
                RoleId = studentRole.Id,
                PermissionId = p.Id,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = "System"
            }).ToList();

            _context.RolePermissions.AddRange(adminRolePermissions);
            _context.RolePermissions.AddRange(managerRolePermissions);
            _context.RolePermissions.AddRange(staffRolePermissions);
            _context.RolePermissions.AddRange(studentRolePermissions);

            await _context.SaveChangesAsync();

            // Create default admin user
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@xyzuniversity.edu",
                PasswordHash = HashPassword("Admin123!"),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();

            // Assign admin role to admin user
            var adminUserRole = new UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = "System"
            };

            _context.UserRoles.Add(adminUserRole);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Default roles and permissions seeded successfully");
        }

        #endregion

        #region Helper Methods

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }

        #endregion
    }
} 