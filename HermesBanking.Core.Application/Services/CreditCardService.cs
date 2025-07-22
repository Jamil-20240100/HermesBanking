using AutoMapper;
using HermesBanking.Core.Application.DTOs.CreditCard;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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

            var allClientDTOs = await _accountServiceForWebApp.GetAllUser(); 
                                                                             
            var clientsDict = allClientDTOs.ToDictionary(u => u.Id);

            if (!string.IsNullOrWhiteSpace(estado))
            {
                bool? stateFilter = null;
                if (estado.Equals("activa", StringComparison.OrdinalIgnoreCase))
                {
                    stateFilter = true;
                }
                else if (estado.Equals("cancelada", StringComparison.OrdinalIgnoreCase))
                {
                    stateFilter = false;
                }

                if (stateFilter.HasValue)
                {
                    cardsQuery = cardsQuery.Where(c => c.IsActive == stateFilter.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(cedula))
            {
                var matchingClientIds = allClientDTOs
                    .Where(u => u.UserId == cedula) 
                    .Select(u => u.Id)
                    .ToList();

                cardsQuery = cardsQuery.Where(c => matchingClientIds.Contains(c.ClientId));
            }

            var allCards = await cardsQuery.ToListAsync(); 
            allCards = allCards.OrderByDescending(c => c.ExpirationDate).ToList();

            int totalRegistros = allCards.Count();
            int totalPaginas = (int)Math.Ceiling(totalRegistros / (double)pageSize);
            pagina = Math.Clamp(pagina, 1, totalPaginas == 0 ? 1 : totalPaginas);

            var cardsPage = allCards
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var data = new List<CreditCardDTO>();
            foreach (var card in cardsPage)
            {
                clientsDict.TryGetValue(card.ClientId, out var user);

                var dto = _mapper.Map<CreditCardDTO>(card);

                if (user != null)
                {
                    dto.ClientFullName = $"{user.Name} {user.LastName}";
                    dto.ClientIdentification = user.UserId;
                }
                 if (card.CreatedByAdminId != null && clientsDict.TryGetValue(card.CreatedByAdminId, out var adminUser))
                {
                    dto.AdminFullName = $"{adminUser.Name} {adminUser.LastName}";
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

            var card = new CreditCard
            {
                CardId = numero,
                ClientId = dto.ClienteId,
                CreditLimit = dto.Limite,
                TotalOwedAmount = 0,
                ExpirationDate = DateTime.Today.AddYears(3),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CVC = GenerateAndEncryptCVC(),
                CreatedByAdminId = "",
                ClientFullName = $"{cliente.Name} {cliente.LastName}"
            };

            await _repository.AddAsync(card);
        }

        public string GenerateUniqueCardId()
        {
            Random random = new();
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
            Random random = new();
            string cvc = random.Next(100, 1000).ToString();
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(cvc));

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }

        public async Task CancelCreditCardAsync(int id)
        {
            var card = await _repository.GetById(id);
            if (card == null) throw new Exception("La tarjeta de crédito no existe.");

            card.IsActive = false;

            await _repository.UpdateAsync(card.Id, card);
        }

        public async Task<CreditCardDTO?> GetCreditCardByIdAsync(int id)
        {
            var card = await _repository.GetById(id);
            if (card == null) return null;

            var dto = _mapper.Map<CreditCardDTO>(card);

            var user = await _accountServiceForWebApp.GetUserById(card.ClientId);
            if (user != null)
            {
                dto.ClientFullName = $"{user.Name} {user.LastName}";
                dto.ClientIdentification = user.UserId; 
            }

            if (!string.IsNullOrEmpty(card.CreatedByAdminId))
            {
                var admin = await _accountServiceForWebApp.GetUserById(card.CreatedByAdminId);
                if (admin != null)
                {
                    dto.AdminFullName = $"{admin.Name} {admin.LastName}";
                }
            }
            return dto;
        }
    }
}