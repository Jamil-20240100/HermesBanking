using AutoMapper;
using HermesBanking.Core.Application.DTOs.CreditCard;
using HermesBanking.Core.Domain.Entities;

namespace HermesBanking.Core.Application.Mappings.EntitiesAndDTOs
{
    public class CreditCardMappingProfile : Profile
    {
        public CreditCardMappingProfile()
        {
            CreateMap<CreditCard, CreditCardDTO>().ReverseMap();
        }
    }
}
