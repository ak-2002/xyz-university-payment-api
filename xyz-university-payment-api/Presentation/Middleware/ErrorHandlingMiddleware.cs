using System.Net;
using System.Text.Json;
using xyz_university_payment_api.Core.Domain.Exceptions;
using xyz_university_payment_api.Core.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace xyz_university_payment_api.Presentation.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var requestId = context.TraceIdentifier;
            var errorResponse = CreateErrorResponse(exception, requestId);

            // Set response properties
            context.Response.StatusCode = errorResponse.StatusCode;
            context.Response.ContentType = "application/json";

            // Log the exception with appropriate level
            LogException(exception, requestId, context);

            // Serialize and send response
            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private ErrorResponseDto CreateErrorResponse(Exception exception, string requestId)
        {
            return exception switch
            {
                // Handle custom API exceptions
                ApiException apiException => new ErrorResponseDto
                {
                    Type = apiException.ErrorCode,
                    Title = GetTitleForStatusCode(apiException.StatusCode),
                    Detail = apiException.Message,
                    StatusCode = apiException.StatusCode,
                    RequestId = requestId,
                    Timestamp = DateTime.UtcNow,
                    Errors = exception is ValidationException validationEx ? validationEx.ValidationErrors : new List<string>()
                },

                // Handle validation exceptions from FluentValidation
                FluentValidation.ValidationException validationException => new ErrorResponseDto
                {
                    Type = "VALIDATION_ERROR",
                    Title = "Validation Failed",
                    Detail = "One or more validation errors occurred.",
                    StatusCode = 400,
                    RequestId = requestId,
                    Timestamp = DateTime.UtcNow,
                    Errors = validationException.Errors.Select(e => e.ErrorMessage).ToList()
                },

                // Handle Entity Framework exceptions
                Microsoft.EntityFrameworkCore.DbUpdateException dbException => new ErrorResponseDto
                {
                    Type = "DATABASE_ERROR",
                    Title = "Database Operation Failed",
                    Detail = _environment.IsDevelopment() ? dbException.Message : "A database operation failed.",
                    StatusCode = 500,
                    RequestId = requestId,
                    Timestamp = DateTime.UtcNow,
                    Errors = new List<string> { "Database operation failed" }
                },

                // Handle authentication/authorization exceptions
                UnauthorizedAccessException => new ErrorResponseDto
                {
                    Type = "UNAUTHORIZED",
                    Title = "Access Denied",
                    Detail = "Authentication is required to access this resource.",
                    StatusCode = 401,
                    RequestId = requestId,
                    Timestamp = DateTime.UtcNow,
                    Errors = new List<string>()
                },

                // Handle general exceptions
                _ => new ErrorResponseDto
                {
                    Type = "INTERNAL_SERVER_ERROR",
                    Title = "Internal Server Error",
                    Detail = _environment.IsDevelopment() ? exception.Message : "An unexpected error occurred.",
                    StatusCode = 500,
                    RequestId = requestId,
                    Timestamp = DateTime.UtcNow,
                    Errors = new List<string> { "An unexpected error occurred" }
                }
            };
        }

        private void LogException(Exception exception, string requestId, HttpContext context)
        {
            var logLevel = exception switch
            {
                ApiException apiException => apiException.StatusCode switch
                {
                    400 => LogLevel.Warning,
                    401 => LogLevel.Warning,
                    403 => LogLevel.Warning,
                    404 => LogLevel.Information,
                    409 => LogLevel.Warning,
                    _ => LogLevel.Error
                },
                FluentValidation.ValidationException => LogLevel.Warning,
                UnauthorizedAccessException => LogLevel.Warning,
                _ => LogLevel.Error
            };

            var logMessage = new
            {
                RequestId = requestId,
                Path = context.Request.Path,
                Method = context.Request.Method,
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                ExceptionType = exception.GetType().Name,
                ExceptionMessage = exception.Message,
                StatusCode = exception is ApiException apiEx ? apiEx.StatusCode : 500
            };

            _logger.Log(logLevel, exception, "Request {RequestId} failed: {Message}", requestId, JsonSerializer.Serialize(logMessage));
        }

        private string GetTitleForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                400 => "Bad Request",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Not Found",
                409 => "Conflict",
                422 => "Unprocessable Entity",
                500 => "Internal Server Error",
                502 => "Bad Gateway",
                503 => "Service Unavailable",
                _ => "Error"
            };
        }
    }


}