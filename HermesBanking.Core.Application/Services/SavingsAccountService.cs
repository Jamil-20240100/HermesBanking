using AutoMapper;
using HermesBanking.Core.Application.DTOs.SavingsAccount;
using HermesBanking.Core.Application.DTOs.Transaction;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Common.Enums;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HermesBanking.Core.Application.Services
{
    public class SavingsAccountService : GenericService<SavingsAccount, SavingsAccountDTO>, ISavingsAccountService
    {
        private readonly ISavingsAccountRepository _repository;
        private readonly ILoanRepository _loanRepository;
        private readonly ICreditCardRepository _ctRepository;
        private readonly IAccountServiceForWebApp _accountServiceForWebApp;
        private readonly IMapper _mapper;
        private readonly ILogger<TransactionService> _logger;
        private readonly ITransactionRepository _transactionRepository;

        public SavingsAccountService(
            ISavingsAccountRepository repository,
            ITransactionRepository transactionRepository,
            ILoanRepository loanRepository,
            ICreditCardRepository ctRepository,
            IMapper mapper,
            IAccountServiceForWebApp accountServiceForWebApp,
            ILogger<TransactionService> logger
        ) : base(repository, mapper)
        {
            _mapper = mapper;
            _repository = repository;
            _loanRepository = loanRepository;
            _ctRepository = ctRepository;
            _accountServiceForWebApp = accountServiceForWebApp;
            _logger = logger;
            _transactionRepository = transactionRepository;
        }

        public async Task<List<SavingsAccountDTO>> GetAllSavingsAccountsOfClients()
        {
            //get users
            var clientDTOsList = await _accountServiceForWebApp.GetAllUserByRole(Roles.Client.ToString());
            var adminDTOsList = await _accountServiceForWebApp.GetAllUserByRole(Roles.Admin.ToString());

            //get users' ids
            var clientIds = clientDTOsList.Select(x => x.Id).ToList();
            var adminIds = adminDTOsList.Select(x => x.Id).ToList();

            //get all savings accounts
            var savingsAccountsList = await _repository.GetAll();

            //get clients' accounts
            var clientsAccounts = savingsAccountsList
                .Where(sa => clientIds.Contains(sa.ClientId) || adminIds.Contains(sa.CreatedByAdminId ?? ""))
                .ToList();

            //map entities to dtos
            var returnDTOsList = _mapper.Map<List<SavingsAccountDTO>>(clientsAccounts);

            //add full names
            foreach (var dto in returnDTOsList)
            {
                var user = clientDTOsList.FirstOrDefault(u => u.Id == dto.ClientId);
                var admin = adminDTOsList.FirstOrDefault(u => u.Id == dto.CreatedByAdminId);
                if (user != null)
                {
                    dto.ClientFullName = $"{user.Name} {user.LastName}";
                    dto.ClientUserId = $"{user.UserId}";
                    dto.AdminFullName = $"{admin?.Name} {admin?.LastName}";
                }
            }
            return returnDTOsList;
        }

        public async Task<string> GenerateUniqueAccountNumberAsync()
        {
            const int maxAttempts = 9999;
            var random = new Random();

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                string candidate = random.Next(100_000_000, 1_000_000_000).ToString();

                var existingAccount = await _repository.GetByAccountNumberAsync(candidate);
                if (existingAccount == null)
                {
                    return candidate;
                }
            }

            throw new Exception("No se pudo generar un número de cuenta único luego de varios intentos.");
        }

        public async Task CancelAsync(int id)
        {
            var account = await _repository.GetById(id);
            if (account == null) throw new Exception("La cuenta no existe.");

            account.IsActive = false;

            await _repository.UpdateAsync(account.Id, account);
        }

        public async Task TransferBalanceAndCancelAsync(int accountId)
        {
            var account = await _repository.GetById(accountId);
            if (account == null || account.AccountType != AccountType.Secondary)
                throw new Exception("Cuenta no válida para transferencia.");

            if (!account.IsActive)
                throw new Exception("La cuenta ya está inactiva.");

            if (account.Balance > 0)
            {
                var allAccounts = await _repository.GetAll();

                var primaryAccount = allAccounts
                    .FirstOrDefault(a =>
                        a.ClientId == account.ClientId &&
                        a.AccountType == AccountType.Primary &&
                        a.IsActive);

                if (primaryAccount == null)
                    throw new Exception("Cuenta principal activa no encontrada.");

                primaryAccount.Balance += account.Balance;
                account.Balance = 0;

                await _repository.UpdateAsync(primaryAccount.Id, primaryAccount);
            }

            account.IsActive = false;
            await _repository.UpdateAsync(account.Id, account);
        }

        public async Task<SavingsAccountDTO?> GetByAccountNumberAsync(string beneficiaryAccountNumber)
        {
            var clientDTOsList = await _accountServiceForWebApp.GetAllUserByRole(Roles.Client.ToString());
            var adminDTOsList = await _accountServiceForWebApp.GetAllUserByRole(Roles.Admin.ToString());

            var clientIds = clientDTOsList.Select(x => x.Id).ToList();
            var adminIds = adminDTOsList.Select(x => x.Id).ToList();

            var savingsAccountsList = await _repository.GetAll();

            var clientAccounts = savingsAccountsList
                .Where(sa =>
                    (clientIds.Contains(sa.ClientId) || adminIds.Contains(sa.CreatedByAdminId ?? "")) &&
                    sa.AccountNumber == beneficiaryAccountNumber &&
                    sa.IsActive)
                .ToList();

            var targetAccount = clientAccounts.FirstOrDefault();
            if (targetAccount == null)
                return null;

            var dto = _mapper.Map<SavingsAccountDTO>(targetAccount);

            var user = clientDTOsList.FirstOrDefault(u => u.Id == dto.ClientId);
            var admin = adminDTOsList.FirstOrDefault(u => u.Id == dto.CreatedByAdminId);

            if (user != null)
            {
                dto.ClientFullName = $"{user.Name} {user.LastName}";
                dto.ClientUserId = user.UserId;
                dto.AdminFullName = admin != null ? $"{admin.Name} {admin.LastName}" : null;
            }

            return dto;
        }

        public async Task<List<DisplayTransactionDTO>> GetSavingAccountTransactionsAsync(string savingAccountId)
        {
            _logger.LogInformation($"Attempting to retrieve transactions for saving account: {savingAccountId}.");

            // Obtener todas las transacciones
            var allTransactions = await _transactionRepository.GetAllQuery()
                                                              .OrderByDescending(t => t.TransactionDate)
                                                              .ToListAsync();

            var savingAccountTransactions = new List<DisplayTransactionDTO>();

            foreach (var t in allTransactions)
            {
                string sourceAccountSavingId = "";
                string destinationAccountSavingId = "";
                string savingsAccountId = "";
                var hasDestinationAccount = !string.IsNullOrEmpty(t.DestinationAccountId);
                var destinationAccount = "";
                var creditCardId = "";
                var hasCt = !string.IsNullOrEmpty(t.DestinationCardId);
                var hasLoan = t.DestinationLoanId.HasValue;
                var loanNumber = "";
                var saNumber = "";
                //string destinationLoanId = "";
                //string destinationCardId = "";


                // Comprobamos si la transacción está asociada a la cuenta de ahorros (SourceAccountId o DestinationAccountId)
                if (!string.IsNullOrEmpty(t.SourceAccountId))
                {
                    var sourceAccount = await _repository.GetById(int.Parse(t.SourceAccountId));
                    if (sourceAccount != null) sourceAccountSavingId = sourceAccount.AccountNumber;
                }
                if (!string.IsNullOrEmpty(t.SavingsAccountId))
                {
                    var sourceAccount = await _repository.GetById(int.Parse(t.SavingsAccountId));
                    if (sourceAccount != null) savingsAccountId = sourceAccount.AccountNumber;
                }
                if (hasDestinationAccount)
                {
                    var destAccount = await _repository.GetById(int.Parse(t.DestinationAccountId));
                    if (destAccount != null) destinationAccountSavingId = destAccount.AccountNumber;
                }
                if (hasCt)
                {
                    var ctNumber = await _ctRepository.GetById(int.Parse(t.DestinationCardId));
                    if (ctNumber != null) creditCardId = ctNumber.CardId; 
                }
                if (hasLoan)
                {
                    var lNumber = await _loanRepository.GetById(t.DestinationLoanId.Value);
                    if (lNumber != null)
                    {
                        loanNumber = lNumber.LoanIdentifier;
                    }
                    else
                    {
                        // Puedes registrar que no se encontró el préstamo con ese ID
                        loanNumber = "Préstamo no encontrado";
                    }
                }
                
                if (!string.IsNullOrEmpty(t.SavingsAccountId))
                {
                    var sNumber = await _repository.GetById(int.Parse(t.SavingsAccountId));
                    if (sNumber != null) saNumber = sNumber.AccountNumber;
                }

                // Si la transacción está relacionada con la cuenta de ahorros proporcionada, la agregamos a la lista
                if (sourceAccountSavingId == savingAccountId.ToString() || destinationAccountSavingId.ToString() == savingAccountId.ToString() || savingsAccountId.ToString() == savingAccountId.ToString())
                {
                    string transactionTypeString = t.TransactionType?.ToString() ?? "Desconocido";
                    string saTransactionTypeString = t.TransactionType?.ToString() ?? "Desconocido";
                    string originIdentifier = "AVANCE";
                    string destinationIdentifier = "N/A";
                    string description = t.Description;

                    // Si la transacción tiene cuenta origen, obtenemos su número (últimos 4 dígitos)
                    if (!string.IsNullOrEmpty(t.SourceAccountId))
                    {
                        var account = await _repository.GetById(int.Parse(t.SourceAccountId));
                        if (account != null)
                        {
                            originIdentifier = $"Cuenta: {account.AccountNumber.Substring(Math.Max(0, account.AccountNumber.Length - 4))}";
                        }
                    }
                   
                    // Filtramos por tipo de transacción
                    switch (t.TransactionType)
                    {
                        case TransactionType.Transferencia:
                            if (!string.IsNullOrEmpty(t.DestinationAccountId))
                            {
                                var account = await _repository.GetById(int.Parse(t.DestinationAccountId));
                                if (account != null)
                                {
                                    destinationIdentifier = $"Cuenta: {account.AccountNumber.Substring(Math.Max(0, account.AccountNumber.Length - 4))}";
                                }
                            }
                            description = description ?? "Transferencia";
                            saTransactionTypeString = TransactionType.DEBITO.ToString();
                            break;

                        case TransactionType.Deposito:
                            transactionTypeString = "Depósito";
                            saTransactionTypeString = TransactionType.CREDITO.ToString();
                            destinationIdentifier = originIdentifier;
                            originIdentifier = "Deposito";
                            description = description ?? "Depósito en cuenta";
                            break;

                        case TransactionType.Retiro:
                            transactionTypeString = "Retiro";
                            saTransactionTypeString = TransactionType.DEBITO.ToString();
                            destinationIdentifier = "Efectivo/Externo";
                            description = description ?? "Retiro de efectivo";
                            break;

                        default:
                            transactionTypeString = "Otro";
                            saTransactionTypeString = TransactionType.DEBITO.ToString(); 
                            description = description ?? "Transacción no clasificada";
                            break;
                    }

                    var beneficiario = hasDestinationAccount ? destinationAccountSavingId : hasCt ? creditCardId?.Substring(creditCardId.Length - 4) : hasLoan ? loanNumber : t.TransactionType == TransactionType.Retiro ? "RETIRO" : saNumber;
                    // Añadimos la transacción a la lista de resultados
                    savingAccountTransactions.Add(new DisplayTransactionDTO
                    {
                        TransactionId = t.Id.ToString(),
                        Type = transactionTypeString,
                        saTransactionType = saTransactionTypeString,
                        Amount = t.Amount,
                        Date = t.TransactionDate ?? DateTime.Now,
                        OriginIdentifier = originIdentifier,
                        DestinationIdentifier = destinationIdentifier,
                        Beneficiary = beneficiario,
                        Status = t.Status == Status.APPROVED ? "APROBADO" : t.Status == Status.REJECTED ? "RECHAZADO" : "PENDING",
                        Description = description
                    });
                }
            }

            _logger.LogInformation($"Retrieved {savingAccountTransactions.Count} transactions for saving account: {savingAccountId}.");
            return savingAccountTransactions.OrderByDescending(t => t.Date).ToList();
        }

    }
}
