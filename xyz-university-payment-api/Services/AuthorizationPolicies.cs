using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace xyz_university_payment_api.Services
{
    /// <summary>
    /// Authorization policies for the application
    /// </summary>
    public static class AuthorizationPolicies
    {
        /// <summary>
        /// Policy for active users only
        /// </summary>
        public static void AddActiveUserPolicy(this AuthorizationOptions options)
        {
            options.AddPolicy("ActiveUser", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("is_active", "True"));
        }

        /// <summary>
        /// Policy for admin users
        /// </summary>
        public static void AddAdminPolicy(this AuthorizationOptions options)
        {
            options.AddPolicy("Admin", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("is_active", "True")
                      .RequireRole("Admin"));
        }

        /// <summary>
        /// Policy for payment operations
        /// </summary>
        public static void AddPaymentPolicy(this AuthorizationOptions options)
        {
            options.AddPolicy("PaymentAccess", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("is_active", "True")
                      .RequireAssertion(context =>
                      {
                          var user = context.User;
                          var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
                          var permissions = user.FindAll("permission").Select(c => c.Value);

                          // Allow if user has Admin role or payment permissions
                          return roles.Contains("Admin") || 
                                 permissions.Any(p => p.StartsWith("payments."));
                      }));
        }

        /// <summary>
        /// Policy for student operations
        /// </summary>
        public static void AddStudentPolicy(this AuthorizationOptions options)
        {
            options.AddPolicy("StudentAccess", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("is_active", "True")
                      .RequireAssertion(context =>
                      {
                          var user = context.User;
                          var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
                          var permissions = user.FindAll("permission").Select(c => c.Value);

                          // Allow if user has Admin role or student permissions
                          return roles.Contains("Admin") || 
                                 permissions.Any(p => p.StartsWith("students."));
                      }));
        }

        /// <summary>
        /// Policy for user management operations
        /// </summary>
        public static void AddUserManagementPolicy(this AuthorizationOptions options)
        {
            options.AddPolicy("UserManagement", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("is_active", "True")
                      .RequireRole("Admin", "UserManager"));
        }

        /// <summary>
        /// Policy for read-only operations
        /// </summary>
        public static void AddReadOnlyPolicy(this AuthorizationOptions options)
        {
            options.AddPolicy("ReadOnly", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("is_active", "True")
                      .RequireAssertion(context =>
                      {
                          var user = context.User;
                          var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
                          var permissions = user.FindAll("permission").Select(c => c.Value);

                          // Allow if user has any role or read permissions
                          return roles.Any() || 
                                 permissions.Any(p => p.EndsWith(".read"));
                      }));
        }

        /// <summary>
        /// Policy for write operations
        /// </summary>
        public static void AddWritePolicy(this AuthorizationOptions options)
        {
            options.AddPolicy("WriteAccess", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("is_active", "True")
                      .RequireAssertion(context =>
                      {
                          var user = context.User;
                          var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
                          var permissions = user.FindAll("permission").Select(c => c.Value);

                          // Allow if user has Admin role or write permissions
                          return roles.Contains("Admin") || 
                                 permissions.Any(p => p.EndsWith(".write") || p.EndsWith(".create") || p.EndsWith(".update"));
                      }));
        }

        /// <summary>
        /// Policy for delete operations
        /// </summary>
        public static void AddDeletePolicy(this AuthorizationOptions options)
        {
            options.AddPolicy("DeleteAccess", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("is_active", "True")
                      .RequireAssertion(context =>
                      {
                          var user = context.User;
                          var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
                          var permissions = user.FindAll("permission").Select(c => c.Value);

                          // Allow if user has Admin role or delete permissions
                          return roles.Contains("Admin") || 
                                 permissions.Any(p => p.EndsWith(".delete"));
                      }));
        }

        /// <summary>
        /// Policy for financial operations
        /// </summary>
        public static void AddFinancialPolicy(this AuthorizationOptions options)
        {
            options.AddPolicy("FinancialAccess", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("is_active", "True")
                      .RequireRole("Admin", "FinanceManager", "Accountant"));
        }

        /// <summary>
        /// Policy for reporting operations
        /// </summary>
        public static void AddReportingPolicy(this AuthorizationOptions options)
        {
            options.AddPolicy("ReportingAccess", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("is_active", "True")
                      .RequireAssertion(context =>
                      {
                          var user = context.User;
                          var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
                          var permissions = user.FindAll("permission").Select(c => c.Value);

                          // Allow if user has reporting roles or permissions
                          return roles.Contains("Admin") || 
                                 roles.Contains("FinanceManager") ||
                                 roles.Contains("Accountant") ||
                                 permissions.Any(p => p.Contains("report") || p.Contains("summary"));
                      }));
        }
    }
} 