using AutoMapper;
using HermesBanking.Core.Application.DTOs.SavingsAccount;
using HermesBanking.Core.Application.ViewModels.SavingsAccount;

namespace HermesBanking.Core.Application.Mappings.DTOsAndViewModels
{
    public class SavingsAccountDTOMappingProfile : Profile
    {
        public SavingsAccountDTOMappingProfile()
        {
            CreateMap<SavingsAccountDTO, SavingsAccountViewModel>().ReverseMap();

            CreateMap<SavingsAccountDTO, SaveSavingsAccountViewModel>().ReverseMap();

            CreateMap<SavingsAccountDTO, DeleteSavingsAccountViewModel>()
               .ReverseMap()
               .ForMember(dest => dest.IsActive, opt => opt.Ignore())
               .ForMember(dest => dest.ClientId, opt => opt.Ignore())
               .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
               .ForMember(dest => dest.AccountNumber, opt => opt.Ignore())
               .ForMember(dest => dest.AccountType, opt => opt.Ignore())
               .ForMember(dest => dest.Balance, opt => opt.Ignore())
               .ForMember(dest => dest.CreatedByAdminId, opt => opt.Ignore())
               .ForMember(dest => dest.AdminFullName, opt => opt.Ignore())
               .ForMember(dest => dest.ClientFullName, opt => opt.Ignore());
        }
    }
}
