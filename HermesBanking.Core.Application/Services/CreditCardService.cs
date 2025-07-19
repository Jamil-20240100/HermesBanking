using AutoMapper;
using HermesBanking.Core.Application.DTOs.CreditCard;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;

namespace HermesBanking.Core.Application.Services
{

    
    public class CreditCardService : GenericService<CreditCard, CreditCardDTO>, ICreditCardService
    {
        private readonly ICreditCardRepository _repository;

        public CreditCardService(ICreditCardRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
        }

        public async Task<CreditCardDTO?> GetCardByNumberAsync(string cardNumber)
        {
            var card = await _repository.GetByCardNumberAsync(cardNumber);
            return _mapper.Map<CreditCardDTO>(card);
        }
    }


}
