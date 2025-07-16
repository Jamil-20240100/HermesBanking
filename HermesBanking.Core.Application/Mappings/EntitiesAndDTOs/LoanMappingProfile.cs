using AutoMapper;
using HermesBanking.Core.Application.DTOs.Loan;
using HermesBanking.Core.Application.ViewModels.Loan;
using HermesBanking.Core.Domain.Entities;

namespace HermesBanking.Core.Application.Mappings.EntitiesAndDTOs
{
    public class LoanMappingProfile : Profile
    {
        public LoanMappingProfile()
        {
            CreateMap<Loan, LoanDTO>().ReverseMap();

            CreateMap<AmortizationInstallment, AmortizationInstallmentDTO>().ReverseMap();

            CreateMap<LoanDTO, EditLoanInterestRateViewModel>()
           .ForMember(dest => dest.LoanId, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.NewInterestRate, opt => opt.MapFrom(src => src.InterestRate));     
            
            CreateMap<AssignLoanViewModel, CreateLoanDTO>()
                .ForMember(dest => dest.AssignedByAdminId, opt => opt.MapFrom(src => src.CreatedByAdminId))
                .ForMember(dest => dest.AdminFullName, opt => opt.MapFrom(src => src.AdminFullName));


            CreateMap<CreateLoanDTO, Loan>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.LoanIdentifier, opt => opt.MapFrom(src => Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()))
                .ForMember(dest => dest.PaidInstallments, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.PendingAmount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.IsOverdue, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.MonthlyInstallmentValue, opt => opt.Ignore())
                .ForMember(dest => dest.TotalInstallments, opt => opt.MapFrom(src => src.LoanTermMonths))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.AmortizationInstallments, opt => opt.Ignore());

            CreateMap<LoanDTO, LoanViewModel>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive ? src.IsOverdue ? "En Mora" : "Activo" : "Completado"));

            CreateMap<LoanDTO, LoanDetailsViewModel>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.LoanIdentifier, opt => opt.MapFrom(src => src.LoanIdentifier))
                 .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.ClientId))
                 .ForMember(dest => dest.ClientFullName, opt => opt.MapFrom(src => src.ClientFullName))
                 .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                 .ForMember(dest => dest.InterestRate, opt => opt.MapFrom(src => src.InterestRate))
                 .ForMember(dest => dest.LoanTermMonths, opt => opt.MapFrom(src => src.LoanTermMonths))
                 .ForMember(dest => dest.MonthlyInstallmentValue, opt => opt.MapFrom(src => src.MonthlyInstallmentValue))
                 .ForMember(dest => dest.TotalInstallments, opt => opt.MapFrom(src => src.TotalInstallments))
                 .ForMember(dest => dest.PaidInstallments, opt => opt.MapFrom(src => src.PaidInstallments))
                 .ForMember(dest => dest.PendingAmount, opt => opt.MapFrom(src => src.PendingAmount))
                 .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                 .ForMember(dest => dest.IsOverdue, opt => opt.MapFrom(src => src.IsOverdue))
                 .ForMember(dest => dest.AssignedByAdminId, opt => opt.MapFrom(src => src.AssignedByAdminId))
                 .ForMember(dest => dest.AdminFullName, opt => opt.MapFrom(src => src.AdminFullName))
                 .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                 .ForMember(dest => dest.CompletedAt, opt => opt.MapFrom(src => src.CompletedAt))
                 .ForMember(dest => dest.AmortizationSchedule, opt => opt.MapFrom(src => src.AmortizationInstallments));

            CreateMap<AmortizationInstallmentDTO, AmortizationInstallmentViewModel>();

            CreateMap<UpdateLoanInterestRateDTO, Loan>();
        }
    }
}