// Purpose: Validation rules for Payment DTOs using FluentValidation
using FluentValidation;
using xyz_university_payment_api.DTOs;

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
                .Length(10, 50).WithMessage("Payment reference must be between 10 and 50 characters")
                .Matches(@"^[A-Z0-9\-_]+$").WithMessage("Payment reference must contain only uppercase letters, numbers, hyphens, and underscores");

            RuleFor(x => x.AmountPaid)
                .GreaterThan(0).WithMessage("Amount paid must be greater than 0")
                .LessThanOrEqualTo(1000000).WithMessage("Amount paid cannot exceed 1,000,000")
                .PrecisionScale(10, 2, false).WithMessage("Amount paid cannot have more than 2 decimal places");

            RuleFor(x => x.PaymentDate)
                .NotEmpty().WithMessage("Payment date is required")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Payment date cannot be in the future")
                .GreaterThanOrEqualTo(DateTime.UtcNow.AddYears(-5)).WithMessage("Payment date cannot be more than 5 years ago");
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
                .Length(10, 50).WithMessage("Payment reference must be between 10 and 50 characters");

            RuleFor(x => x.AmountPaid)
                .GreaterThan(0).WithMessage("Amount paid must be greater than 0")
                .LessThanOrEqualTo(1000000).WithMessage("Amount paid cannot exceed 1,000,000");

            RuleFor(x => x.PaymentDate)
                .NotEmpty().WithMessage("Payment date is required");

            RuleFor(x => x.DateReceived)
                .NotEmpty().WithMessage("Date received is required");
        }
    }

    // Validator for PaymentValidationDto
    public class PaymentValidationDtoValidator : AbstractValidator<PaymentValidationDto>
    {
        public PaymentValidationDtoValidator()
        {
            RuleFor(x => x.PaymentReference)
                .NotEmpty().WithMessage("Payment reference is required")
                .Length(10, 50).WithMessage("Payment reference must be between 10 and 50 characters");

            RuleFor(x => x.Errors)
                .NotNull().WithMessage("Errors list cannot be null");
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

    // Validator for BankPaymentDataDto
    public class BankPaymentDataDtoValidator : AbstractValidator<BankPaymentDataDto>
    {
        public BankPaymentDataDtoValidator()
        {
            RuleFor(x => x.PaymentReference)
                .NotEmpty().WithMessage("Payment reference is required")
                .Length(10, 50).WithMessage("Payment reference must be between 10 and 50 characters");

            RuleFor(x => x.StudentNumber)
                .NotEmpty().WithMessage("Student number is required")
                .Length(5, 20).WithMessage("Student number must be between 5 and 20 characters");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0")
                .LessThanOrEqualTo(1000000).WithMessage("Amount cannot exceed 1,000,000");

            RuleFor(x => x.TransactionDate)
                .NotEmpty().WithMessage("Transaction date is required")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Transaction date cannot be in the future");

            RuleFor(x => x.BankTransactionId)
                .NotEmpty().WithMessage("Bank transaction ID is required")
                .Length(5, 50).WithMessage("Bank transaction ID must be between 5 and 50 characters");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required")
                .Must(status => new[] { "Pending", "Completed", "Failed", "Cancelled" }.Contains(status))
                .WithMessage("Status must be one of: Pending, Completed, Failed, Cancelled");
        }
    }

    // Custom validator for payment reference format
    public class PaymentReferenceValidator : AbstractValidator<string>
    {
        public PaymentReferenceValidator()
        {
            RuleFor(x => x)
                .NotEmpty().WithMessage("Payment reference is required")
                .Length(10, 50).WithMessage("Payment reference must be between 10 and 50 characters")
                .Matches(@"^[A-Z0-9\-_]+$").WithMessage("Payment reference must contain only uppercase letters, numbers, hyphens, and underscores")
                .Must(BeUniquePaymentReference).WithMessage("Payment reference must be unique");
        }

        private bool BeUniquePaymentReference(string paymentReference)
        {
            // This would typically check against the database
            // For now, we'll return true and implement this in the service layer
            return true;
        }
    }
}