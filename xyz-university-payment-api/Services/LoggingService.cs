// Purpose: Custom logging service for business-specific logging
// Provides structured logging with additional context and business-specific methods
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using System;
using System.Threading.Tasks;

namespace xyz_university_payment_api.Services
{
    
    // Custom logging service that provides structured logging with business context

    public interface ILoggingService
    {
        void LogStudentOperation(string operation, string studentNumber, string message, LogLevel level = LogLevel.Information);
        void LogPaymentOperation(string operation, string paymentReference, string studentNumber, decimal amount, string message, LogLevel level = LogLevel.Information);
        void LogDatabaseOperation(string operation, string entity, string message, LogLevel level = LogLevel.Information);
        void LogApiRequest(string method, string endpoint, string clientIp, string userAgent, LogLevel level = LogLevel.Information);
        void LogApiResponse(string method, string endpoint, int statusCode, long durationMs, LogLevel level = LogLevel.Information);
        void LogBusinessRule(string rule, string context, string message, LogLevel level = LogLevel.Information);
        void LogSecurityEvent(string eventType, string user, string details, LogLevel level = LogLevel.Warning);
        void LogPerformance(string operation, long durationMs, string details = "", LogLevel level = LogLevel.Information);
        Task LogErrorAsync(string operation, Exception ex);
    }


    // Implementation of custom logging service
    
    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;

        public LoggingService(ILogger<LoggingService> logger)
        {
            _logger = logger;
        }

        public void LogStudentOperation(string operation, string studentNumber, string message, LogLevel level = LogLevel.Information)
        {
            using (LogContext.PushProperty("Operation", operation))
            using (LogContext.PushProperty("StudentNumber", studentNumber))
            using (LogContext.PushProperty("Category", "Student"))
            {
                LogMessage(level, message);
            }
        }

        public void LogPaymentOperation(string operation, string paymentReference, string studentNumber, decimal amount, string message, LogLevel level = LogLevel.Information)
        {
            using (LogContext.PushProperty("Operation", operation))
            using (LogContext.PushProperty("PaymentReference", paymentReference))
            using (LogContext.PushProperty("StudentNumber", studentNumber))
            using (LogContext.PushProperty("Amount", amount))
            using (LogContext.PushProperty("Category", "Payment"))
            {
                LogMessage(level, message);
            }
        }

        public void LogDatabaseOperation(string operation, string entity, string message, LogLevel level = LogLevel.Information)
        {
            using (LogContext.PushProperty("Operation", operation))
            using (LogContext.PushProperty("Entity", entity))
            using (LogContext.PushProperty("Category", "Database"))
            {
                LogMessage(level, message);
            }
        }

        public void LogApiRequest(string method, string endpoint, string clientIp, string userAgent, LogLevel level = LogLevel.Information)
        {
            using (LogContext.PushProperty("Method", method))
            using (LogContext.PushProperty("Endpoint", endpoint))
            using (LogContext.PushProperty("ClientIP", clientIp))
            using (LogContext.PushProperty("UserAgent", userAgent))
            using (LogContext.PushProperty("Category", "API"))
            using (LogContext.PushProperty("Type", "Request"))
            {
                LogMessage(level, "API Request received");
            }
        }

        public void LogApiResponse(string method, string endpoint, int statusCode, long durationMs, LogLevel level = LogLevel.Information)
        {
            using (LogContext.PushProperty("Method", method))
            using (LogContext.PushProperty("Endpoint", endpoint))
            using (LogContext.PushProperty("StatusCode", statusCode))
            using (LogContext.PushProperty("DurationMs", durationMs))
            using (LogContext.PushProperty("Category", "API"))
            using (LogContext.PushProperty("Type", "Response"))
            {
                LogMessage(level, "API Response sent");
            }
        }

        public void LogBusinessRule(string rule, string context, string message, LogLevel level = LogLevel.Information)
        {
            using (LogContext.PushProperty("BusinessRule", rule))
            using (LogContext.PushProperty("Context", context))
            using (LogContext.PushProperty("Category", "BusinessRule"))
            {
                LogMessage(level, message);
            }
        }

        public void LogSecurityEvent(string eventType, string user, string details, LogLevel level = LogLevel.Warning)
        {
            using (LogContext.PushProperty("SecurityEvent", eventType))
            using (LogContext.PushProperty("User", user))
            using (LogContext.PushProperty("Details", details))
            using (LogContext.PushProperty("Category", "Security"))
            {
                LogMessage(level, "Security event occurred");
            }
        }

        public void LogPerformance(string operation, long durationMs, string details = "", LogLevel level = LogLevel.Information)
        {
            using (LogContext.PushProperty("Operation", operation))
            using (LogContext.PushProperty("DurationMs", durationMs))
            using (LogContext.PushProperty("Details", details))
            using (LogContext.PushProperty("Category", "Performance"))
            {
                LogMessage(level, "Performance measurement");
            }
        }

        public async Task LogErrorAsync(string operation, Exception ex)
        {
            using (LogContext.PushProperty("Operation", operation))
            using (LogContext.PushProperty("Category", "Error"))
            using (LogContext.PushProperty("ExceptionType", ex.GetType().Name))
            {
                _logger.LogError(ex, "Error in operation: {Operation}", operation);
            }
            await Task.CompletedTask; // For async compatibility
        }

        private void LogMessage(LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Trace:
                    _logger.LogTrace(message);
                    break;
                case LogLevel.Debug:
                    _logger.LogDebug(message);
                    break;
                case LogLevel.Information:
                    _logger.LogInformation(message);
                    break;
                case LogLevel.Warning:
                    _logger.LogWarning(message);
                    break;
                case LogLevel.Error:
                    _logger.LogError(message);
                    break;
                case LogLevel.Critical:
                    _logger.LogCritical(message);
                    break;
                default:
                    _logger.LogInformation(message);
                    break;
            }
        }
    }
}