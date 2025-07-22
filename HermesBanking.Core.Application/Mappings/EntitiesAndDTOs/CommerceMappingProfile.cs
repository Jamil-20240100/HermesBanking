using AutoMapper;
using HermesBanking.Core.Application.DTOs.Commerce;
using HermesBanking.Core.Domain.Entities;

namespace HermesBanking.Core.Application.Mappings.EntitiesAndDTOs
{
    public class CommerceMappingProfile : Profile
    {
        public CommerceMappingProfile()
        {
            // Mapeo de la entidad Commerce a CommerceDTO
            CreateMap<Commerce, CommerceDTO>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))  // Ejemplo de configuración personalizada
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))  // Mapear UserId, si es necesario
                .ReverseMap();  // Para mapear también desde CommerceDTO a Commerce
        }
    }
}
