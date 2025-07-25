using HermesBanking.Core.Application.DTOs.HermesPay;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Common.Enums;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

public class HermesPayService : IHermesPayService
{
    private readonly ICommerceRepository _commerceRepository;
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly ISavingsAccountRepository _savingsAccountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IEmailService _emailService;
    private readonly IAccountServiceForWebApp _accountServiceForWebApp;

    public HermesPayService(
        ICommerceRepository commerceRepository,
        ICreditCardRepository creditCardRepository,
        ISavingsAccountRepository savingsAccountRepository,
        IEmailService emailService,
        IAccountServiceForWebApp accountServiceForWebApp,
        ITransactionRepository transactionRepository)
    {
        _commerceRepository = commerceRepository;
        _creditCardRepository = creditCardRepository;
        _savingsAccountRepository = savingsAccountRepository;
        _transactionRepository = transactionRepository;
        _emailService = emailService;
        _accountServiceForWebApp = accountServiceForWebApp;
    }

    public async Task<List<Transaction>> GetTransactionsByCommerceAsync(int commerceId, int page = 1, int pageSize = 20)
    {
        var skip = (page - 1) * pageSize;

        var filtered = await _transactionRepository.GetByConditionAsync(t => t.CommerceId == commerceId);
        return filtered
            .OrderByDescending(t => t.TransactionDate)
            .Skip(skip)
            .Take(pageSize)
            .ToList();
    }


    public async Task<HermesPayResponse> ProccesHermesPay(HermesPayRequest request, ClaimsPrincipal user)
    {
        if (string.IsNullOrWhiteSpace(request.CardNumber) ||
            string.IsNullOrWhiteSpace(request.MonthExpirationCard) ||
            string.IsNullOrWhiteSpace(request.YearExpirationCard) ||
            string.IsNullOrWhiteSpace(request.CVC) ||
            request.Ammount <= 0)
        {
            return new HermesPayResponse { Exitoso = false, Mensaje = "Todos los campos son obligatorios." };
        }

        // Determinar comercio según el rol
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        var userIdFromToken = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (role == "Comercio")
        {
            var comercioUsuario = await _commerceRepository.GetByUserIdAsync(userIdFromToken);
            if (comercioUsuario == null)
                return new HermesPayResponse { Exitoso = false, Mensaje = "No se encontró el comercio del usuario autenticado." };

            request.CommerceId = comercioUsuario.Id;
        }

        if (request.CommerceId == null)
            return new HermesPayResponse { Exitoso = false, Mensaje = "Debe proporcionar un commerceId válido." };

        var comercio = await _commerceRepository.GetCommerceByIdAsync(request.CommerceId.Value);
        if (comercio == null)
            return new HermesPayResponse { Exitoso = false, Mensaje = "Comercio no encontrado." };

        // Obtener tarjeta
        var tarjeta = await _creditCardRepository.GetByCardNumberAsync(request.CardNumber);
        if (tarjeta == null || !tarjeta.IsActive)
            return new HermesPayResponse { Exitoso = false, Mensaje = "Tarjeta no válida o inactiva." };

        // Validar CVC y fecha de expiración
        if (!int.TryParse(request.MonthExpirationCard, out int mes) || !int.TryParse(request.YearExpirationCard, out int anio))
            return new HermesPayResponse { Exitoso = false, Mensaje = "Fecha de expiración inválida." };

        int year = int.Parse(request.YearExpirationCard);
        if (year < 100) year += 2000; // Ej: 28 => 2028

        int month = int.Parse(request.MonthExpirationCard);

        var fechaExpInput = new DateTime(year, month, 1);
        if (tarjeta.CVC != request.CVC ||
    tarjeta.ExpirationDate.Year != year ||
    tarjeta.ExpirationDate.Month != month)
        {
            return new HermesPayResponse { Exitoso = false, Mensaje = "Datos de la tarjeta incorrectos." };
        }

        if (tarjeta.ExpirationDate < DateTime.UtcNow.Date)
            return new HermesPayResponse { Exitoso = false, Mensaje = "La tarjeta está vencida." };

        // Validar crédito
        var disponible = tarjeta.CreditLimit - tarjeta.TotalOwedAmount;
        if (request.Ammount > disponible)
            return new HermesPayResponse { Exitoso = false, Mensaje = "Crédito insuficiente." };

        // Buscar cuenta de ahorro por ClientId (que equivale al UserId del comercio)
        var cuentas = await _savingsAccountRepository.GetAccountsByClientIdAsync(comercio.UserId);
        var cuentaPrincipal = cuentas.FirstOrDefault(c => c.AccountType == AccountType.Primary);
        if (cuentaPrincipal == null)
            return new HermesPayResponse { Exitoso = false, Mensaje = "El comercio no tiene cuenta principal asociada." };

        // Actualizar montos
        tarjeta.TotalOwedAmount += (decimal)request.Ammount;
        cuentaPrincipal.Balance += (decimal)request.Ammount;

        await _creditCardRepository.UpdateAsync(tarjeta);
        await _savingsAccountRepository.UpdateAsync(cuentaPrincipal);

        // Registrar transacción
        var transaccion = new Transaction
        {
            Amount = (decimal)request.Ammount,
            CreditCardId = tarjeta.CardId,
            DestinationAccountId = cuentaPrincipal.AccountNumber,
            TransactionType = TransactionType.PagoTarjetaCredito,
            Description = $"Pago realizado al comercio {comercio.Name}",
            TransactionDate = DateTime.Now,
            CommerceId = comercio.Id,
            Status = Status.APPROVED
        };
        await _transactionRepository.AddAsync(transaccion);

        // Obtener email del cliente desde ClientId
        var clientEmail = await _accountServiceForWebApp.GetUserEmailAsync(tarjeta.ClientId);
        if (!string.IsNullOrEmpty(clientEmail))
        {
            await _emailService.SendCreditCardPaymentNotificationAsync(
                clientEmail,
                request.Ammount ?? 0m,
                request.CardNumber[^4..],
                cuentaPrincipal.AccountNumber[^4..],
                transaccion.TransactionDate ?? DateTime.UtcNow
            );
        }

        var savingsAccountOwnerEmail = await _accountServiceForWebApp.GetUserEmailAsync(cuentaPrincipal.ClientId);
        if (!string.IsNullOrEmpty(savingsAccountOwnerEmail))
        {
            await _emailService.SendCreditCardPaymentNotificationAsync(
                savingsAccountOwnerEmail,
                request.Ammount ?? 0m,
                request.CardNumber[^4..],
                cuentaPrincipal.AccountNumber[^4..],
                transaccion.TransactionDate ?? DateTime.UtcNow
            );
        }

        return new HermesPayResponse
        {
            Exitoso = true,
            Mensaje = "Transacción aprobada correctamente."
        };
    }


}
