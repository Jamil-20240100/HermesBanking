using AutoMapper;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Application.ViewModels.SavingsAccount;

namespace HermesBanking.Core.Application.Mappings.DTOsAndViewModels
{
    public class SavingsAccountViewModelMappingProfile : Profile
    {
        public SavingsAccountViewModelMappingProfile()
        {
            CreateMap<SavingsAccount, SavingsAccountViewModel>()
                .ForMember(dest => dest.AccountNumber, opt => opt.MapFrom(src => src.AccountNumber))
                .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.Balance))
                .ForMember(dest => dest.AccountType, opt => opt.MapFrom(src => src.AccountType))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
        }
    }
}
