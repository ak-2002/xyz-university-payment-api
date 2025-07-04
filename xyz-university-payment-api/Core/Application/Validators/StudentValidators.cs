// Purpose: Validation rules for Student DTOs using FluentValidation
using FluentValidation;
using xyz_university_payment_api.Core.Application.DTOs;

namespace xyz_university_payment_api.Validators
{
    // Validator for CreateStudentDto
    public class CreateStudentDtoValidator : AbstractValidator<CreateStudentDto>
    {
        public CreateStudentDtoValidator()
        {
            RuleFor(x => x.StudentNumber)
                .NotEmpty().WithMessage("Student number is required")
                .Length(5, 20).WithMessage("Student number must be between 5 and 20 characters")
                .Matches(@"^[A-Z0-9]+$").WithMessage("Student number must contain only uppercase letters and numbers")
                .Must(BeUniqueStudentNumber).WithMessage("Student number must be unique");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .Length(2, 100).WithMessage("Full name must be between 2 and 100 characters")
                .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("Full name can only contain letters, spaces, hyphens, and apostrophes");

            RuleFor(x => x.Program)
                .NotEmpty().WithMessage("Program is required")
                .Length(2, 100).WithMessage("Program must be between 2 and 100 characters")
                .Must(BeValidProgram).WithMessage("Program must be a valid program offered by the university");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
                .Must(BeUniqueEmail).WithMessage("Email must be unique");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number must be a valid international format")
                .Must(BeUniquePhoneNumber).WithMessage("Phone number must be unique");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required")
                .LessThan(DateTime.UtcNow.AddYears(-16)).WithMessage("Student must be at least 16 years old")
                .GreaterThan(DateTime.UtcNow.AddYears(-100)).WithMessage("Date of birth is invalid");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required")
                .Length(10, 200).WithMessage("Address must be between 10 and 200 characters");
        }

        private bool BeUniqueStudentNumber(string studentNumber)
        {
            // This would typically check against the database
            // For now, we'll return true and implement this in the service layer
            return true;
        }

        private bool BeValidProgram(string program)
        {
            var validPrograms = new[] { "Computer Science", "Engineering", "Business", "Medicine", "Arts", "Science" };
            return validPrograms.Contains(program, StringComparer.OrdinalIgnoreCase);
        }

        private bool BeUniqueEmail(string email)
        {
            // This would typically check against the database
            return true;
        }

        private bool BeUniquePhoneNumber(string phoneNumber)
        {
            // This would typically check against the database
            return true;
        }
    }

    // Validator for UpdateStudentDto
    public class UpdateStudentDtoValidator : AbstractValidator<UpdateStudentDto>
    {
        public UpdateStudentDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .Length(2, 100).WithMessage("Full name must be between 2 and 100 characters")
                .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("Full name can only contain letters, spaces, hyphens, and apostrophes");

            RuleFor(x => x.Program)
                .NotEmpty().WithMessage("Program is required")
                .Length(2, 100).WithMessage("Program must be between 2 and 100 characters")
                .Must(BeValidProgram).WithMessage("Program must be a valid program offered by the university");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number must be a valid international format");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required")
                .Length(10, 200).WithMessage("Address must be between 10 and 200 characters");
        }

        private bool BeValidProgram(string program)
        {
            var validPrograms = new[] { "Computer Science", "Engineering", "Business", "Medicine", "Arts", "Science" };
            return validPrograms.Contains(program, StringComparer.OrdinalIgnoreCase);
        }
    }

    // Validator for StudentDto
    public class StudentDtoValidator : AbstractValidator<StudentDto>
    {
        public StudentDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Student ID must be greater than 0");

            RuleFor(x => x.StudentNumber)
                .NotEmpty().WithMessage("Student number is required")
                .Length(5, 20).WithMessage("Student number must be between 5 and 20 characters");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .Length(2, 100).WithMessage("Full name must be between 2 and 100 characters");

            RuleFor(x => x.Program)
                .NotEmpty().WithMessage("Program is required")
                .Length(2, 100).WithMessage("Program must be between 2 and 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required")
                .Length(10, 200).WithMessage("Address must be between 10 and 200 characters");

            RuleFor(x => x.CreatedAt)
                .NotEmpty().WithMessage("Created date is required");
        }
    }

    // Validator for StudentSearchDto
    public class StudentSearchDtoValidator : AbstractValidator<StudentSearchDto>
    {
        public StudentSearchDtoValidator()
        {
            RuleFor(x => x.StudentNumber)
                .MaximumLength(20).WithMessage("Student number cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.StudentNumber));

            RuleFor(x => x.FullName)
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.FullName));

            RuleFor(x => x.Program)
                .MaximumLength(100).WithMessage("Program cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Program));

            RuleFor(x => x.EnrollmentDateFrom)
                .LessThanOrEqualTo(x => x.EnrollmentDateTo)
                .When(x => x.EnrollmentDateFrom.HasValue && x.EnrollmentDateTo.HasValue)
                .WithMessage("Enrollment date from must be less than or equal to enrollment date to");
        }
    }

    // Validator for StudentValidationDto
    public class StudentValidationDtoValidator : AbstractValidator<StudentValidationDto>
    {
        public StudentValidationDtoValidator()
        {
            RuleFor(x => x.StudentNumber)
                .NotEmpty().WithMessage("Student number is required")
                .Length(5, 20).WithMessage("Student number must be between 5 and 20 characters");

            RuleFor(x => x.Errors)
                .NotNull().WithMessage("Errors list cannot be null");
        }
    }



    // Validator for StudentFilterDtoV3
    public class StudentFilterDtoV3Validator : AbstractValidator<StudentFilterDtoV3>
    {
        public StudentFilterDtoV3Validator()
        {
            RuleFor(x => x.StudentNumber)
                .MaximumLength(20).WithMessage("Student number cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.StudentNumber));

            RuleFor(x => x.FullName)
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.FullName));

            RuleFor(x => x.Program)
                .MaximumLength(100).WithMessage("Program cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Program));

            RuleFor(x => x.SearchTerm)
                .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));

            RuleFor(x => x.CreatedFrom)
                .LessThanOrEqualTo(x => x.CreatedTo)
                .When(x => x.CreatedFrom.HasValue && x.CreatedTo.HasValue)
                .WithMessage("Created from date must be less than or equal to created to date");

            RuleFor(x => x.MinTotalPayments)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum total payments must be greater than or equal to 0")
                .When(x => x.MinTotalPayments.HasValue);

            RuleFor(x => x.MaxTotalPayments)
                .GreaterThanOrEqualTo(0).WithMessage("Maximum total payments must be greater than or equal to 0")
                .When(x => x.MaxTotalPayments.HasValue);

            RuleFor(x => x.MaxTotalPayments)
                .GreaterThanOrEqualTo(x => x.MinTotalPayments)
                .When(x => x.MaxTotalPayments.HasValue && x.MinTotalPayments.HasValue)
                .WithMessage("Maximum total payments must be greater than or equal to minimum total payments");
        }
    }
}