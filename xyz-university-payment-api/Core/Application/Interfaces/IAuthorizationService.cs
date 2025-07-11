// Purpose: Authorization service interface for role-based access control
using xyz_university_payment_api.Core.Domain.Entities;
using xyz_university_payment_api.Core.Application.DTOs;
using xyz_university_payment_api.Infrastructure.Data;

namespace xyz_university_payment_api.Core.Application.Interfaces
{
    public interface IAuthorizationService
    {
        // User management
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserDto> UpdateUserAsync(int userId, UpdateUserDto updateUserDto);
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<UserDto?> GetUserByUsernameAsync(string username);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);

        // Authentication
        Task<UserLoginResponseDto?> AuthenticateAsync(UserLoginDto loginDto);
        Task<UserLoginResponseDto?> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(string refreshToken);

        // Role management
        Task<RoleDto> CreateRoleAsync(CreateRoleDto createRoleDto);
        Task<RoleDto> UpdateRoleAsync(int roleId, UpdateRoleDto updateRoleDto);
        Task<RoleDto?> GetRoleByIdAsync(int roleId);
        Task<RoleDto?> GetRoleByNameAsync(string roleName);
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task<bool> DeleteRoleAsync(int roleId);

        // Permission management
        Task<PermissionDto> CreatePermissionAsync(CreatePermissionDto createPermissionDto);
        Task<PermissionDto> UpdatePermissionAsync(int permissionId, UpdatePermissionDto updatePermissionDto);
        Task<PermissionDto?> GetPermissionByIdAsync(int permissionId);
        Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
        Task<bool> DeletePermissionAsync(int permissionId);

        // User-Role assignments
        Task<bool> AssignRolesToUserAsync(AssignRolesToUserDto assignRolesDto);
        Task<bool> RemoveRolesFromUserAsync(RemoveRolesFromUserDto removeRolesDto);
        Task<IEnumerable<RoleDto>> GetUserRolesAsync(int userId);

        // Role-Permission assignments
        Task<bool> AssignPermissionsToRoleAsync(AssignPermissionsToRoleDto assignPermissionsDto);
        Task<bool> RemovePermissionsFromRoleAsync(RemovePermissionsFromRoleDto removePermissionsDto);
        Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(int roleId);

        // Authorization checks
        Task<bool> HasPermissionAsync(string username, string resource, string action);
        Task<PermissionCheckResultDto> CheckPermissionAsync(CheckPermissionDto checkPermissionDto);
        Task<IEnumerable<string>> GetUserPermissionsAsync(string username);
        Task<IEnumerable<string>> GetUserRolesAsync(string username);

        // Audit logging
        Task LogAuthorizationActionAsync(string action, string entityType, string entityId, string performedBy, string? oldValues = null, string? newValues = null);
        Task<IEnumerable<AuthorizationAuditLogDto>> GetAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null, string? action = null, string? entityType = null);

        // Seed data
        Task SeedDefaultRolesAndPermissionsAsync();

        // Helper methods
        Task<bool> LogAuthorizationActionAsync(string action, string resource, string resourceId, string performedBy);
        AppDbContext GetDbContext();
        Task<ApiResponse<UserDto>> CreateFullAccessUserAsync(CreateFullAccessUserDto createUserDto);
    }
}