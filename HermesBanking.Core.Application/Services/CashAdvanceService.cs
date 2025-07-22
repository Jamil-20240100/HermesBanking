using AutoMapper;
using HermesBanking.Core.Application.DTOs.CashAdvance;
using HermesBanking.Core.Application.DTOs.Email;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.ViewModels.CashAdvance;
using HermesBanking.Core.Domain.Common.Enums;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;

namespace HermesBanking.Core.Application.Services
{
    public class CashAdvanceService : ICashAdvanceService
    {
        private readonly ICreditCardRepository _creditCardRepo;
        private readonly ISavingsAccountRepository _savingsAccountRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly IAccountServiceForWebApp _accountServiceForWebApp;

        public CashAdvanceService(
            ICreditCardRepository creditCardRepo,
            ISavingsAccountRepository savingsAccountRepo,
            ITransactionRepository transactionRepo,
            IEmailService emailService,
            IMapper mapper,
            IAccountServiceForWebApp accountServiceForWebApp)
        {
            _creditCardRepo = creditCardRepo;
            _savingsAccountRepo = savingsAccountRepo;
            _transactionRepo = transactionRepo;
            _emailService = emailService;
            _mapper = mapper;
            _accountServiceForWebApp = accountServiceForWebApp;
        }

        public async Task<CashAdvanceViewModel> ProcessCashAdvanceAsync(CashAdvanceDTO dto)
        {
            var response = new CashAdvanceViewModel { HasError = false };

            var sourceCard = await _creditCardRepo.GetById(dto.SourceCreditCardId);
            var destinationAccount = await _savingsAccountRepo.GetById(dto.DestinationSavingsAccountId);

            if (sourceCard == null || destinationAccount == null)
            {
                response.HasError = true;
                response.ErrorMessage = "La tarjeta o cuenta seleccionada no es válida.";
                return response;
            }

            decimal availableCredit = sourceCard.CreditLimit - sourceCard.TotalOwedAmount;
            if (dto.Amount > availableCredit)
            {
                response.HasError = true;
                response.ErrorMessage = $"El monto solicitado (RD${dto.Amount:N2}) excede su crédito disponible (RD${availableCredit:N2}).";
                return response;
            }

            decimal interestRate = 0.0625m;
            decimal interestAmount = dto.Amount * interestRate;
            decimal totalDebtIncrease = dto.Amount + interestAmount;

            destinationAccount.Balance += dto.Amount;
            sourceCard.TotalOwedAmount += totalDebtIncrease;

            var savingsTransaction = new Transaction
            {
                SavingsAccountId = destinationAccount.Id.ToString(),
                Amount = dto.Amount,
                Type = TransactionType.CREDITO.ToString(),
                Origin = $"Avance TDC ...{sourceCard?.CardId?.Substring(sourceCard.CardId.Length - 4)}",
                Beneficiary = destinationAccount.AccountNumber,
                Date = DateTime.Now,
                Status = Status.APPROVED,
                Description = "Recepción de avance de efectivo",
            };
            await _transactionRepo.AddAsync(savingsTransaction);

            if(sourceCard != null)
            {
                var creditCardTransaction = new Transaction
                {
                    CreditCardId = sourceCard.Id,
                    Amount = dto.Amount,
                    Description = "AVANCE DE EFECTIVO",
                    Date = DateTime.Now,
                    Status = Status.APPROVED
                };
            }

            await _savingsAccountRepo.UpdateAsync(destinationAccount.Id, destinationAccount);
            await _creditCardRepo.UpdateAsync(sourceCard.Id, sourceCard);

            var user = await _accountServiceForWebApp.GetUserById(dto.ClientId);
            if (user != null)
            {
                string last4Card = sourceCard.CardId.Substring(sourceCard.CardId.Length - 4);
                string last4Account = destinationAccount.AccountNumber.Substring(destinationAccount.AccountNumber.Length - 4);

                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = $"Avance de efectivo desde la tarjeta [{last4Card}]",
                    HtmlBody = $@"
                        <h1>Notificación de Avance de Efectivo</h1>
                        <p>Se ha realizado un avance de efectivo desde su tarjeta de crédito.</p>
                        <p><strong>Monto del Avance:</strong> RD${dto.Amount:N2}</p>
                        <p><strong>Depositado en Cuenta:</strong> ...{last4Account}</p>
                        <p><strong>Tarjeta de Origen:</strong> ...{last4Card}</p>
                        <p><strong>Fecha y Hora:</strong> {DateTime.Now:dd/MM/yyyy hh:mm:ss tt}</p>"
                });
            }
            return response;
        }
    }
}