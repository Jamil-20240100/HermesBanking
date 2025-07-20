using AutoMapper;
using HermesBanking.Core.Application.DTOs;
using HermesBanking.Core.Application.DTOs.Transaction;
using HermesBanking.Core.Application.DTOs.Transfer;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace HermesBanking.Infrastructure.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ISavingsAccountRepository _savingsAccountRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly IGenericRepository<Transaction> _transactionGenericRepo;
        private readonly IGenericService<TransactionDTO> _genericService;
        private readonly IMapper _mapper;
        private readonly ILoanRepository _loanRepo;
        private readonly ICreditCardRepository _creditCardRepo;

        public TransactionService(
            ISavingsAccountRepository savingsAccountRepo,
            ITransactionRepository transactionRepo,
            IGenericService<TransactionDTO> genericService,
            IGenericRepository<Transaction> transactionGenericRepo,
            IMapper mapper,
            ILoanRepository loanRepo,           
            ICreditCardRepository creditCardRepo)
        {
            _genericService = genericService;
            _savingsAccountRepo = savingsAccountRepo;
            _transactionRepo = transactionRepo;
            _transactionGenericRepo = transactionGenericRepo;
            _mapper = mapper;
            _loanRepo = loanRepo;            
            _creditCardRepo = creditCardRepo;
        }

        public async Task ExecuteExpressTransactionAsync(ExpressTransactionDTO dto)
        {
            using var dbTransaction = await _transactionRepo.BeginTransactionAsync();

            try
            {
                var senderAccount = await _savingsAccountRepo.GetByAccountNumberAsync(dto.SenderAccountNumber);
                var receiverAccount = await _savingsAccountRepo.GetByAccountNumberAsync(dto.ReceiverAccountNumber);

                if (senderAccount == null || !senderAccount.IsActive)
                    throw new InvalidOperationException("Cuenta origen inválida o inactiva.");

                if (receiverAccount == null || !receiverAccount.IsActive)
                    throw new InvalidOperationException("Cuenta destino inválida o inactiva.");

                if (senderAccount.Balance < dto.Amount)
                    throw new InvalidOperationException("Fondos insuficientes en la cuenta origen.");

                senderAccount.Balance -= dto.Amount;
                receiverAccount.Balance += dto.Amount;

                var transaction = new Transaction
                {
                    Amount = dto.Amount,
                    Origin = dto.SenderAccountNumber,
                    Beneficiary = dto.ReceiverAccountNumber,
                    Date = DateTime.Now,
                    Type = "Express",
                    Description = $"Transferencia express RD$ {dto.Amount} de {dto.SenderAccountNumber} a {dto.ReceiverAccountNumber}.",
                    SavingsAccountId = dto.SavingsAccountId // Asigna el SavingsAccountId
                };

                await _transactionGenericRepo.AddAsync(transaction);
                await _savingsAccountRepo.UpdateAsync(senderAccount.Id, senderAccount);
                await _savingsAccountRepo.UpdateAsync(receiverAccount.Id, receiverAccount);

                await dbTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                throw new InvalidOperationException("Error ejecutando la transacción Express", ex);
            }
        }

        public async Task<bool> RegisterTransactionAsync(TransactionDTO transactionDto)
        {
            var transaction = new Transaction
            {
                Amount = transactionDto.Amount,  // Usar el DTO tal cual
                Origin = transactionDto.Origin,  // Usar el número de cuenta de origen
                Beneficiary = transactionDto.Beneficiary,  // Usar el número de cuenta o destino
                Date = DateTime.Now,  // La fecha de la transacción es la actual
                Type = transactionDto.Type,  // Tipo de transacción
                Description = transactionDto.Description,  // Descripción de la transacción
                SavingsAccountId = transactionDto.SavingsAccountId.Value // Usamos el SavingsAccountId del DTO
            };

            await _transactionGenericRepo.AddAsync(transaction); 
            return true;
        }

        public async Task<bool> ValidateAccountExistsAndActive(string accountNumber)
        {
            var account = await _savingsAccountRepo.GetByAccountNumberAsync(accountNumber);
            return account != null && account.IsActive;
        }

        public async Task<bool> HasSufficientFunds(string accountNumber, decimal amount)
        {
            var account = await _savingsAccountRepo.GetByAccountNumberAsync(accountNumber);
            return account != null && account.Balance >= amount;
        }

        public async Task ProcessTransferAsync(TransferDTO dto)
        {
            using var dbTransaction = await _transactionRepo.BeginTransactionAsync();

            try
            {
                var senderAccount = await _savingsAccountRepo.GetById(dto.SourceAccountId);
                var receiverAccount = await _savingsAccountRepo.GetById(dto.DestinationAccountId);

                if (senderAccount == null || !senderAccount.IsActive)
                    throw new InvalidOperationException("Cuenta de origen inválida o inactiva.");

                if (receiverAccount == null || !receiverAccount.IsActive)
                    throw new InvalidOperationException("Cuenta destino inválida o inactiva.");

                if (senderAccount.Balance < dto.Amount)
                    throw new InvalidOperationException("Fondos insuficientes en la cuenta origen.");

                senderAccount.Balance -= dto.Amount;
                receiverAccount.Balance += dto.Amount;

                var debitTransaction = new Transaction
                {
                    Amount = dto.Amount,
                    Origin = senderAccount.AccountNumber,
                    Beneficiary = receiverAccount.AccountNumber,
                    Date = DateTime.Now,
                    Type = "Transferencia",
                    Description = $"Transferencia de RD$ {dto.Amount} de {senderAccount.AccountNumber} a {receiverAccount.AccountNumber}."
                };

                var creditTransaction = new Transaction
                {
                    Amount = dto.Amount,
                    Origin = senderAccount.AccountNumber,
                    Beneficiary = receiverAccount.AccountNumber,
                    Date = DateTime.Now,
                    Type = "Transferencia",
                    Description = $"Transferencia de RD$ {dto.Amount} desde {senderAccount.AccountNumber} a {receiverAccount.AccountNumber}."
                };

                await _transactionGenericRepo.AddAsync(debitTransaction);
                await _transactionGenericRepo.AddAsync(creditTransaction);

                await _savingsAccountRepo.UpdateAsync(senderAccount.Id, senderAccount);
                await _savingsAccountRepo.UpdateAsync(receiverAccount.Id, receiverAccount);

                await dbTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                throw new InvalidOperationException("Error ejecutando la transferencia", ex);
            }
        }

        public async Task<TransactionDTO?> GetByIdAsync(int id)
        {
            var transaction = await _genericService.GetById(id); // Usar GenericService para obtener la transacción
            return transaction;
        }
    

    public async Task ExecutePayCreditCardTransactionAsync(PayCreditCardDTO dto)
        {
            using var dbTransaction = await _transactionRepo.BeginTransactionAsync();

            try
            {
                var senderAccount = await _savingsAccountRepo.GetByAccountNumberAsync(dto.FromAccountNumber);
                var creditCard = await _creditCardRepo.GetByCardNumberAsync(dto.CreditCardNumber);

                if (senderAccount == null || !senderAccount.IsActive)
                    throw new InvalidOperationException("Cuenta origen inválida o inactiva.");

                if (creditCard == null || !creditCard.IsActive)
                    throw new InvalidOperationException("Tarjeta de crédito inválida o inactiva.");

                if (senderAccount.Balance < dto.Amount)
                    throw new InvalidOperationException("Fondos insuficientes en la cuenta origen.");

                senderAccount.Balance -= dto.Amount;
                creditCard.CreditLimit -= dto.Amount;

                var transaction = new Transaction
                {
                    Amount = dto.Amount,
                    Origin = dto.FromAccountNumber,
                    Beneficiary = dto.CreditCardNumber,
                    Date = DateTime.Now,
                    Type = "CreditCard",
                    Description = $"Pago de tarjeta de crédito RD$ {dto.Amount} desde {dto.FromAccountNumber} a {dto.CreditCardNumber}."
                };

                // Guardar la transacción
                await _transactionGenericRepo.AddAsync(transaction);
                await _savingsAccountRepo.UpdateAsync(senderAccount.Id, senderAccount);
                await _creditCardRepo.UpdateAsync(creditCard.Id, creditCard);

                await dbTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                throw new InvalidOperationException("Error ejecutando la transacción de pago de tarjeta de crédito", ex);
            }
        }


        public async Task ExecutePayLoanTransactionAsync(PayLoanDTO dto)
        {
            using var dbTransaction = await _transactionRepo.BeginTransactionAsync();

            try
            {
                var senderAccount = await _savingsAccountRepo.GetByAccountNumberAsync(dto.FromAccountNumber);
                var loan = await _loanRepo.GetLoanByIdentifierAsync(dto.LoanCode);

                if (senderAccount == null || !senderAccount.IsActive)
                    throw new InvalidOperationException("Cuenta origen inválida o inactiva.");

                if (loan == null || !loan.IsActive)
                    throw new InvalidOperationException("Préstamo inválido o inactivo.");

                if (senderAccount.Balance < dto.Amount)
                    throw new InvalidOperationException("Fondos insuficientes en la cuenta origen.");

                senderAccount.Balance -= dto.Amount;
                loan.Amount -= dto.Amount;

                var transaction = new Transaction
                {
                    Amount = dto.Amount,
                    Origin = dto.FromAccountNumber,
                    Beneficiary = dto.LoanCode,
                    Date = DateTime.Now,
                    Type = "Loan",
                    Description = $"Pago de préstamo RD$ {dto.Amount} desde {dto.FromAccountNumber} al préstamo {dto.LoanCode}."
                };

                // Guardar la transacción
                await _transactionGenericRepo.AddAsync(transaction);
                await _savingsAccountRepo.UpdateAsync(senderAccount.Id, senderAccount);
                await _loanRepo.UpdateAsync(loan.Id, loan);

                await dbTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                throw new InvalidOperationException("Error ejecutando la transacción de pago de préstamo", ex);
            }
        }

        public async Task ExecutePayBeneficiaryTransactionAsync(PayBeneficiaryDTO dto)
        {
            using var dbTransaction = await _transactionRepo.BeginTransactionAsync();

            try
            {
                var senderAccount = await _savingsAccountRepo.GetByAccountNumberAsync(dto.FromAccountNumber);
                var beneficiaryAccount = await _savingsAccountRepo.GetByAccountNumberAsync(dto.BeneficiaryAccountNumber);

                if (senderAccount == null || !senderAccount.IsActive)
                    throw new InvalidOperationException("Cuenta origen inválida o inactiva.");

                if (beneficiaryAccount == null || !beneficiaryAccount.IsActive)
                    throw new InvalidOperationException("Cuenta beneficiario inválida o inactiva.");

                if (senderAccount.Balance < dto.Amount)
                    throw new InvalidOperationException("Fondos insuficientes en la cuenta origen.");

                senderAccount.Balance -= dto.Amount;
                beneficiaryAccount.Balance += dto.Amount;

                var transaction = new Transaction
                {
                    Amount = dto.Amount,
                    Origin = dto.FromAccountNumber,
                    Beneficiary = dto.BeneficiaryAccountNumber,
                    Date = DateTime.Now,
                    Type = "Beneficiary",
                    Description = $"Transferencia a beneficiario RD$ {dto.Amount} desde {dto.FromAccountNumber} a {dto.BeneficiaryAccountNumber}."
                };

                // Guardar la transacción
                await _transactionGenericRepo.AddAsync(transaction);
                await _savingsAccountRepo.UpdateAsync(senderAccount.Id, senderAccount);
                await _savingsAccountRepo.UpdateAsync(beneficiaryAccount.Id, beneficiaryAccount);

                await dbTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                throw new InvalidOperationException("Error ejecutando la transacción a beneficiario", ex);
            }
        }
        






    }
}