using AutoMapper;
using HermesBanking.Core.Application.DTOs.CreditCard;
using HermesBanking.Core.Application.ViewModels.CreditCard;

namespace HermesBanking.Core.Application.Mappings.DTOsAndViewModels
{
    public class CreditCardMappingProfile : Profile
    {
        public CreditCardMappingProfile()
        {
            CreateMap<CreditCardDTO, CreditCardViewModel>()
                .ReverseMap();

            CreateMap<CreditCardDTO, SaveCreditCardViewModel>()
                .ReverseMap()
                .ForMember(dest => dest.CardId, opt => opt.Ignore())
                .ForMember(dest => dest.CVC, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.AdminFullName, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByAdminId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalOwedAmount, opt => opt.Ignore());

            CreateMap<CreditCardDTO, CancelCreditCardViewModel>()
                .ReverseMap()
                .ForMember(dest => dest.CardId, opt => opt.Ignore())
                .ForMember(dest => dest.ClientId, opt => opt.Ignore())
                .ForMember(dest => dest.ClientFullName, opt => opt.Ignore())
                .ForMember(dest => dest.CreditLimit, opt => opt.Ignore())
                .ForMember(dest => dest.TotalOwedAmount, opt => opt.Ignore())
                .ForMember(dest => dest.CVC, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.ExpirationDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByAdminId, opt => opt.Ignore())
                .ForMember(dest => dest.AdminFullName, opt => opt.Ignore());
        }
    }
}
