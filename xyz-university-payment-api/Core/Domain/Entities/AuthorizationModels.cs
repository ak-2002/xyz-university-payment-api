// Purpose: Authorization models for role-based access control
using System.ComponentModel.DataAnnotations;

namespace xyz_university_payment_api.Core.Domain.Entities
{
    // User entity with authentication info
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        
        // Navigation properties
        public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

    // Role entity
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public List<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    // Permission entity
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty; // e.g., "Payments", "Students"
        public string Action { get; set; } = string.Empty; // e.g., "Read", "Write", "Delete"
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public List<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    // Many-to-many relationship between User and Role
    public class UserRole
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public string? AssignedBy { get; set; }
        
        // Navigation properties
        public User User { get; set; } = new User();
        public Role Role { get; set; } = new Role();
    }

    // Many-to-many relationship between Role and Permission
    public class RolePermission
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public string? AssignedBy { get; set; }
        
        // Navigation properties
        public Role Role { get; set; } = new Role();
        public Permission Permission { get; set; } = new Permission();
    }

    // Audit log for authorization changes
    public class AuthorizationAuditLog
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty; // e.g., "RoleAssigned", "PermissionGranted"
        public string EntityType { get; set; } = string.Empty; // e.g., "User", "Role", "Permission"
        public string EntityId { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}