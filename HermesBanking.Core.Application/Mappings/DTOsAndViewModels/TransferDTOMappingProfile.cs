using AutoMapper;
using HermesBanking.Core.Application.DTOs.Transfer;
using HermesBanking.Core.Application.ViewModels.Transfer;

namespace HermesBanking.Core.Application.Mappings.DTOsAndViewModels
{
    public class TransferDTOMappingProfile : Profile
    {
        public TransferDTOMappingProfile()
        {
            CreateMap<TransferViewModel, TransferDTO>().ReverseMap();
        }
    }
}
