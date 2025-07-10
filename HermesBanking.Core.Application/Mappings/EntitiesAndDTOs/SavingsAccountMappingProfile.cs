using AutoMapper;
using HermesBanking.Core.Application.DTOs.SavingsAccount;
using HermesBanking.Core.Domain.Entities;

namespace HermesBanking.Core.Application.Mappings.EntitiesAndDTOs
{
    public class SavingsAccountMappingProfile : Profile
    {
        public SavingsAccountMappingProfile()
        {
            CreateMap<SavingsAccount, SavingsAccountDTO>().ReverseMap();
        }
    }
}
