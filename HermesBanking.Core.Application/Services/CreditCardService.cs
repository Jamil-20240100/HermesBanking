using AutoMapper;
using HermesBanking.Core.Application.DTOs.CreditCard;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace HermesBanking.Core.Application.Services
{
    public class CreditCardService : GenericService<CreditCard, CreditCardDTO>, ICreditCardService
    {
        private readonly ICreditCardRepository _repository;
        private readonly IMapper _mapper;
        private readonly IAccountServiceForWebApp _accountServiceForWebApp;
        public CreditCardService(ICreditCardRepository repository, IMapper mapper, IAccountServiceForWebApp accountServiceForWebApp) : base(repository, mapper)
        {
            _repository = repository;
            _accountServiceForWebApp = accountServiceForWebApp;
            _mapper = mapper;
        }

        public async Task<CreditCardPagedResponseDTO> GetCreditCardsAsync(string? cedula = null, string? estado = null, int pagina = 1, int pageSize = 10)
        {
            var cardsQuery = _repository.GetAllQuery().AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
            {
                bool state = false;
                if(estado == "activa")
                {
                    state = true;
                }
                else if (estado == "cancelada")
                {
                    state = false;
                }

                if (state == true || state == false)
                {
                    cardsQuery = cardsQuery.Where(c => c.IsActive == state);
                }
            }

            var allCards = cardsQuery.ToList();

            if (!string.IsNullOrWhiteSpace(cedula))
            {
                var matchingClientIds = new List<string>();

                foreach (var card in allCards)
                {
                    var user = await _accountServiceForWebApp.GetUserById(card.ClientId);
                    if (user != null && user.UserId == cedula)
                    {
                        matchingClientIds.Add(card.ClientId);
                    }
                }

                allCards = allCards.Where(c => matchingClientIds.Contains(c.ClientId)).ToList();
            }

            allCards = allCards.OrderByDescending(c => c.ExpirationDate).ToList();

            int totalRegistros = allCards.Count;
            int totalPaginas = (int)Math.Ceiling(totalRegistros / (double)pageSize);
            pagina = Math.Clamp(pagina, 1, totalPaginas == 0 ? 1 : totalPaginas);

            var cardsPage = allCards
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var data = new List<CreditCardDTO>();
            foreach (var card in cardsPage)
            {
                var user = await _accountServiceForWebApp.GetUserById(card.ClientId);
                var dto = _mapper.Map<CreditCardDTO>(card);

                if (user != null)
                {
                    dto.ClientFullName = $"{user.Name} {user.LastName}";
                }

                data.Add(dto);
            }

            return new CreditCardPagedResponseDTO
            {
                Data = data,
                Paginacion = new PaginationDTO
                {
                    PaginaActual = pagina,
                    TotalPaginas = totalPaginas,
                    TotalRegistros = totalRegistros
                }
            };
        }

        public async Task CreateCreditCardAsync(CreateCreditCardForApiDTO dto)
        {
            var cliente = await _accountServiceForWebApp.GetUserById(dto.ClienteId);
            if (cliente == null)
                throw new ArgumentException("Cliente no encontrado.");

            string numero;
            do
            {
                numero = GenerateUniqueCardId();
            } while (await _repository.ExistsAsync(c => c.CardId == numero));

            var client = await _accountServiceForWebApp.GetUserById(dto.ClienteId);

            var card = new CreditCardDTO
            {
                Id = 0,
                CardId = numero,
                ClientId = dto.ClienteId,
                CreditLimit = dto.Limite,
                TotalOwedAmount = 0,
                ExpirationDate = DateTime.Today.AddYears(3),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CVC = GenerateAndEncryptCVC(),
                ClientFullName = $"{client?.Name} {client?.LastName}",
                CreatedByAdminId = "",
                AdminFullName = ""
            };

            var mapped = _mapper.Map<CreditCard>(card);

            await _repository.AddAsync(mapped);
        }

        public string GenerateUniqueCardId()
        {
            Random random = new Random();
            string cardId;
            do
            {
                cardId = "";
                for (int i = 0; i < 16; i++)
                {
                    cardId += random.Next(0, 10).ToString();
                }
            } while (false);
            return cardId;
        }

        public string GenerateAndEncryptCVC()
        {
            Random random = new Random();

            string cvc = random.Next(100, 1000).ToString();

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(cvc));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}