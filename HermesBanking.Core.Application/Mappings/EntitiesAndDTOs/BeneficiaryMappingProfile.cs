using AutoMapper;
using HermesBanking.Core.Application.DTOs.Beneficiary;
using HermesBanking.Core.Domain.Entities;

namespace HermesBanking.Core.Application.Mappings.EntitiesAndDTOs
{
    public class BeneficiaryMappingProfile : Profile
    {
        public BeneficiaryMappingProfile()
        {
            CreateMap<Beneficiary, BeneficiaryDTO>().ReverseMap();
        }
    }
}