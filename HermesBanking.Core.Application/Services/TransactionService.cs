using HermesBanking.Core.Application.DTOs;
using HermesBanking.Core.Application.DTOs.Transaction;
using HermesBanking.Core.Application.DTOs.Transfer;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using HermesBanking.Core.Domain.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace HermesBanking.Core.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ISavingsAccountRepository _savingsAccountRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountServiceForWebApp _accountServiceForWebApp;
        private readonly ICommerceService _commerceService;
        private readonly ILoanAmortizationService _loanAmortizationService;
        private readonly IEmailService _emailService;
        private readonly IBeneficiaryService _beneficiaryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            ISavingsAccountRepository savingsAccountRepository,
            ICreditCardRepository creditCardRepository,
            ITransactionRepository transactionRepository,
            IAccountServiceForWebApp accountServiceForWebApp,
            ICommerceService commerceService,
            ILoanAmortizationService loanAmortizationService,
            IEmailService emailService,
            IBeneficiaryService beneficiaryService,
            IUnitOfWork unitOfWork,
            ILogger<TransactionService> logger)
        {
            _savingsAccountRepository = savingsAccountRepository;
            _creditCardRepository = creditCardRepository;
            _transactionRepository = transactionRepository;
            _accountServiceForWebApp = accountServiceForWebApp;
            _loanAmortizationService = loanAmortizationService;
            _emailService = emailService;
            _beneficiaryService = beneficiaryService;
            _unitOfWork = unitOfWork;
            _commerceService = commerceService;
            _logger = logger;
        }

        public async Task<List<TransactionDTO>> GetAllTransactionsAsync()
        {
            _logger.LogInformation("Attempting to retrieve all transactions.");
            var transactions = await _transactionRepository.GetAllAsync();

            //var transactionDtos = transactions.Select(async t => new TransactionDTO
            //{
            //    Id = t.Id,
            //    Amount = t.Amount,
            //    TransactionDate = t.TransactionDate ?? DateTime.MinValue,
            //    TransactionType = t.TransactionType,
            //    Commerce = await _commerceService.GetCommerceByIdAsync(t.CommerceId ?? 0),
            //    Description = t.Description,
            //    SourceAccountId = t.SourceAccountId,
            //    DestinationAccountId = t.DestinationAccountId,
            //    DestinationCardId = t.DestinationCardId,
            //    DestinationLoanId = t.DestinationLoanId,
            //    CreditCardId = t.CreditCardId
            //});

            var transactionDtos = new List<TransactionDTO>();
            foreach (var t in transactions)
            {
                var dto = new TransactionDTO
                {
                    Id = t.Id,
                    Status = t.Status,
                    Amount = t.Amount,
                    TransactionDate = t.TransactionDate ?? DateTime.MinValue,
                    TransactionType = t.TransactionType,
                    Commerce = await _commerceService.GetCommerceByIdAsync(t.CommerceId ?? 0),
                    Description = t.Description,
                    SourceAccountId = t.SourceAccountId,
                    DestinationAccountId = t.DestinationAccountId,
                    DestinationCardId = t.DestinationCardId,
                    DestinationLoanId = t.DestinationLoanId,
                    CreditCardId = t.CreditCardId
                };
                transactionDtos.Add(dto);
            }

            var transactionOrderedDto = transactionDtos.OrderByDescending(t => t.TransactionDate).ToList();

            _logger.LogInformation($"Retrieved {transactionOrderedDto.Count} transactions.");
            return transactionOrderedDto;
        }

        public async Task PerformTransactionAsync(DTOs.Transaction.TransactionRequestDto request)
        {
            _logger.LogInformation($"Iniciando transferencia entre cuentas: Origen={request.SourceAccountNumber}, Destino={request.DestinationAccountNumber}, Monto={request.Amount}");

            using (var transactionScope = _unitOfWork.BeginTransaction())
            {
                try
                {
                    // 1. Validar cuentas (origen y destino)
                    var sourceAccount = await _savingsAccountRepository.GetByAccountNumberAsync(request.SourceAccountNumber);
                    var destinationAccount = await _savingsAccountRepository.GetByAccountNumberAsync(request.DestinationAccountNumber);

                    if (sourceAccount == null) throw new InvalidOperationException("Cuenta de origen no encontrada.");
                    if (destinationAccount == null) throw new InvalidOperationException("Cuenta de destino no encontrada.");

                    if (!sourceAccount.IsActive) throw new InvalidOperationException("La cuenta de origen está inactiva.");
                    if (!destinationAccount.IsActive) throw new InvalidOperationException("La cuenta de destino está inactiva.");

                    var transaction = new Transaction();

                    // 2. Verificar fondos suficientes
                    if (sourceAccount.Balance < request.Amount)
                    {
                        // 3. Registrar transacción rechazada
                        transaction.Amount = request.Amount;
                        transaction.SourceAccountId = sourceAccount.Id.ToString();
                        transaction.DestinationAccountId = destinationAccount.Id.ToString();
                        transaction.TransactionDate = DateTime.Now;
                        transaction.Type = TransactionType.DEBITO.ToString();
                        transaction.TransactionType = TransactionType.Transferencia;
                        transaction.Description = request.Description ?? $"Fondos Insuficientes: Transferencia de {request.SourceAccountNumber} a {request.DestinationAccountNumber}";
                        transaction.Origin = sourceAccount.AccountNumber;
                        transaction.Status = Status.REJECTED;

                        await _transactionRepository.AddAsync(transaction);
                        await _unitOfWork.CommitAsync();

                        _logger.LogInformation($"Transferencia rechazada por fondos insuficientes: {request.SourceAccountNumber} → {request.DestinationAccountNumber}, Monto={request.Amount:C}");
                    }
                    else
                    {
                        // 3. Realizar la transferencia
                        sourceAccount.Balance -= request.Amount;
                        destinationAccount.Balance += request.Amount;

                        await _savingsAccountRepository.UpdateAsync(sourceAccount);
                        await _savingsAccountRepository.UpdateAsync(destinationAccount);

                        // 4. Registrar transacción aprobada
                        transaction.Amount = request.Amount;
                        transaction.SourceAccountId = sourceAccount.Id.ToString();
                        transaction.DestinationAccountId = destinationAccount.Id.ToString();
                        transaction.TransactionDate = DateTime.Now;
                        transaction.Type = TransactionType.DEBITO.ToString();
                        transaction.TransactionType = TransactionType.Transferencia;
                        transaction.Description = request.Description ?? $"Transferencia de {request.SourceAccountNumber} a {request.DestinationAccountNumber}";
                        transaction.Origin = sourceAccount.AccountNumber;
                        transaction.Status = Status.APPROVED;

                        await _transactionRepository.AddAsync(transaction);
                        await _unitOfWork.CommitAsync();

                        _logger.LogInformation($"Transferencia completada exitosamente: {request.SourceAccountNumber} → {request.DestinationAccountNumber}, Monto={request.Amount:C}");

                        // 5. Notificación por correo
                        var sourceClientEmail = await _accountServiceForWebApp.GetUserEmailAsync(sourceAccount.ClientId);
                        var destinationClientEmail = await _accountServiceForWebApp.GetUserEmailAsync(destinationAccount.ClientId);

                        if (!string.IsNullOrEmpty(sourceClientEmail))
                        {
                            await _emailService.SendTransactionInitiatorNotificationAsync(
                                sourceClientEmail,
                                request.Amount,
                                request.DestinationAccountNumber.Substring(request.DestinationAccountNumber.Length - 4),
                                transaction.TransactionDate ?? DateTime.Now
                            );
                        }

                        if (!string.IsNullOrEmpty(destinationClientEmail) && destinationClientEmail != sourceClientEmail)
                        {
                            await _emailService.SendTransactionReceiverNotificationAsync(
                                destinationClientEmail,
                                request.Amount,
                                request.SourceAccountNumber.Substring(request.SourceAccountNumber.Length - 4),
                                transaction.TransactionDate ?? DateTime.Now
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(ex, $"Error al realizar transferencia entre cuentas: {ex.Message}");
                    throw;
                }
            }
        }


        public async Task PayCreditCardAsync(DTOs.Transaction.CreditCardPaymentDto paymentDto)
        {
            _logger.LogInformation($"Iniciando pago de tarjeta de crédito: Tarjeta={paymentDto.CreditCardNumber}, Origen={paymentDto.SourceAccountNumber}, Monto={paymentDto.Amount}");

            using (var transactionScope = _unitOfWork.BeginTransaction())
            {
                try
                {
                    // 1. Validar cuentas/tarjetas
                    var sourceAccount = await _savingsAccountRepository.GetByAccountNumberAsync(paymentDto.SourceAccountNumber);
                    var creditCard = await _creditCardRepository.GetByCardNumberAsync(paymentDto.CreditCardNumber);

                    if (sourceAccount == null) throw new InvalidOperationException("Cuenta de origen no encontrada.");
                    if (creditCard == null) throw new InvalidOperationException("Tarjeta de crédito no encontrada.");

                    if (!sourceAccount.IsActive) throw new InvalidOperationException("La cuenta de origen está inactiva.");
                    if (!creditCard.IsActive) throw new InvalidOperationException("La tarjeta de crédito está inactiva.");

                    var transaction = new Transaction();

                    // 2. Verificar fondos suficientes
                    if (sourceAccount.Balance < paymentDto.Amount)
                    {
                        // 5. Registrar la transacción
                        transaction.Amount = paymentDto.Amount;
                        transaction.SourceAccountId = sourceAccount.Id.ToString();
                        transaction.DestinationCardId = creditCard.Id.ToString();
                        transaction.TransactionDate = DateTime.Now;
                        transaction.Type = TransactionType.DEBITO.ToString();
                        transaction.TransactionType = TransactionType.PagoTarjetaCredito;
                        transaction.Description = paymentDto.Description ?? $" Fondos Insuficientes: Pago a tarjeta de crédito {paymentDto.CreditCardNumber.Substring(paymentDto.CreditCardNumber.Length - 4)}";
                        transaction.Origin = sourceAccount.AccountNumber;
                        transaction.Status = Status.REJECTED;
                        await _transactionRepository.AddAsync(transaction);

                        await _unitOfWork.CommitAsync();
                        _logger.LogInformation($"Pago de tarjeta de crédito {paymentDto.CreditCardNumber} por {paymentDto.Amount:C} no completado, fondos insuficientes.");
                    }
                    else
                    {
                        sourceAccount.Balance -= paymentDto.Amount;
                        creditCard.TotalOwedAmount -= paymentDto.Amount; // Asumiendo TotalOwedAmount es el saldo pendiente

                        await _savingsAccountRepository.UpdateAsync(sourceAccount);
                        await _creditCardRepository.UpdateAsync(creditCard);

                        // 5. Registrar la transacción
                        transaction.Amount = paymentDto.Amount;
                        transaction.SourceAccountId = sourceAccount.Id.ToString();
                        transaction.DestinationCardId = creditCard.Id.ToString();
                        transaction.TransactionDate = DateTime.Now;
                        transaction.TransactionType = TransactionType.PagoTarjetaCredito;
                        transaction.Type = TransactionType.DEBITO.ToString();
                        transaction.Description = paymentDto.Description ??  $"Pago a tarjeta de crédito {paymentDto.CreditCardNumber.Substring(paymentDto.CreditCardNumber.Length - 4)}";
                        transaction.Origin = sourceAccount.AccountNumber;
                        transaction.Status = Status.APPROVED;
                        await _transactionRepository.AddAsync(transaction);

                        await _unitOfWork.CommitAsync();
                        _logger.LogInformation($"Pago de tarjeta de crédito {paymentDto.CreditCardNumber} por {paymentDto.Amount:C} completado exitosamente.");


                        // 6. Enviar notificación por correo
                        var clientEmail = await _accountServiceForWebApp.GetUserEmailAsync(sourceAccount.ClientId);
                        if (!string.IsNullOrEmpty(clientEmail))
                        {
                            await _emailService.SendCreditCardPaymentNotificationAsync(
                                clientEmail,
                                paymentDto.Amount,
                                paymentDto.CreditCardNumber.Substring(paymentDto.CreditCardNumber.Length - 4),
                                paymentDto.SourceAccountNumber.Substring(paymentDto.SourceAccountNumber.Length - 4),
                                transaction.TransactionDate ?? DateTime.Now
                            );
                        }

                    }

                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(ex, $"Error al realizar pago de tarjeta de crédito: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task PayLoanAsync(DTOs.Transaction.LoanPaymentDto paymentDto)
        {
            _logger.LogInformation($"Iniciando pago de préstamo: Préstamo={paymentDto.LoanIdentifier}, Origen={paymentDto.SourceAccountNumber}, Monto={paymentDto.Amount}");

            using (var transactionScope = _unitOfWork.BeginTransaction())
            {
                try
                {
                    // 1. Validar cuenta origen y préstamo
                    var sourceAccount = await _savingsAccountRepository.GetByAccountNumberAsync(paymentDto.SourceAccountNumber);
                    var loan = await _accountServiceForWebApp.GetLoanInfoAsync(paymentDto.LoanIdentifier); // Devuelve Loan o DTO con Id y IsActive

                    if (sourceAccount == null) throw new InvalidOperationException("Cuenta de origen no encontrada.");
                    if (loan == null) throw new InvalidOperationException("Préstamo no encontrado.");

                    if (!sourceAccount.IsActive) throw new InvalidOperationException("La cuenta de origen está inactiva.");
                    if (!loan.IsActive) throw new InvalidOperationException("El préstamo ya no está activo o está completado.");

                    var transaction = new Transaction();

                    // 2. Verificar fondos suficientes
                    if (sourceAccount.Balance < paymentDto.Amount)
                    {
                        // 3. Registrar transacción rechazada
                        transaction.Amount = paymentDto.Amount;
                        transaction.SourceAccountId = sourceAccount.Id.ToString();
                        transaction.DestinationLoanId = loan.Id;
                        transaction.TransactionDate = DateTime.Now;
                        transaction.Type = TransactionType.DEBITO.ToString();
                        transaction.TransactionType = TransactionType.PagoPrestamo;
                        transaction.Description = paymentDto.Description ?? $"Fondos Insuficientes: Pago de préstamo {paymentDto.LoanIdentifier}";
                        transaction.Origin = sourceAccount.AccountNumber;
                        transaction.Status = Status.REJECTED;

                        await _transactionRepository.AddAsync(transaction);
                        await _unitOfWork.CommitAsync();

                        _logger.LogInformation($"Pago de préstamo {paymentDto.LoanIdentifier} por {paymentDto.Amount:C} no completado, fondos insuficientes.");
                    }
                    else
                    {
                        // 4. Realizar deducción del balance
                        sourceAccount.Balance -= paymentDto.Amount;
                        await _savingsAccountRepository.UpdateAsync(sourceAccount);

                        // 5. Aplicar el pago al préstamo
                        decimal amountAppliedToLoan = await _loanAmortizationService.ApplyPaymentToLoanAsync(loan.Id, paymentDto.Amount);

                        // 6. Registrar transacción aprobada
                        transaction.Amount = paymentDto.Amount;
                        transaction.SourceAccountId = sourceAccount.Id.ToString();
                        transaction.DestinationLoanId = loan.Id;
                        transaction.TransactionDate = DateTime.Now;
                        transaction.Type = TransactionType.DEBITO.ToString();
                        transaction.TransactionType = TransactionType.PagoPrestamo;
                        transaction.Description = paymentDto.Description ?? $"Pago de préstamo {paymentDto.LoanIdentifier}";
                        transaction.Origin = sourceAccount.AccountNumber;
                        transaction.Status = Status.APPROVED;

                        await _transactionRepository.AddAsync(transaction);
                        await _unitOfWork.CommitAsync();

                        _logger.LogInformation($"Pago de préstamo {paymentDto.LoanIdentifier} por {paymentDto.Amount:C} completado exitosamente.");

                        // 7. Enviar notificación por correo
                        var clientEmail = await _accountServiceForWebApp.GetUserEmailAsync(sourceAccount.ClientId);
                        if (!string.IsNullOrEmpty(clientEmail))
                        {
                            await _emailService.SendLoanPaymentNotificationAsync(
                                clientEmail,
                                paymentDto.Amount,
                                paymentDto.LoanIdentifier,
                                paymentDto.SourceAccountNumber.Substring(paymentDto.SourceAccountNumber.Length - 4),
                                transaction.TransactionDate ?? DateTime.Now
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(ex, $"Error al realizar pago de préstamo: {ex.Message}");
                    throw;
                }
            }
        }


        public async Task ExecutePayBeneficiaryTransactionAsync(PayBeneficiaryDTO dto)
        {
            _logger.LogInformation($"Iniciando pago a beneficiario: BeneficiaryId={dto.BeneficiaryId}, Origen={dto.SourceAccountNumber}, Monto={dto.Amount}");

            using (var transactionScope = _unitOfWork.BeginTransaction())
            {
                try
                {
                    // 1. Obtener datos del beneficiario
                    var beneficiaryDto = await _beneficiaryService.GetById(dto.BeneficiaryId);
                    if (beneficiaryDto == null)
                        throw new InvalidOperationException("Beneficiario no encontrado.");

                    // 2. Validar cuentas (origen y destino)
                    var sourceAccount = await _savingsAccountRepository.GetByAccountNumberAsync(dto.SourceAccountNumber);
                    var destinationAccount = await _savingsAccountRepository.GetByAccountNumberAsync(beneficiaryDto.BeneficiaryAccountNumber);

                    if (sourceAccount == null) throw new InvalidOperationException("Cuenta de origen no encontrada.");
                    if (destinationAccount == null) throw new InvalidOperationException("Cuenta de destino del beneficiario no encontrada.");

                    if (!sourceAccount.IsActive) throw new InvalidOperationException("La cuenta de origen está inactiva.");
                    if (!destinationAccount.IsActive) throw new InvalidOperationException("La cuenta de destino del beneficiario está inactiva.");

                    var transaction = new Transaction();

                    // 3. Verificar fondos
                    if (sourceAccount.Balance < dto.Amount)
                    {
                        // 4. Registrar transacción RECHAZADA
                        transaction.Amount = dto.Amount;
                        transaction.SourceAccountId = sourceAccount.Id.ToString();
                        transaction.DestinationAccountId = destinationAccount.Id.ToString();
                        transaction.TransactionDate = DateTime.Now;
                        transaction.Type = TransactionType.DEBITO.ToString();
                        transaction.TransactionType = TransactionType.PagoBeneficiario;
                        transaction.Description = dto.Description ?? $"Fondos Insuficientes: Pago a beneficiario {beneficiaryDto.Name} {beneficiaryDto.LastName}";
                        transaction.Origin = sourceAccount.AccountNumber;
                        transaction.Status = Status.REJECTED;

                        await _transactionRepository.AddAsync(transaction);
                        await _unitOfWork.CommitAsync();

                        _logger.LogInformation($"Pago a beneficiario {beneficiaryDto.Name} {beneficiaryDto.LastName} no completado, fondos insuficientes.");
                    }
                    else
                    {
                        // 5. Realizar transferencia
                        sourceAccount.Balance -= dto.Amount;
                        destinationAccount.Balance += dto.Amount;

                        await _savingsAccountRepository.UpdateAsync(sourceAccount);
                        await _savingsAccountRepository.UpdateAsync(destinationAccount);

                        // 6. Registrar transacción ACEPTADA
                        transaction.Amount = dto.Amount;
                        transaction.SourceAccountId = sourceAccount.Id.ToString();
                        transaction.DestinationAccountId = destinationAccount.Id.ToString();
                        transaction.TransactionDate = DateTime.Now;
                        transaction.Type = TransactionType.DEBITO.ToString();
                        transaction.TransactionType = TransactionType.PagoBeneficiario;
                        transaction.Description = dto.Description ?? $"Pago a beneficiario: {beneficiaryDto.Name} {beneficiaryDto.LastName}";
                        transaction.Origin = sourceAccount.AccountNumber;
                        transaction.Status = Status.APPROVED;

                        await _transactionRepository.AddAsync(transaction);
                        await _unitOfWork.CommitAsync();

                        _logger.LogInformation($"Pago a beneficiario {beneficiaryDto.Name} {beneficiaryDto.LastName} completado exitosamente desde la cuenta {dto.SourceAccountNumber} por {dto.Amount:C}.");

                        // 7. Enviar notificaciones por correo
                        var sourceClientEmail = await _accountServiceForWebApp.GetUserEmailAsync(sourceAccount.ClientId);
                        var destinationClientEmail = await _accountServiceForWebApp.GetUserEmailAsync(destinationAccount.ClientId);

                        if (!string.IsNullOrEmpty(sourceClientEmail))
                        {
                            await _emailService.SendTransactionInitiatorNotificationAsync(
                                sourceClientEmail,
                                dto.Amount,
                                beneficiaryDto.BeneficiaryAccountNumber[^4..],
                                transaction.TransactionDate ?? DateTime.Now
                            );
                        }

                        if (!string.IsNullOrEmpty(destinationClientEmail) && sourceClientEmail != destinationClientEmail)
                        {
                            await _emailService.SendTransactionReceiverNotificationAsync(
                                destinationClientEmail,
                                dto.Amount,
                                dto.SourceAccountNumber[^4..],
                                transaction.TransactionDate ?? DateTime.Now
                            );
                        }
                    }
                }
                catch (InvalidOperationException ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogWarning($"Validación fallida para pago a beneficiario: {ex.Message}");
                    throw;
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(ex, $"Error inesperado al ejecutar pago a beneficiario: {ex.Message}");
                    throw;
                }
            }
        }


        public Task ExecuteExpressTransactionAsync(ExpressTransactionDTO dto)
        {
            throw new NotImplementedException();
        }

        public Task ProcessTransferAsync(TransferDTO dto)
        {
            throw new NotImplementedException();
        }

        public Task ExecutePayCreditCardTransactionAsync(PayCreditCardDTO dto)
        {
            throw new NotImplementedException();
        }

        public async Task<List<DisplayTransactionDTO>> GetClientServiceTransactionsAsync(string clientId)
        {
            _logger.LogInformation($"Attempting to retrieve service transactions for client: {clientId}.");

            var allTransactions = await _transactionRepository.GetAllQuery()
                                                              .OrderByDescending(t => t.TransactionDate)
                                                              .ToListAsync(); 

            var clientTransactions = new List<DisplayTransactionDTO>();

            foreach (var t in allTransactions)
            {
                string sourceAccountClientId = null;
                string destinationAccountClientId = null;
                string destinationCardClientId = null;
                string destinationLoanClientId = null;

                if (!string.IsNullOrEmpty(t.SourceAccountId))
                {
                    var sourceAccount = await _savingsAccountRepository.GetById(int.Parse(t.SourceAccountId));
                    if (sourceAccount != null) sourceAccountClientId = sourceAccount.ClientId;
                }
                if (!string.IsNullOrEmpty(t.DestinationAccountId))
                {
                    var destAccount = await _savingsAccountRepository.GetById(int.Parse(t.DestinationAccountId));
                    if (destAccount != null) destinationAccountClientId = destAccount.ClientId;
                }
                if (!string.IsNullOrEmpty(t.DestinationCardId))
                {
                    var destCard = await _creditCardRepository.GetById(int.Parse(t.DestinationCardId));
                    if (destCard != null) destinationCardClientId = destCard.ClientId;
                }
                if (t.DestinationLoanId.HasValue)
                {
                    var destLoan = await _accountServiceForWebApp.GetLoanInfoAsync(t.DestinationLoanId.Value.ToString());
                    if (destLoan != null) destinationLoanClientId = destLoan.ClientId;
                }


                if (sourceAccountClientId == clientId ||
                    destinationAccountClientId == clientId ||
                    destinationCardClientId == clientId ||
                    destinationLoanClientId == clientId)
                {
                    string transactionTypeString = t.TransactionType?.ToString() ?? "Desconocido";
                    string saTransactionType = t.TransactionType?.ToString() ?? "Desconocido";

                    string originIdentifier = "N/A";
                    string destinationIdentifier = "N/A";
                    string description = t.Description;

                    if (!string.IsNullOrEmpty(t.SourceAccountId))
                    {
                        var account = await _savingsAccountRepository.GetById(int.Parse(t.SourceAccountId));
                        if (account != null)
                        {
                            originIdentifier = $"Cuenta: {account.AccountNumber.Substring(Math.Max(0, account.AccountNumber.Length - 4))}";
                        }
                    }

                    switch (t.TransactionType)
                    {
                        case TransactionType.Transferencia:
                            if (!string.IsNullOrEmpty(t.DestinationAccountId))
                            {
                                var account = await _savingsAccountRepository.GetById(int.Parse(t.DestinationAccountId));
                                if (account != null)
                                {
                                    destinationIdentifier = $"Cuenta: {account.AccountNumber.Substring(Math.Max(0, account.AccountNumber.Length - 4))}";
                                }
                            }
                            description = description ?? $"Transferencia";
                            break;
                        case TransactionType.PagoTarjetaCredito:
                            if (!string.IsNullOrEmpty(t.DestinationCardId))
                            {
                                var card = await _creditCardRepository.GetById(int.Parse(t.DestinationCardId));
                                if (card != null)
                                {
                                    destinationIdentifier = $"Tarjeta: {card.CardId.Substring(Math.Max(0, card.CardId.Length - 4))}";
                                }
                            }
                            description = description ?? $"Pago de Tarjeta";
                            break;
                        case TransactionType.PagoPrestamo:
                            if (t.DestinationLoanId.HasValue)
                            {
                                var loan = await _accountServiceForWebApp.GetLoanInfoAsync(t.DestinationLoanId.Value.ToString());
                                if (loan != null)
                                {
                                    destinationIdentifier = $"Préstamo: {loan.LoanIdentifier}"; 
                                }
                            }
                            description = description ?? $"Pago de Préstamo";
                            break;
                        case TransactionType.PagoBeneficiario:
                            if (!string.IsNullOrEmpty(t.DestinationAccountId))
                            {
                                var account = await _savingsAccountRepository.GetById(int.Parse(t.DestinationAccountId));
                                if (account != null)
                                {
                                    destinationIdentifier = $"Beneficiario (Cta: {account.AccountNumber.Substring(Math.Max(0, account.AccountNumber.Length - 4))})";
                                }
                            }
                            description = description ?? $"Pago a Beneficiario";
                            break;
                        case TransactionType.Deposito:
                            transactionTypeString = "Depósito";
                            saTransactionType = TransactionType.CREDITO.ToString();
                            destinationIdentifier = originIdentifier;
                            originIdentifier = "Externo";
                            description = description ?? "Depósito en cuenta";
                            break;
                        case TransactionType.Retiro:
                            transactionTypeString = "Retiro";
                            saTransactionType = TransactionType.DEBITO.ToString();
                            destinationIdentifier = "Efectivo/Externo";
                            description = description ?? "Retiro de efectivo";
                            break;
                        default:
                            transactionTypeString = "Otro";
                            saTransactionType = "pene";
                            description = description ?? "Transacción no clasificada";
                            break;
                    }


                    clientTransactions.Add(new DisplayTransactionDTO
                    {
                        TransactionId = t.Id.ToString(),
                        Type = transactionTypeString,
                        saTransactionType = saTransactionType,
                        Amount = t.Amount,
                        Date = t.TransactionDate ?? DateTime.Now,
                        OriginIdentifier = originIdentifier,
                        DestinationIdentifier = destinationIdentifier,
                        Description = description
                    });
                }
            }
            _logger.LogInformation($"Retrieved {clientTransactions.Count} service transactions for client: {clientId}.");
            return clientTransactions.OrderByDescending(t => t.Date).ToList();
        }
    }
}