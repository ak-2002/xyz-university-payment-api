using xyz_university_payment_api.Core.Application.Interfaces;
using xyz_university_payment_api.Core.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace xyz_university_payment_api.Core.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISendGridClient _sendGridClient;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            
            var sendGridApiKey = _configuration["SendGrid:ApiKey"];
            if (string.IsNullOrEmpty(sendGridApiKey))
            {
                _logger.LogWarning("SendGrid API key not configured. Email service will log emails instead of sending them.");
                _sendGridClient = null;
            }
            else
            {
                _sendGridClient = new SendGridClient(sendGridApiKey);
            }
            
            _fromEmail = _configuration["SendGrid:FromEmail"] ?? "noreply@xyzuniversity.edu";
            _fromName = _configuration["SendGrid:FromName"] ?? "XYZ University Finance Department";
        }

        public async Task<bool> SendPaymentReceiptAsync(PaymentNotification payment, string studentEmail, string studentName)
        {
            try
            {
                _logger.LogInformation("Sending payment receipt email to {StudentEmail} for payment {PaymentReference}", 
                    studentEmail, payment.PaymentReference);

                var subject = $"Payment Receipt - {payment.PaymentReference}";
                var content = GeneratePaymentReceiptContent(payment, studentName);

                return await SendEmailAsync(studentEmail, subject, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send payment receipt email to {StudentEmail}", studentEmail);
                return false;
            }
        }

        public async Task<bool> SendPaymentConfirmationAsync(PaymentNotification payment, string studentEmail, string studentName)
        {
            try
            {
                _logger.LogInformation("Sending payment confirmation email to {StudentEmail} for payment {PaymentReference}", 
                    studentEmail, payment.PaymentReference);

                var subject = $"Payment Confirmation - {payment.PaymentReference}";
                var content = GeneratePaymentConfirmationContent(payment, studentName);

                return await SendEmailAsync(studentEmail, subject, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send payment confirmation email to {StudentEmail}", studentEmail);
                return false;
            }
        }

        public async Task<bool> SendPaymentFailureNotificationAsync(string studentEmail, string studentName, string errorMessage)
        {
            try
            {
                _logger.LogInformation("Sending payment failure notification to {StudentEmail}", studentEmail);

                var subject = "Payment Processing Failed";
                var content = GeneratePaymentFailureContent(studentName, errorMessage);

                return await SendEmailAsync(studentEmail, subject, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send payment failure notification to {StudentEmail}", studentEmail);
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string studentEmail, string studentName, string studentNumber)
        {
            try
            {
                _logger.LogInformation("Sending welcome email to {StudentEmail}", studentEmail);

                var subject = "Welcome to XYZ University Payment System";
                var content = GenerateWelcomeEmailContent(studentName, studentNumber);

                return await SendEmailAsync(studentEmail, subject, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {StudentEmail}", studentEmail);
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string content)
        {
            try
            {
                if (_sendGridClient == null)
                {
                    // Fallback to logging when SendGrid is not configured
                    _logger.LogInformation("Email would be sent to {ToEmail}", toEmail);
                    _logger.LogInformation("Subject: {Subject}", subject);
                    _logger.LogInformation("Content: {Content}", content);
                    await Task.Delay(100); // Simulate email sending delay
                    return true;
                }

                var from = new EmailAddress(_fromEmail, _fromName);
                var to = new EmailAddress(toEmail);
                var htmlContent = ConvertToHtmlContent(content);
                var plainTextContent = content;

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await _sendGridClient.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to send email to {ToEmail}. Status: {StatusCode}", toEmail, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {ToEmail}", toEmail);
                return false;
            }
        }

        private string ConvertToHtmlContent(string plainTextContent)
        {
            // Convert plain text to basic HTML
            var htmlContent = plainTextContent
                .Replace("\n", "<br>")
                .Replace("  ", "&nbsp;&nbsp;");

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .header {{ background-color: #1e40af; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; }}
        .footer {{ background-color: #f3f4f6; padding: 15px; text-align: center; font-size: 12px; color: #666; }}
        .receipt-details {{ background-color: #f9fafb; padding: 15px; margin: 15px 0; border-left: 4px solid #1e40af; }}
        .amount {{ font-size: 18px; font-weight: bold; color: #059669; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>XYZ University</h1>
    </div>
    <div class='content'>
        {htmlContent}
    </div>
    <div class='footer'>
        <p>This is an automated message from XYZ University Finance Department.</p>
        <p>Please do not reply to this email. For support, contact finance@xyzuniversity.edu</p>
    </div>
</body>
</html>";
        }

        private string GeneratePaymentReceiptContent(PaymentNotification payment, string studentName)
        {
            return $@"
Dear {studentName},

Thank you for your payment. Please find your receipt details below:

Receipt Number: {payment.PaymentReference}
Date: {payment.PaymentDate:dd/MM/yyyy}
Time: {payment.PaymentDate:HH:mm:ss}
Student Number: {payment.StudentNumber}
Amount Paid: ${payment.AmountPaid:F2}
Payment Method: {payment.PaymentMethod}
Transaction ID: {payment.TransactionId ?? "N/A"}
Receipt Number: {payment.ReceiptNumber ?? "N/A"}

Notes: {payment.Notes ?? "No additional notes"}

This receipt serves as proof of payment. Please keep it for your records.

If you have any questions, please contact the finance office.

Best regards,
XYZ University Finance Department
Email: finance@xyzuniversity.edu
Phone: +254-XXX-XXX-XXX

Generated on: {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss} UTC
            ".Trim();
        }

        private string GeneratePaymentConfirmationContent(PaymentNotification payment, string studentName)
        {
            return $@"
Dear {studentName},

Your payment has been successfully processed and confirmed.

Payment Details:
- Reference: {payment.PaymentReference}
- Amount: ${payment.AmountPaid:F2}
- Date: {payment.PaymentDate:dd/MM/yyyy}
- Method: {payment.PaymentMethod}

Your student account has been updated accordingly.

Thank you for your prompt payment.

Best regards,
XYZ University Finance Department
            ".Trim();
        }

        private string GeneratePaymentFailureContent(string studentName, string errorMessage)
        {
            return $@"
Dear {studentName},

We regret to inform you that your payment could not be processed successfully.

Error Details: {errorMessage}

Please try again or contact the finance office for assistance.

Best regards,
XYZ University Finance Department
            ".Trim();
        }

        private string GenerateWelcomeEmailContent(string studentName, string studentNumber)
        {
            return $@"
Dear {studentName},

Welcome to the XYZ University Payment System!

Your student account has been successfully created:
- Student Number: {studentNumber}
- Name: {studentName}

You can now:
- View your payment history
- Download payment receipts
- Access your financial statements

If you have any questions, please contact the finance office.

Best regards,
XYZ University Finance Department
            ".Trim();
        }
    }
} 