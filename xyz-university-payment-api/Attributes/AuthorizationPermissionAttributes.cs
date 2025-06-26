// Purpose: Custom authorization attribute for permission-based access control
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using xyz_university_payment_api.Interfaces;
using xyz_university_payment_api.Exceptions;
using System.Security.Claims;

namespace xyz_university_payment_api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _resource;
        private readonly string _action;

        public AuthorizePermissionAttribute(string resource, string action)
        {
            _resource = resource;
            _action = action;
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
            var authorizationService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();

            // Check if user has the required permission
            var hasPermission = await authorizationService.HasPermissionAsync(username, _resource, _action);
            if (!hasPermission)
            {
                context.Result = new ForbidResult();
                return;
            }
        }
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
            var authorizationService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();

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
            var authorizationService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();

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
            var authorizationService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();

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