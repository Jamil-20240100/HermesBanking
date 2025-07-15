using AutoMapper;
using HermesBanking.Core.Application.DTOs.Cashier;
using HermesBanking.Core.Application.ViewModels.Cashier;

namespace HermesBanking.Core.Application.Mappings.DTOsAndViewModels
{
    public class CashierDtoMappingProfile : Profile
    {
        public CashierDtoMappingProfile()
        {
            CreateMap<DepositViewModel, DepositDto>().ReverseMap();
            CreateMap<WithdrawViewModel, WithdrawDto>().ReverseMap();
            CreateMap<ThirdPartyTransferViewModel, ThirdPartyTransferDto>().ReverseMap();
            CreateMap<PagoPrestamoViewModel, LoanPaymentDto>().ReverseMap();
            CreateMap<PagoTarjetaCreditoViewModel, CreditCardPaymentDto>().ReverseMap();

            // Confirmaciones
            CreateMap<ConfirmDepositViewModel, DepositDto>();
            CreateMap<ConfirmWithdrawViewModel, WithdrawDto>();
            CreateMap<ConfirmThirdPartyTransferViewModel, ThirdPartyTransferDto>();
            CreateMap<ConfirmPagoPrestamoViewModel, LoanPaymentDto>();
            CreateMap<ConfirmPagoTarjetaCreditoViewModel, CreditCardPaymentDto>();
        }
    }
}
