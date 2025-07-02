// Purpose: AutoMapper configuration for mapping between models and DTOs
using AutoMapper;
using xyz_university_payment_api.Core.Domain.Entities;
using xyz_university_payment_api.Core.Application.DTOs;

namespace xyz_university_payment_api.Infrastructure.Data
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Payment mappings
            CreateMap<PaymentNotification, PaymentDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => string.Empty)) // Will be populated from service
                .ForMember(dest => dest.StudentProgram, opt => opt.MapFrom(src => string.Empty)); // Will be populated from service

            CreateMap<CreatePaymentDto, PaymentNotification>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.DateReceived, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<PaymentDto, PaymentNotification>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // Student mappings
            CreateMap<Student, StudentDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => string.Empty)) // Will be added to Student model later
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => string.Empty)) // Will be added to Student model later
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => DateTime.UtcNow)) // Will be added to Student model later
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => string.Empty)) // Will be added to Student model later
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => (DateTime?)null));

            CreateMap<CreateStudentDto, Student>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<UpdateStudentDto, Student>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.StudentNumber, opt => opt.Ignore());

            CreateMap<Student, StudentSummaryDto>()
                .ForMember(dest => dest.TotalPayments, opt => opt.MapFrom(src => 0)) // Will be calculated in service
                .ForMember(dest => dest.TotalAmountPaid, opt => opt.MapFrom(src => 0)) // Will be calculated in service
                .ForMember(dest => dest.OutstandingBalance, opt => opt.MapFrom(src => 0)) // Will be calculated in service
                .ForMember(dest => dest.LastPaymentDate, opt => opt.MapFrom(src => DateTime.MinValue)) // Will be calculated in service
                .ForMember(dest => dest.EnrollmentDate, opt => opt.MapFrom(src => DateTime.UtcNow)); // Will be added to Student model later

            // Payment response mappings
            CreateMap<PaymentNotification, PaymentResponseDto>()
                .ForMember(dest => dest.Success, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => "Payment processed successfully"))
                .ForMember(dest => dest.StudentExists, opt => opt.MapFrom(src => false)) // Will be determined in service
                .ForMember(dest => dest.StudentIsActive, opt => opt.MapFrom(src => false)) // Will be determined in service
                .ForMember(dest => dest.ProcessedPayment, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.ValidationErrors, opt => opt.MapFrom(src => new List<string>()));

            // Payment validation mappings
            CreateMap<PaymentNotification, PaymentValidationDto>()
                .ForMember(dest => dest.IsValid, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Errors, opt => opt.MapFrom(src => new List<string>()));

            // Batch payment mappings
            CreateMap<BatchPaymentDto, BatchPaymentResultDto>()
                .ForMember(dest => dest.TotalProcessed, opt => opt.MapFrom(src => src.Payments.Count))
                .ForMember(dest => dest.SuccessfulPayments, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.FailedPayments, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.Results, opt => opt.MapFrom(src => new List<PaymentResponseDto>()))
                .ForMember(dest => dest.Errors, opt => opt.MapFrom(src => new List<string>()));

            // Bank payment data mappings
            CreateMap<BankPaymentDataDto, PaymentNotification>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.StudentNumber, opt => opt.MapFrom(src => src.StudentNumber))
                .ForMember(dest => dest.PaymentReference, opt => opt.MapFrom(src => src.PaymentReference))
                .ForMember(dest => dest.AmountPaid, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => src.TransactionDate))
                .ForMember(dest => dest.DateReceived, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Reverse mappings for updates
            CreateMap<PaymentDto, PaymentNotification>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<StudentDto, Student>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // Custom mapping for payment summary with recent payments
            CreateMap<Student, PaymentSummaryDto>()
                .ForMember(dest => dest.RecentPayments, opt => opt.MapFrom(src => new List<PaymentDto>())) // Will be populated from service
                .ForMember(dest => dest.TotalPayments, opt => opt.MapFrom(src => 0)) // Will be calculated in service
                .ForMember(dest => dest.TotalAmountPaid, opt => opt.MapFrom(src => 0)) // Will be calculated in service
                .ForMember(dest => dest.OutstandingBalance, opt => opt.MapFrom(src => 0)) // Will be calculated in service
                .ForMember(dest => dest.LastPaymentDate, opt => opt.MapFrom(src => DateTime.MinValue)); // Will be calculated in service

            // Custom mapping for student statistics
            CreateMap<IEnumerable<Student>, StudentStatisticsDto>()
                .ForMember(dest => dest.TotalStudents, opt => opt.MapFrom(src => src.Count()))
                .ForMember(dest => dest.ActiveStudents, opt => opt.MapFrom(src => src.Count(s => s.IsActive)))
                .ForMember(dest => dest.InactiveStudents, opt => opt.MapFrom(src => src.Count(s => !s.IsActive)))
                .ForMember(dest => dest.StudentsByProgram, opt => opt.MapFrom(src => 
                    src.GroupBy(s => s.Program)
                       .ToDictionary(g => g.Key, g => g.Count())))
                .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => 0)) // Will be calculated in service
                .ForMember(dest => dest.AveragePaymentAmount, opt => opt.MapFrom(src => 0)) // Will be calculated in service
                .ForMember(dest => dest.StudentsWithOutstandingBalance, opt => opt.MapFrom(src => 0)); // Will be calculated in service
        }
    }
} 