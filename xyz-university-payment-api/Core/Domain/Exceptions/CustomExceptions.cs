// Purpose: Custom exceptions for business logic and API error handling
namespace xyz_university_payment_api.Core.Domain.Exceptions
{
    // Base custom exception
    public abstract class ApiException : Exception
    {
        public string ErrorCode { get; }
        public int StatusCode { get; }

        protected ApiException(string message, string errorCode, int statusCode)
            : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }

        protected ApiException(string message, string errorCode, int statusCode, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }
    }

    // Payment-related exceptions
    public class PaymentNotFoundException : ApiException
    {
        public PaymentNotFoundException(int paymentId)
            : base($"Payment with ID {paymentId} was not found.", "PAYMENT_NOT_FOUND", 404)
        {
        }

        public PaymentNotFoundException(string paymentReference)
            : base($"Payment with reference {paymentReference} was not found.", "PAYMENT_NOT_FOUND", 404)
        {
        }
    }

    public class DuplicatePaymentException : ApiException
    {
        public DuplicatePaymentException(string paymentReference)
            : base($"Payment with reference {paymentReference} already exists.", "DUPLICATE_PAYMENT", 409)
        {
        }
    }

    public class InvalidPaymentAmountException : ApiException
    {
        public InvalidPaymentAmountException(decimal amount)
            : base($"Invalid payment amount: {amount}. Amount must be greater than 0.", "INVALID_PAYMENT_AMOUNT", 400)
        {
        }
    }

    public class PaymentProcessingException : ApiException
    {
        public PaymentProcessingException(string message)
            : base($"Payment processing failed: {message}", "PAYMENT_PROCESSING_ERROR", 500)
        {
        }

        public PaymentProcessingException(string message, Exception innerException)
            : base($"Payment processing failed: {message}", "PAYMENT_PROCESSING_ERROR", 500, innerException)
        {
        }
    }

    // Student-related exceptions
    public class StudentNotFoundException : ApiException
    {
        public StudentNotFoundException(string studentNumber)
            : base($"Student with number {studentNumber} was not found.", "STUDENT_NOT_FOUND", 404)
        {
        }

        public StudentNotFoundException(int studentId)
            : base($"Student with ID {studentId} was not found.", "STUDENT_NOT_FOUND", 404)
        {
        }
    }

    public class DuplicateStudentException : ApiException
    {
        public DuplicateStudentException(string studentNumber)
            : base($"Student with number {studentNumber} already exists.", "DUPLICATE_STUDENT", 409)
        {
        }
    }

    public class InactiveStudentException : ApiException
    {
        public InactiveStudentException(string studentNumber)
            : base($"Student with number {studentNumber} is inactive and cannot receive payments.", "INACTIVE_STUDENT", 400)
        {
        }
    }

    // Generic not found exception
    public class NotFoundException : ApiException
    {
        public NotFoundException(string message)
            : base(message, "NOT_FOUND", 404)
        {
        }

        public NotFoundException(string entityType, object id)
            : base($"{entityType} with ID {id} was not found.", "NOT_FOUND", 404)
        {
        }
    }

    // Validation exceptions
    public class ValidationException : ApiException
    {
        public List<string> ValidationErrors { get; }

        public ValidationException(List<string> validationErrors)
            : base("Validation failed. Please check the provided data.", "VALIDATION_ERROR", 400)
        {
            ValidationErrors = validationErrors;
        }

        public ValidationException(string message, List<string> validationErrors)
            : base(message, "VALIDATION_ERROR", 400)
        {
            ValidationErrors = validationErrors;
        }
    }

    // Database exceptions
    public class DatabaseException : ApiException
    {
        public DatabaseException(string message)
            : base($"Database operation failed: {message}", "DATABASE_ERROR", 500)
        {
        }

        public DatabaseException(string message, Exception innerException)
            : base($"Database operation failed: {message}", "DATABASE_ERROR", 500, innerException)
        {
        }
    }

    // Authentication and Authorization exceptions
    public class UnauthorizedException : ApiException
    {
        public UnauthorizedException(string message = "Access denied. Authentication required.")
            : base(message, "UNAUTHORIZED", 401)
        {
        }
    }

    public class ForbiddenException : ApiException
    {
        public ForbiddenException(string message = "Access denied. Insufficient permissions.")
            : base(message, "FORBIDDEN", 403)
        {
        }
    }

    // External service exceptions
    public class ExternalServiceException : ApiException
    {
        public string ServiceName { get; }

        public ExternalServiceException(string serviceName, string message)
            : base($"External service '{serviceName}' error: {message}", "EXTERNAL_SERVICE_ERROR", 502)
        {
            ServiceName = serviceName;
        }

        public ExternalServiceException(string serviceName, string message, Exception innerException)
            : base($"External service '{serviceName}' error: {message}", "EXTERNAL_SERVICE_ERROR", 502, innerException)
        {
            ServiceName = serviceName;
        }
    }

    // Message queue exceptions
    public class MessageQueueException : ApiException
    {
        public MessageQueueException(string message)
            : base($"Message queue operation failed: {message}", "MESSAGE_QUEUE_ERROR", 500)
        {
        }

        public MessageQueueException(string message, Exception innerException)
            : base($"Message queue operation failed: {message}", "MESSAGE_QUEUE_ERROR", 500, innerException)
        {
        }
    }

    // Messaging exceptions
    public class MessagingException : ApiException
    {
        public MessagingException(string message)
            : base($"Messaging failed: {message}", "MESSAGING_ERROR", 500)
        {
        }

        public MessagingException(string message, Exception innerException)
            : base($"Messaging failed: {message}", "MESSAGING_ERROR", 500, innerException)
        {
        }
    }

    // Configuration exceptions
    public class ConfigurationException : ApiException
    {
        public ConfigurationException(string message)
            : base($"Configuration error: {message}", "CONFIGURATION_ERROR", 500)
        {
        }
    }

    // Batch processing exceptions
    public class BatchProcessingException : ApiException
    {
        public BatchProcessingException(string message)
            : base($"Batch processing failed: {message}", "BATCH_PROCESSING_ERROR", 500)
        {
        }

        public BatchProcessingException(string message, Exception innerException)
            : base($"Batch processing failed: {message}", "BATCH_PROCESSING_ERROR", 500, innerException)
        {
        }
    }

    // Reconciliation exceptions
    public class ReconciliationException : ApiException
    {
        public ReconciliationException(string message)
            : base($"Reconciliation failed: {message}", "RECONCILIATION_ERROR", 500)
        {
        }

        public ReconciliationException(string message, Exception innerException)
            : base($"Reconciliation failed: {message}", "RECONCILIATION_ERROR", 500, innerException)
        {
        }
    }

    // Cache exceptions
    public class CacheException : ApiException
    {
        public CacheException(string message)
            : base($"Cache operation failed: {message}", "CACHE_ERROR", 500)
        {
        }

        public CacheException(string message, Exception innerException)
            : base($"Cache operation failed: {message}", "CACHE_ERROR", 500, innerException)
        {
        }
    }
}