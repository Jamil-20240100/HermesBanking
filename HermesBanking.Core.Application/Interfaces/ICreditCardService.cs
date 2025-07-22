using HermesBanking.Core.Application.DTOs.CreditCard;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface ICreditCardService : IGenericService<CreditCardDTO>
    {
        Task<CreditCardPagedResponseDTO> GetCreditCardsAsync(string? cedula = null, string? estado = null, int pagina = 1, int pageSize = 10);
        Task CreateCreditCardAsync(CreateCreditCardForApiDTO dto);
        public string GenerateUniqueCardId();
        public string GenerateAndEncryptCVC();

        Task<CreditCardDTO> GetCreditCardByIdAsync(int id);

    }
}
