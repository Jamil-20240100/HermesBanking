using AutoMapper;
using HermesBanking.Core.Application.DTOs.CreditCard;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;

namespace HermesBanking.Core.Application.Services
{
    public class CreditCardService : GenericService<CreditCard, CreditCardDTO>, ICreditCardService
    {
        public CreditCardService(IGenericRepository<CreditCard> repository, IMapper mapper) : base(repository, mapper)
        {
        }
    }
}
