using AutoMapper;
using HermesBanking.Core.Application.DTOs.CreditCard;
using HermesBanking.Core.Application.ViewModels.CreditCard;

namespace HermesBanking.Core.Application.Mappings.DTOsAndViewModels
{
    public class CreditCardDTOMappingProfile : Profile
    {
        public CreditCardDTOMappingProfile()
        {
            CreateMap<CreditCardDTO, CreditCardViewModel>()
                .ReverseMap();

            CreateMap<SaveCreditCardViewModel, CreditCardDTO>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.TotalOwedAmount, opt => opt.MapFrom(src => 0));

            CreateMap<CreditCardDTO, SaveCreditCardViewModel>();

            CreateMap<SaveCreditCardViewModel, CreditCardDTO>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

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