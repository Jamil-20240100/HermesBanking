using AutoMapper;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Application.DTOs.SavingsAccount;

namespace HermesBanking.Core.Application.Mappings.DTOsAndViewModels
{
    public class SavingsAccountEntityToDTOMappingProfile : Profile
    {
        public SavingsAccountEntityToDTOMappingProfile()
        {
            CreateMap<SavingsAccount, SavingsAccountDTO>().ReverseMap();
        }
    }
}