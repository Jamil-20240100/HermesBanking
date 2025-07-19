using AutoMapper;
using HermesBanking.Core.Application.DTOs.Beneficiary;
using HermesBanking.Core.Application.ViewModels.Beneficiary;
using HermesBanking.Core.Domain.Entities;

namespace HermesBanking.Core.Application.Mappings.DTOsAndViewModels
{
    public class BeneficiaryDTOMappingProfile : Profile
    {
        public BeneficiaryDTOMappingProfile()
        {
            // Mapeo de Beneficiary (entidad) a BeneficiaryViewModel
            CreateMap<Beneficiary, BeneficiaryViewModel>()
                .ForMember(dest => dest.BeneficiaryAccountNumber, opt => opt.MapFrom(src => src.BeneficiaryAccountNumber))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            // Mapeo de BeneficiaryViewModel a Beneficiary (en caso de que necesites crear o actualizar)
            CreateMap<BeneficiaryViewModel, Beneficiary>()
                .ForMember(dest => dest.BeneficiaryAccountNumber, opt => opt.MapFrom(src => src.BeneficiaryAccountNumber))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));

            // Mapeo existente entre BeneficiaryDTO y BeneficiaryViewModel
            CreateMap<BeneficiaryDTO, BeneficiaryViewModel>().ReverseMap();

            // Mapeo entre SaveBeneficiaryViewModel y BeneficiaryDTO
            CreateMap<SaveBeneficiaryViewModel, BeneficiaryDTO>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.Ignore())
                .ForMember(dest => dest.LastName, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            // Mapeo entre BeneficiaryDTO y DeleteBeneficiaryViewModel
            CreateMap<BeneficiaryDTO, DeleteBeneficiaryViewModel>()
                .ForMember(dest => dest.AccountNumber, opt => opt.MapFrom(src => src.BeneficiaryAccountNumber))
                .ReverseMap();
        }
    }
}
