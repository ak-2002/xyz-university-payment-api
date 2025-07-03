// Purpose: Authorization attributes for role-based access control
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace xyz_university_payment_api.Presentation.Attributes
{
    /// <summary>
    /// Authorization attribute for permission-based access control
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizePermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string? _resource;
        private readonly string? _action;
        private readonly string[]? _requiredRoles;

        /// <summary>
        /// Constructor for resource and action-based authorization
        /// </summary>
        /// <param name="resource">Resource to authorize access to</param>
        /// <param name="action">Action to authorize</param>
        public AuthorizePermissionAttribute(string resource, string action)
        {
            _resource = resource;
            _action = action;
        }

        /// <summary>
        /// Constructor for role-based authorization
        /// </summary>
        /// <param name="roles">Comma-separated list of required roles</param>
        public AuthorizePermissionAttribute(string roles)
        {
            _requiredRoles = roles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(r => r.Trim())
                                 .ToArray();
        }

        /// <summary>
        /// Constructor for combined role and permission-based authorization
        /// </summary>
        /// <param name="resource">Resource to authorize access to</param>
        /// <param name="action">Action to authorize</param>
        /// <param name="roles">Comma-separated list of required roles</param>
        public AuthorizePermissionAttribute(string resource, string action, string roles)
        {
            _resource = resource;
            _action = action;
            _requiredRoles = roles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(r => r.Trim())
                                 .ToArray();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check if user is authenticated
            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Check roles if required
            if (_requiredRoles != null && _requiredRoles.Any())
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
    public class AuthorizePermissionsAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _resource;
        private readonly string[] _actions;

        public AuthorizePermissionsAttribute(string resource, params string[] actions)
        {
            _resource = resource;
            _actions = actions;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check if user is authenticated
            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get user permissions from JWT claims
            var userPermissions = user.FindAll("permission").Select(c => c.Value).ToList();

            // Check if user has ALL required permissions
            foreach (var action in _actions)
            {
                var requiredPermission = $"{_resource}.{action}";
                if (!userPermissions.Contains(requiredPermission))
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }
        }
    }

    // Attribute for requiring any of multiple permissions (ANY can satisfy)
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizeAnyPermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _resource;
        private readonly string[] _actions;

        public AuthorizeAnyPermissionAttribute(string resource, params string[] actions)
        {
            _resource = resource;
            _actions = actions;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check if user is authenticated
            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get user permissions from JWT claims
            var userPermissions = user.FindAll("permission").Select(c => c.Value).ToList();

            // Check if user has ANY of the required permissions
            foreach (var action in _actions)
            {
                var requiredPermission = $"{_resource}.{action}";
                if (userPermissions.Contains(requiredPermission))
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
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check if user is authenticated
            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get user roles from JWT claims
            var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

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