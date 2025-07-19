using AutoMapper;
using HermesBanking.Core.Application.DTOs.Beneficiary;
using HermesBanking.Core.Application.ViewModels.Beneficiary;

namespace HermesBanking.Core.Application.Mappings.DTOsAndViewModels
{
    public class BeneficiaryDTOMappingProfile : Profile
    {
        public BeneficiaryDTOMappingProfile()
        {
            CreateMap<BeneficiaryDTO, BeneficiaryViewModel>().ReverseMap();

            CreateMap<SaveBeneficiaryViewModel, BeneficiaryDTO>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.Ignore())
                .ForMember(dest => dest.LastName, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            CreateMap<BeneficiaryDTO, DeleteBeneficiaryViewModel>()
                 .ForMember(dest => dest.AccountNumber, opt => opt.MapFrom(src => src.BeneficiaryAccountNumber))
                 .ReverseMap();
        }
    }
}