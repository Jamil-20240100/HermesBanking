using AutoMapper;
using TransactionDTO = HermesBanking.Core.Application.DTOs.Transaction.TransactionDTO;
using TransactionEntity = HermesBanking.Core.Domain.Entities.Transaction;

namespace HermesBanking.Core.Application.Mappings.DTOsAndViewModels
{
    public class TransactionMapperProfile : Profile
    {
        public TransactionMapperProfile()
        {
            // Mapeo de la entidad Transaction a DTO TransactionDTO
            CreateMap<TransactionEntity, TransactionDTO>()
                .ForMember(dest => dest.SavingsAccountId, opt => opt.MapFrom(src => src.SavingsAccountId))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Origin, opt => opt.MapFrom(src => src.Origin))
                .ForMember(dest => dest.Beneficiary, opt => opt.MapFrom(src => src.Beneficiary))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date));

            // Mapeo de DTO TransactionDTO a entidad Transaction
            CreateMap<TransactionDTO, TransactionEntity>()
                .ForMember(dest => dest.SavingsAccountId, opt => opt.MapFrom(src => src.SavingsAccountId))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Origin, opt => opt.MapFrom(src => src.Origin))
                .ForMember(dest => dest.Beneficiary, opt => opt.MapFrom(src => src.Beneficiary))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date));
        }
    }

}

