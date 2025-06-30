// Purpose: Custom authorization attribute for permission-based access control
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using xyz_university_payment_api.Interfaces;
using xyz_university_payment_api.Exceptions;
using System.Security.Claims;
using System.Linq;

namespace xyz_university_payment_api.Attributes
{
    /// <summary>
    /// Custom authorization attribute for permission-based access control
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizePermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string _resource;
        private readonly string _action;
        private readonly string[] _requiredRoles;

        /// <summary>
        /// Initialize with resource and action for permission-based authorization
        /// </summary>
        /// <param name="resource">The resource being accessed (e.g., "payments", "students")</param>
        /// <param name="action">The action being performed (e.g., "read", "write", "delete")</param>
        public AuthorizePermissionAttribute(string resource, string action)
        {
            _resource = resource;
            _action = action;
            Policy = "PermissionPolicy";
        }

        /// <summary>
        /// Initialize with required roles for role-based authorization
        /// </summary>
        /// <param name="roles">Comma-separated list of required roles</param>
        public AuthorizePermissionAttribute(string roles)
        {
            _requiredRoles = roles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(r => r.Trim())
                                 .ToArray();
            Policy = "RolePolicy";
        }

        /// <summary>
        /// Initialize with both roles and permissions
        /// </summary>
        /// <param name="resource">The resource being accessed</param>
        /// <param name="action">The action being performed</param>
        /// <param name="roles">Comma-separated list of required roles</param>
        public AuthorizePermissionAttribute(string resource, string action, string roles)
        {
            _resource = resource;
            _action = action;
            _requiredRoles = roles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(r => r.Trim())
                                 .ToArray();
            Policy = "PermissionAndRolePolicy";
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // Check if user is authenticated
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Check if user is active
            var isActiveClaim = user.FindFirst("is_active")?.Value;
            if (isActiveClaim != "True")
            {
                context.Result = new ForbidResult();
                return;
            }

            // Check roles if required
            if (_requiredRoles != null && _requiredRoles.Length > 0)
            {
                var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
                var hasRequiredRole = _requiredRoles.Any(role => userRoles.Contains(role));
                
                if (!hasRequiredRole)
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }

            // Check permissions if required
            if (!string.IsNullOrEmpty(_resource) && !string.IsNullOrEmpty(_action))
            {
                var requiredPermission = $"{_resource}.{_action}";
                var userPermissions = user.FindAll("permission").Select(c => c.Value);
                var hasPermission = userPermissions.Contains(requiredPermission);

                if (!hasPermission)
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Authorization attribute for admin-only access
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAdminAttribute : AuthorizePermissionAttribute
    {
        public AuthorizeAdminAttribute() : base("Admin") { }
    }

    /// <summary>
    /// Authorization attribute for payment operations
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizePaymentAttribute : AuthorizePermissionAttribute
    {
        public AuthorizePaymentAttribute(string action) : base("payments", action) { }
    }

    /// <summary>
    /// Authorization attribute for student operations
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeStudentAttribute : AuthorizePermissionAttribute
    {
        public AuthorizeStudentAttribute(string action) : base("students", action) { }
    }

    /// <summary>
    /// Authorization attribute for user management operations
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeUserManagementAttribute : AuthorizePermissionAttribute
    {
        public AuthorizeUserManagementAttribute(string action) : base("users", action) { }
    }

    // Attribute for requiring multiple permissions (ALL must be satisfied)
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizePermissionsAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _resource;
        private readonly string[] _actions;

        public AuthorizePermissionsAttribute(string resource, params string[] actions)
        {
            _resource = resource;
            _actions = actions;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Check if user is authenticated
            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get username from claims
            var username = user.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get authorization service
            var authorizationService = context.HttpContext.RequestServices.GetRequiredService<xyz_university_payment_api.Interfaces.IAuthorizationService>();

            // Check if user has ALL required permissions
            foreach (var action in _actions)
            {
                var hasPermission = await authorizationService.HasPermissionAsync(username, _resource, action);
                if (!hasPermission)
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }
        }
    }

    // Attribute for requiring any of multiple permissions (ANY can satisfy)
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizeAnyPermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _resource;
        private readonly string[] _actions;

        public AuthorizeAnyPermissionAttribute(string resource, params string[] actions)
        {
            _resource = resource;
            _actions = actions;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Check if user is authenticated
            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get username from claims
            var username = user.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get authorization service
            var authorizationService = context.HttpContext.RequestServices.GetRequiredService<xyz_university_payment_api.Interfaces.IAuthorizationService>();

            // Check if user has ANY of the required permissions
            foreach (var action in _actions)
            {
                var hasPermission = await authorizationService.HasPermissionAsync(username, _resource, action);
                if (hasPermission)
                {
                    return; // User has at least one required permission
                }
            }

            // User doesn't have any of the required permissions
            context.Result = new ForbidResult();
        }
    }

    // Attribute for requiring specific roles
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizeRoleAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Check if user is authenticated
            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get username from claims
            var username = user.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get authorization service
            var authorizationService = context.HttpContext.RequestServices.GetRequiredService<xyz_university_payment_api.Interfaces.IAuthorizationService>();

            // Get user roles
            var userRoles = await authorizationService.GetUserRolesAsync(username);

            // Check if user has ANY of the required roles
            var hasRequiredRole = _roles.Any(role => userRoles.Contains(role));
            if (!hasRequiredRole)
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
} 