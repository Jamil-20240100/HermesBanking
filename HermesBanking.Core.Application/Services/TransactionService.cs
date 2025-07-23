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
            _logger = logger;
        }

        public async Task<List<TransactionDTO>> GetAllTransactionsAsync()
        {
            _logger.LogInformation("Attempting to retrieve all transactions.");
            var transactions = await _transactionRepository.GetAllAsync();

            var transactionDtos = transactions.Select(t => new TransactionDTO
            {
                Id = t.Id,
                Amount = t.Amount,
                TransactionDate = t.TransactionDate ?? DateTime.MinValue,
                TransactionType = t.TransactionType,
                Description = t.Description,
                SourceAccountId = t.SourceAccountId,
                DestinationAccountId = t.DestinationAccountId,
                DestinationCardId = t.DestinationCardId,
                DestinationLoanId = t.DestinationLoanId,
                CreditCardId = t.CreditCardId
            }).OrderByDescending(t => t.TransactionDate).ToList();

            _logger.LogInformation($"Retrieved {transactionDtos.Count} transactions.");
            return transactionDtos;
        }

        public async Task PerformTransactionAsync(DTOs.Transaction.TransactionRequestDto request)
        {
            _logger.LogInformation($"Iniciando transferencia: Origen={request.SourceAccountNumber}, Destino={request.DestinationAccountNumber}, Monto={request.Amount}");

            using (var transactionScope = _unitOfWork.BeginTransaction()) // Inicia la transacción de base de datos
            {
                try
                {
                    // 1. Validar cuentas (origen y destino existen, están activas)
                    var sourceAccount = await _savingsAccountRepository.GetByAccountNumberAsync(request.SourceAccountNumber);
                    var destinationAccount = await _savingsAccountRepository.GetByAccountNumberAsync(request.DestinationAccountNumber);

                    if (sourceAccount == null) throw new InvalidOperationException("Cuenta de origen no encontrada.");
                    if (destinationAccount == null) throw new InvalidOperationException("Cuenta de destino no encontrada.");

                    if (!sourceAccount.IsActive) throw new InvalidOperationException("La cuenta de origen está inactiva.");
                    if (!destinationAccount.IsActive) throw new InvalidOperationException("La cuenta de destino está inactiva.");

                    // 2. Verificar fondos suficientes
                    if (sourceAccount.Balance < request.Amount)
                    {
                        throw new InvalidOperationException("Fondos insuficientes en la cuenta de origen.");
                    }

                    // 3. Realizar la transacción (deducir de origen, añadir a destino)
                    sourceAccount.Balance -= request.Amount;
                    destinationAccount.Balance += request.Amount;

                    await _savingsAccountRepository.UpdateAsync(sourceAccount);
                    await _savingsAccountRepository.UpdateAsync(destinationAccount);

                    // 4. Registrar la transacción
                    var transaction = new Transaction
                    {
                        Amount = request.Amount,
                        SourceAccountId = sourceAccount.Id.ToString(),
                        DestinationAccountId = destinationAccount.Id.ToString(),
                        TransactionDate = DateTime.Now,
                        TransactionType = TransactionType.Transferencia, // Usando el enum
                        Description = request.Description ?? $"Transferencia de {request.SourceAccountNumber} a {request.DestinationAccountNumber}"
                    };
                    await _transactionRepository.AddAsync(transaction);

                    await _unitOfWork.CommitAsync(); // Confirma todos los cambios en la base de datos
                    _logger.LogInformation($"Transacción exitosa de {request.Amount:C} de {request.SourceAccountNumber} a {request.DestinationAccountNumber}.");

                    // 5. Enviar notificaciones por correo electrónico (fuera de la transacción de BD, si se prefiere)
                    var sourceClientEmail = await _accountServiceForWebApp.GetUserEmailAsync(sourceAccount.ClientId);
                    var destinationClientEmail = await _accountServiceForWebApp.GetUserEmailAsync(destinationAccount.ClientId);

                    if (!string.IsNullOrEmpty(sourceClientEmail))
                    {
                        await _emailService.SendTransactionInitiatorNotificationAsync(
                            sourceClientEmail,
                            request.Amount,
                            request.DestinationAccountNumber.Substring(request.DestinationAccountNumber.Length - 4),
                            transaction.Date ?? DateTime.Now
                        );
                    }

                    if (!string.IsNullOrEmpty(destinationClientEmail) && sourceClientEmail != destinationClientEmail) // Evitar doble envío si es la misma persona
                    {
                        await _emailService.SendTransactionReceiverNotificationAsync(
                            destinationClientEmail,
                            request.Amount,
                            request.SourceAccountNumber.Substring(request.SourceAccountNumber.Length - 4),
                            transaction.TransactionDate ?? DateTime.Now
                        );
                    }
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync(); // Revierte todos los cambios si algo falla
                    _logger.LogError(ex, $"Error al realizar la transferencia entre cuentas: {ex.Message}");
                    throw; // Re-lanzar la excepción para que sea manejada por el llamador
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

                    // 2. Verificar fondos suficientes
                    if (sourceAccount.Balance < paymentDto.Amount)
                    {
                        throw new InvalidOperationException("Fondos insuficientes en la cuenta de origen.");
                    }

                    sourceAccount.Balance -= paymentDto.Amount;
                    creditCard.TotalOwedAmount -= paymentDto.Amount; // Asumiendo TotalOwedAmount es el saldo pendiente

                    await _savingsAccountRepository.UpdateAsync(sourceAccount);
                    await _creditCardRepository.UpdateAsync(creditCard);

                    // 5. Registrar la transacción
                    var transaction = new Transaction
                    {
                        Amount = paymentDto.Amount,
                        SourceAccountId = sourceAccount.Id.ToString(),
                        DestinationCardId = creditCard.Id.ToString(), // Campo para la tarjeta de destino
                        TransactionDate = DateTime.Now,
                        TransactionType = TransactionType.PagoTarjetaCredito, // Usando el enum
                        Description = $"Pago a tarjeta de crédito {paymentDto.CreditCardNumber.Substring(paymentDto.CreditCardNumber.Length - 4)}"
                    };
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
                    // 1. Validar cuentas/préstamos
                    var sourceAccount = await _savingsAccountRepository.GetByAccountNumberAsync(paymentDto.SourceAccountNumber);
                    // Suponemos que GetLoanInfoAsync devuelve una entidad Loan o un DTO de préstamo con IsActive y Id
                    var loan = await _accountServiceForWebApp.GetLoanInfoAsync(paymentDto.LoanIdentifier);

                    if (sourceAccount == null) throw new InvalidOperationException("Cuenta de origen no encontrada.");
                    if (loan == null) throw new InvalidOperationException("Préstamo no encontrado.");

                    if (!sourceAccount.IsActive) throw new InvalidOperationException("La cuenta de origen está inactiva.");
                    // Ajusta según cómo manejes el estado de un préstamo completado/inactivo
                    if (!loan.IsActive) throw new InvalidOperationException("El préstamo ya no está activo o está completado.");

                    // 2. Verificar fondos suficientes
                    if (sourceAccount.Balance < paymentDto.Amount)
                    {
                        throw new InvalidOperationException("Fondos insuficientes en la cuenta de origen.");
                    }

                    // 3. Deducir monto de la cuenta de origen
                    sourceAccount.Balance -= paymentDto.Amount;
                    await _savingsAccountRepository.UpdateAsync(sourceAccount);

                    // 4. Aplicar pago al préstamo usando el servicio de amortización
                    decimal amountAppliedToLoan = await _loanAmortizationService.ApplyPaymentToLoanAsync(loan.Id, paymentDto.Amount);

                    // 5. Registrar la transacción
                    var transaction = new Transaction
                    {
                        Amount = paymentDto.Amount, // Monto total debitado inicialmente
                        SourceAccountId = sourceAccount.Id.ToString(),
                        DestinationLoanId = loan.Id, // Campo para el préstamo de destino
                        TransactionDate = DateTime.Now,
                        TransactionType = TransactionType.PagoPrestamo, // Usando el enum
                        Description = $"Pago de préstamo {paymentDto.LoanIdentifier}"
                    };
                    await _transactionRepository.AddAsync(transaction);

                    await _unitOfWork.CommitAsync();
                    _logger.LogInformation($"Pago de préstamo {paymentDto.LoanIdentifier} por {paymentDto.Amount:C} completado exitosamente.");

                    // 6. Enviar notificación por correo
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
            _logger.LogInformation($"Iniciando pago a beneficiario: BeneficiaryId={dto.BeneficiaryId}, SourceAccount={dto.SourceAccountNumber}, Amount={dto.Amount}");

            using (var transactionScope = _unitOfWork.BeginTransaction()) // Inicia la transacción de base de datos
            {
                try
                {
                    // 1. Obtener el beneficiario por ID del BeneficiaryService
                    // Asumimos que GetByIdAsync está implementado en IBeneficiaryService y devuelve un BeneficiaryDTO
                    var beneficiaryDto = await _beneficiaryService.GetById(dto.BeneficiaryId);
                    if (beneficiaryDto == null)
                    {
                        _logger.LogWarning($"Beneficiario con ID {dto.BeneficiaryId} no encontrado.");
                        throw new InvalidOperationException("Beneficiario no encontrado.");
                    }

                    // 2. Validar cuentas (origen y destino) y fondos
                    var sourceAccount = await _savingsAccountRepository.GetByAccountNumberAsync(dto.SourceAccountNumber);
                    var destinationAccount = await _savingsAccountRepository.GetByAccountNumberAsync(beneficiaryDto.BeneficiaryAccountNumber);

                    if (sourceAccount == null) throw new InvalidOperationException("Cuenta de origen no encontrada.");
                    if (destinationAccount == null) throw new InvalidOperationException("Cuenta de destino del beneficiario no encontrada.");

                    if (!sourceAccount.IsActive) throw new InvalidOperationException("La cuenta de origen está inactiva.");
                    if (!destinationAccount.IsActive) throw new InvalidOperationException("La cuenta de destino del beneficiario está inactiva.");

                    // 2.1. Validar fondos suficientes
                    if (sourceAccount.Balance < dto.Amount)
                    {
                        throw new InvalidOperationException("Fondos insuficientes en la cuenta de origen para realizar el pago al beneficiario.");
                    }

                    // 3. Realizar la transferencia (débito de origen, crédito a destino)
                    sourceAccount.Balance -= dto.Amount;
                    destinationAccount.Balance += dto.Amount;

                    await _savingsAccountRepository.UpdateAsync(sourceAccount);
                    await _savingsAccountRepository.UpdateAsync(destinationAccount);

                    // 4. Registrar la transacción
                    var transaction = new Transaction
                    {
                        Amount = dto.Amount,
                        SourceAccountId = sourceAccount.Id.ToString(),
                        DestinationAccountId = destinationAccount.Id.ToString(),
                        TransactionDate = DateTime.Now,
                        TransactionType = TransactionType.PagoBeneficiario, // Usando el nuevo tipo de transacción
                        Description = dto.Description ?? $"Pago a beneficiario: {beneficiaryDto.Name} {beneficiaryDto.LastName}"
                    };
                    await _transactionRepository.AddAsync(transaction);

                    await _unitOfWork.CommitAsync(); // Confirma la transacción de base de datos
                    _logger.LogInformation($"Pago a beneficiario {beneficiaryDto.Name} {beneficiaryDto.LastName} completado exitosamente desde la cuenta {dto.SourceAccountNumber} por {dto.Amount:C}.");

                    // 5. Enviar notificaciones por correo electrónico
                    var sourceClientEmail = await _accountServiceForWebApp.GetUserEmailAsync(sourceAccount.ClientId);
                    var destinationClientEmail = await _accountServiceForWebApp.GetUserEmailAsync(destinationAccount.ClientId); // Email del titular de la cuenta del beneficiario

                    // Correo al cliente que realizó la transacción
                    if (!string.IsNullOrEmpty(sourceClientEmail))
                    {
                        await _emailService.SendTransactionInitiatorNotificationAsync(
                            sourceClientEmail,
                            dto.Amount,
                            beneficiaryDto.BeneficiaryAccountNumber.Substring(beneficiaryDto.BeneficiaryAccountNumber.Length - 4), // Últimos 4 dígitos del beneficiario
                            transaction.TransactionDate ?? DateTime.Now
                        );
                    }

                    // Correo al beneficiario
                    if (!string.IsNullOrEmpty(destinationClientEmail) && sourceClientEmail != destinationClientEmail) // Evitar doble envío si es la misma persona
                    {
                        await _emailService.SendTransactionReceiverNotificationAsync(
                            destinationClientEmail,
                            dto.Amount,
                            dto.SourceAccountNumber.Substring(dto.SourceAccountNumber.Length - 4), // Últimos 4 dígitos de la cuenta de origen
                            transaction.TransactionDate ?? DateTime.Now
                        );
                    }
                }
                catch (InvalidOperationException ex)
                {
                    await _unitOfWork.RollbackAsync(); // Revertir si hay un error de validación
                    _logger.LogWarning($"Validación fallida para pago a beneficiario: {ex.Message}");
                    throw; // Re-lanzar la excepción para manejo en la capa superior (UI)
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync(); // Revertir si hay un error inesperado
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
                            destinationIdentifier = originIdentifier;
                            originIdentifier = "Externo";
                            description = description ?? "Depósito en cuenta";
                            break;
                        case TransactionType.Retiro:
                            transactionTypeString = "Retiro";
                            destinationIdentifier = "Efectivo/Externo";
                            description = description ?? "Retiro de efectivo";
                            break;
                        default:
                            transactionTypeString = "Otro";
                            description = description ?? "Transacción no clasificada";
                            break;
                    }


                    clientTransactions.Add(new DisplayTransactionDTO
                    {
                        TransactionId = t.Id.ToString(),
                        Type = transactionTypeString,
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