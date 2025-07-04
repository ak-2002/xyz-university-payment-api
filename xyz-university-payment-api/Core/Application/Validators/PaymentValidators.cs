// Purpose: Validation rules for Payment DTOs using FluentValidation
using FluentValidation;
using xyz_university_payment_api.Core.Application.DTOs;

namespace xyz_university_payment_api.Validators
{
    // Validator for CreatePaymentDto
    public class CreatePaymentDtoValidator : AbstractValidator<CreatePaymentDto>
    {
        public CreatePaymentDtoValidator()
        {
            RuleFor(x => x.StudentNumber)
                .NotEmpty().WithMessage("Student number is required")
                .Length(5, 20).WithMessage("Student number must be between 5 and 20 characters")
                .Matches(@"^[A-Z0-9]+$").WithMessage("Student number must contain only uppercase letters and numbers");

            RuleFor(x => x.PaymentReference)
                .NotEmpty().WithMessage("Payment reference is required")
                .Length(5, 50).WithMessage("Payment reference must be between 5 and 50 characters")
                .Matches(@"^[A-Z0-9\-_]+$").WithMessage("Payment reference can only contain uppercase letters, numbers, hyphens, and underscores");

            RuleFor(x => x.AmountPaid)
                .GreaterThan(0).WithMessage("Amount paid must be greater than 0")
                .LessThanOrEqualTo(1000000).WithMessage("Amount paid cannot exceed 1,000,000")
                .PrecisionScale(10, 2, false).WithMessage("Amount paid cannot have more than 2 decimal places");

            RuleFor(x => x.PaymentDate)
                .NotEmpty().WithMessage("Payment date is required")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Payment date cannot be in the future")
                .GreaterThan(DateTime.UtcNow.AddYears(-10)).WithMessage("Payment date cannot be more than 10 years ago");
        }
    }

    // Validator for PaymentDto
    public class PaymentDtoValidator : AbstractValidator<PaymentDto>
    {
        public PaymentDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Payment ID must be greater than 0");

            RuleFor(x => x.StudentNumber)
                .NotEmpty().WithMessage("Student number is required")
                .Length(5, 20).WithMessage("Student number must be between 5 and 20 characters");

            RuleFor(x => x.PaymentReference)
                .NotEmpty().WithMessage("Payment reference is required")
                .Length(5, 50).WithMessage("Payment reference must be between 5 and 50 characters");

            RuleFor(x => x.AmountPaid)
                .GreaterThan(0).WithMessage("Amount paid must be greater than 0")
                .LessThanOrEqualTo(1000000).WithMessage("Amount paid cannot exceed 1,000,000");

            RuleFor(x => x.PaymentDate)
                .NotEmpty().WithMessage("Payment date is required");

            RuleFor(x => x.DateReceived)
                .NotEmpty().WithMessage("Date received is required");

            RuleFor(x => x.StudentName)
                .NotEmpty().WithMessage("Student name is required")
                .Length(2, 100).WithMessage("Student name must be between 2 and 100 characters");

            RuleFor(x => x.StudentProgram)
                .NotEmpty().WithMessage("Student program is required")
                .Length(2, 100).WithMessage("Student program must be between 2 and 100 characters");
        }
    }

    // Validator for PaymentResponseDto
    public class PaymentResponseDtoValidator : AbstractValidator<PaymentResponseDto>
    {
        public PaymentResponseDtoValidator()
        {
            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Message is required")
                .MaximumLength(500).WithMessage("Message cannot exceed 500 characters");

            RuleFor(x => x.ValidationErrors)
                .NotNull().WithMessage("Validation errors list cannot be null");

            RuleFor(x => x.ProcessedPayment)
                .SetValidator(new PaymentDtoValidator())
                .When(x => x.ProcessedPayment != null);
        }
    }

    // Validator for PaymentValidationDto
    public class PaymentValidationDtoValidator : AbstractValidator<PaymentValidationDto>
    {
        public PaymentValidationDtoValidator()
        {
            RuleFor(x => x.PaymentReference)
                .NotEmpty().WithMessage("Payment reference is required")
                .Length(5, 50).WithMessage("Payment reference must be between 5 and 50 characters");

            RuleFor(x => x.Errors)
                .NotNull().WithMessage("Errors list cannot be null");
        }
    }

    // Validator for PaymentSummaryDto
    public class PaymentSummaryDtoValidator : AbstractValidator<PaymentSummaryDto>
    {
        public PaymentSummaryDtoValidator()
        {
            RuleFor(x => x.StudentNumber)
                .NotEmpty().WithMessage("Student number is required")
                .Length(5, 20).WithMessage("Student number must be between 5 and 20 characters");

            RuleFor(x => x.StudentName)
                .NotEmpty().WithMessage("Student name is required")
                .Length(2, 100).WithMessage("Student name must be between 2 and 100 characters");

            RuleFor(x => x.TotalPayments)
                .GreaterThanOrEqualTo(0).WithMessage("Total payments cannot be negative");

            RuleFor(x => x.TotalAmountPaid)
                .GreaterThanOrEqualTo(0).WithMessage("Total amount paid cannot be negative")
                .LessThanOrEqualTo(10000000).WithMessage("Total amount paid cannot exceed 10,000,000");

            RuleFor(x => x.OutstandingBalance)
                .GreaterThanOrEqualTo(0).WithMessage("Outstanding balance cannot be negative")
                .LessThanOrEqualTo(10000000).WithMessage("Outstanding balance cannot exceed 10,000,000");

            RuleFor(x => x.LastPaymentDate)
                .NotEmpty().WithMessage("Last payment date is required");

            RuleFor(x => x.RecentPayments)
                .NotNull().WithMessage("Recent payments list cannot be null");
        }
    }

    // Validator for BatchPaymentDto
    public class BatchPaymentDtoValidator : AbstractValidator<BatchPaymentDto>
    {
        public BatchPaymentDtoValidator()
        {
            RuleFor(x => x.Payments)
                .NotEmpty().WithMessage("At least one payment is required")
                .Must(payments => payments.Count <= 100).WithMessage("Cannot process more than 100 payments at once");

            RuleForEach(x => x.Payments)
                .SetValidator(new CreatePaymentDtoValidator());
        }
    }

    // Validator for BatchPaymentResultDto
    public class BatchPaymentResultDtoValidator : AbstractValidator<BatchPaymentResultDto>
    {
        public BatchPaymentResultDtoValidator()
        {
            RuleFor(x => x.TotalProcessed)
                .GreaterThanOrEqualTo(0).WithMessage("Total processed cannot be negative");

            RuleFor(x => x.SuccessfulPayments)
                .GreaterThanOrEqualTo(0).WithMessage("Successful payments cannot be negative");

            RuleFor(x => x.FailedPayments)
                .GreaterThanOrEqualTo(0).WithMessage("Failed payments cannot be negative");

            RuleFor(x => x.TotalProcessed)
                .Equal(x => x.SuccessfulPayments + x.FailedPayments)
                .When(x => x.TotalProcessed > 0)
                .WithMessage("Total processed must equal successful plus failed payments");

            RuleFor(x => x.Results)
                .NotNull().WithMessage("Results list cannot be null");

            RuleFor(x => x.Errors)
                .NotNull().WithMessage("Errors list cannot be null");
        }
    }

    // Validator for BankPaymentDataDto
    public class BankPaymentDataDtoValidator : AbstractValidator<BankPaymentDataDto>
    {
        public BankPaymentDataDtoValidator()
        {
            RuleFor(x => x.PaymentReference)
                .NotEmpty().WithMessage("Payment reference is required")
                .Length(5, 50).WithMessage("Payment reference must be between 5 and 50 characters");

            RuleFor(x => x.StudentNumber)
                .NotEmpty().WithMessage("Student number is required")
                .Length(5, 20).WithMessage("Student number must be between 5 and 20 characters")
                .Matches(@"^[A-Z0-9]+$").WithMessage("Student number must contain only uppercase letters and numbers");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0")
                .LessThanOrEqualTo(1000000).WithMessage("Amount cannot exceed 1,000,000")
                .PrecisionScale(10, 2, false).WithMessage("Amount cannot have more than 2 decimal places");

            RuleFor(x => x.TransactionDate)
                .NotEmpty().WithMessage("Transaction date is required")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Transaction date cannot be in the future")
                .GreaterThan(DateTime.UtcNow.AddYears(-10)).WithMessage("Transaction date cannot be more than 10 years ago");

            RuleFor(x => x.BankTransactionId)
                .NotEmpty().WithMessage("Bank transaction ID is required")
                .Length(5, 100).WithMessage("Bank transaction ID must be between 5 and 100 characters");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required")
                .Must(BeValidStatus).WithMessage("Status must be one of: Pending, Completed, Failed, Cancelled");
        }

        private bool BeValidStatus(string status)
        {
            var validStatuses = new[] { "Pending", "Completed", "Failed", "Cancelled" };
            return validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
        }
    }

    // Validator for ReconciliationResultDto
    public class ReconciliationResultDtoValidator : AbstractValidator<ReconciliationResultDto>
    {
        public ReconciliationResultDtoValidator()
        {
            RuleFor(x => x.TotalBankRecords)
                .GreaterThanOrEqualTo(0).WithMessage("Total bank records cannot be negative");

            RuleFor(x => x.MatchedPayments)
                .GreaterThanOrEqualTo(0).WithMessage("Matched payments cannot be negative");

            RuleFor(x => x.UnmatchedPayments)
                .GreaterThanOrEqualTo(0).WithMessage("Unmatched payments cannot be negative");

            RuleFor(x => x.TotalBankRecords)
                .Equal(x => x.MatchedPayments + x.UnmatchedPayments)
                .When(x => x.TotalBankRecords > 0)
                .WithMessage("Total bank records must equal matched plus unmatched payments");

            RuleFor(x => x.Discrepancies)
                .NotNull().WithMessage("Discrepancies list cannot be null");

            RuleFor(x => x.ReconciliationDate)
                .NotEmpty().WithMessage("Reconciliation date is required")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Reconciliation date cannot be in the future");
        }
    }



    // Custom validator for amount
    public class AmountValidator : AbstractValidator<decimal>
    {
        public AmountValidator()
        {
            RuleFor(x => x)
                .GreaterThan(0).WithMessage("Amount must be greater than 0")
                .LessThanOrEqualTo(1000000).WithMessage("Amount cannot exceed 1,000,000")
                .PrecisionScale(10, 2, false).WithMessage("Amount cannot have more than 2 decimal places");
        }
    }
}