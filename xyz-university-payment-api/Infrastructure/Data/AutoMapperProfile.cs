// Purpose: AutoMapper configuration for mapping between models and DTOs
using AutoMapper;
using xyz_university_payment_api.Core.Application.DTOs;
using xyz_university_payment_api.Core.Domain.Entities;

namespace xyz_university_payment_api.Infrastructure.Data
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Existing mappings
            CreateMap<Student, StudentDto>().ReverseMap();
            CreateMap<Student, CreateStudentDto>().ReverseMap();
            CreateMap<Student, UpdateStudentDto>().ReverseMap();

            CreateMap<PaymentNotification, PaymentDto>().ReverseMap();
            CreateMap<PaymentNotification, CreatePaymentDto>()
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod))
                .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.TransactionId))
                .ForMember(dest => dest.ReceiptNumber, opt => opt.MapFrom(src => src.ReceiptNumber))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ReverseMap();

            CreateMap<FeeSchedule, FeeScheduleDto>().ReverseMap();
            CreateMap<FeeSchedule, CreateFeeScheduleDto>().ReverseMap();
            CreateMap<FeeSchedule, UpdateFeeScheduleDto>().ReverseMap();

            CreateMap<StudentBalance, StudentBalanceDto>().ReverseMap();
            CreateMap<StudentBalance, CreateStudentBalanceDto>().ReverseMap();
            CreateMap<StudentBalance, UpdateStudentBalanceDto>().ReverseMap();

            CreateMap<PaymentPlan, PaymentPlanDto>().ReverseMap();
            CreateMap<PaymentPlan, CreatePaymentPlanDto>().ReverseMap();
            CreateMap<PaymentPlan, UpdatePaymentPlanDto>().ReverseMap();

            // Fee Management mappings
            CreateMap<FeeCategory, FeeCategoryDto>().ReverseMap();
            CreateMap<FeeCategory, CreateFeeCategoryDto>().ReverseMap();
            CreateMap<FeeCategory, UpdateFeeCategoryDto>().ReverseMap();

            CreateMap<FeeStructure, FeeStructureDto>().ReverseMap();
            CreateMap<FeeStructure, CreateFeeStructureDto>().ReverseMap();
            CreateMap<FeeStructure, UpdateFeeStructureDto>().ReverseMap();

            CreateMap<FeeStructureItem, FeeStructureItemDto>()
                .ForMember(dest => dest.FeeCategoryName, opt => opt.MapFrom(src => src.FeeCategory.Name))
                .ReverseMap();
            CreateMap<FeeStructureItem, CreateFeeStructureItemDto>().ReverseMap();

            CreateMap<AdditionalFee, AdditionalFeeDto>().ReverseMap();
            CreateMap<AdditionalFee, CreateAdditionalFeeDto>().ReverseMap();
            CreateMap<AdditionalFee, UpdateAdditionalFeeDto>().ReverseMap();

            CreateMap<StudentFeeAssignment, StudentFeeAssignmentDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.FullName))
                .ForMember(dest => dest.FeeStructureName, opt => opt.MapFrom(src => src.FeeStructure.Name))
                .ReverseMap();
            CreateMap<StudentFeeAssignment, AssignFeeStructureDto>().ReverseMap();

            CreateMap<StudentFeeBalance, StudentFeeBalanceDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.FullName))
                .ForMember(dest => dest.FeeCategoryName, opt => opt.MapFrom(src => src.FeeStructureItem.FeeCategory.Name))
                .ReverseMap();

            CreateMap<StudentAdditionalFee, StudentAdditionalFeeDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.FullName))
                .ForMember(dest => dest.AdditionalFeeName, opt => opt.MapFrom(src => src.AdditionalFee.Name))
                .ReverseMap();
        }
    }
}