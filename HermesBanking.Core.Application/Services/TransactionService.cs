using AutoMapper;
using HermesBanking.Core.Application.DTOs;
using HermesBanking.Core.Application.DTOs.Transaction;
using HermesBanking.Core.Application.DTOs.Transfer;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Core.Domain.Common.Enums;
using System;
using System.Threading.Tasks;

namespace HermesBanking.Infrastructure.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ISavingsAccountRepository _savingsAccountRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly IMapper _mapper;
        private readonly ILoanRepository _loanRepo;
        private readonly ICreditCardRepository _creditCardRepo;
        private readonly IBeneficiaryRepository _beneficiaryRepo; 

        public TransactionService(
            ISavingsAccountRepository savingsAccountRepo,
            ITransactionRepository transactionRepo,
            IMapper mapper,
            ILoanRepository loanRepo,
            ICreditCardRepository creditCardRepo,
            IBeneficiaryRepository beneficiaryRepo)
        {
            _savingsAccountRepo = savingsAccountRepo;
            _transactionRepo = transactionRepo;
            _mapper = mapper;
            _loanRepo = loanRepo;
            _creditCardRepo = creditCardRepo;
            _beneficiaryRepo = beneficiaryRepo;
        }

        // Transacción Express
        public async Task ExecuteExpressTransactionAsync(ExpressTransactionDTO dto)
        {
            using var dbTransaction = await _transactionRepo.BeginTransactionAsync();

            try
            {
                // Verificar existencia de cuentas
                var senderAccount = await _savingsAccountRepo.GetByAccountNumberAsync(dto.SenderAccountNumber);
                var receiverAccount = await _savingsAccountRepo.GetByAccountNumberAsync(dto.ReceiverAccountNumber);

                if (senderAccount == null || !senderAccount.IsActive)
                    throw new InvalidOperationException($"Cuenta origen {dto.SenderAccountNumber} no válida o inactiva.");

                if (receiverAccount == null || !receiverAccount.IsActive)
                    throw new InvalidOperationException($"Cuenta destino {dto.ReceiverAccountNumber} no válida o inactiva.");

                // Verificar fondos suficientes en la cuenta de origen
                if (senderAccount.Balance < dto.Amount)
                    throw new InvalidOperationException("Fondos insuficientes en la cuenta origen.");

                // Realizar la transacción
                senderAccount.Balance -= dto.Amount;
                receiverAccount.Balance += dto.Amount;

                // Crear la transacción
                var transaction = new Transaction
                {
                    Amount = dto.Amount,
                    Origin = dto.SenderAccountNumber,
                    Beneficiary = dto.ReceiverAccountNumber,
                    Date = DateTime.Now,
                    Type = FrTransactionType.Express.ToString(), // Usamos el enum aquí
                    Description = $"Transferencia express RD$ {dto.Amount} de {dto.SenderAccountNumber} a {dto.ReceiverAccountNumber}.",
                    SavingsAccountId = senderAccount.Id // Asignar el ID de la cuenta origen
                };

                // Guardar la transacción
                await _transactionRepo.AddAsync(transaction);

                // Actualizar las cuentas
                await _savingsAccountRepo.UpdateAsync(senderAccount.Id, senderAccount);
                await _savingsAccountRepo.UpdateAsync(receiverAccount.Id, receiverAccount);

                // Confirmar transacción
                await dbTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                throw new InvalidOperationException("Error ejecutando la transacción Express", ex);
            }
        }

        // Verificar existencia y estado activo de cuentas
        public async Task<bool> ValidateAccountExistsAndActive(string accountNumber)
        {
            var account = await _savingsAccountRepo.GetByAccountNumberAsync(accountNumber);
            return account != null && account.IsActive;
        }

        // Verificar si hay fondos suficientes en la cuenta
        public async Task<bool> HasSufficientFunds(string accountNumber, decimal amount)
        {
            var account = await _savingsAccountRepo.GetByAccountNumberAsync(accountNumber);
            if (account == null)
                throw new InvalidOperationException($"La cuenta {accountNumber} no existe.");

            return account.Balance >= amount;
        }

        // Transacción de Transferencia
        public async Task ProcessTransferAsync(TransferDTO dto)
        {
            using var dbTransaction = await _transactionRepo.BeginTransactionAsync();

            try
            {
                // Verificar existencia de cuentas
                var senderAccount = await _savingsAccountRepo.GetById(dto.SourceAccountId);
                var receiverAccount = await _savingsAccountRepo.GetById(dto.DestinationAccountId);

                if (senderAccount == null || !senderAccount.IsActive)
                    throw new InvalidOperationException("Cuenta de origen inválida o inactiva.");

                if (receiverAccount == null || !receiverAccount.IsActive)
                    throw new InvalidOperationException("Cuenta destino inválida o inactiva.");

                // Verificar fondos
                if (senderAccount.Balance < dto.Amount)
                    throw new InvalidOperationException("Fondos insuficientes en la cuenta origen.");

                // Realizar la transacción
                senderAccount.Balance -= dto.Amount;
                receiverAccount.Balance += dto.Amount;

                // Crear y guardar transacciones de débito y crédito
                var debitTransaction = new Transaction
                {
                    Amount = dto.Amount,
                    Origin = senderAccount.AccountNumber,
                    Beneficiary = receiverAccount.AccountNumber,
                    Date = DateTime.Now,
                    Type = FrTransactionType.CreditCard.ToString(), // Usamos el enum aquí
                    Description = $"Transferencia de RD$ {dto.Amount} de {senderAccount.AccountNumber} a {receiverAccount.AccountNumber}."
                };

                var creditTransaction = new Transaction
                {
                    Amount = dto.Amount,
                    Origin = senderAccount.AccountNumber,
                    Beneficiary = receiverAccount.AccountNumber,
                    Date = DateTime.Now,
                    Type = FrTransactionType.CreditCard.ToString(), // Usamos el enum aquí
                    Description = $"Transferencia de RD$ {dto.Amount} desde {senderAccount.AccountNumber} a {receiverAccount.AccountNumber}."
                };

                await _transactionRepo.AddAsync(debitTransaction);
                await _transactionRepo.AddAsync(creditTransaction);

                // Actualizar las cuentas
                await _savingsAccountRepo.UpdateAsync(senderAccount.Id, senderAccount);
                await _savingsAccountRepo.UpdateAsync(receiverAccount.Id, receiverAccount);

                // Confirmar la transacción
                await dbTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                throw new InvalidOperationException("Error ejecutando la transferencia", ex);
            }
        }

        //Transacción de Pago de Tarjeta de Crédito
        public async Task ExecutePayCreditCardTransactionAsync(PayCreditCardDTO dto)
        {
            using var dbTransaction = await _transactionRepo.BeginTransactionAsync();

            try
            {
                // Verificar existencia de cuenta origen
                var senderAccount = await _savingsAccountRepo.GetByAccountNumberAsync(dto.FromAccountNumber);
                if (senderAccount == null || !senderAccount.IsActive)
                    throw new InvalidOperationException($"Cuenta origen {dto.FromAccountNumber} no válida o inactiva.");

                // Verificar existencia de tarjeta de crédito
                var creditCard = await _creditCardRepo.GetByCardNumberAsync(dto.CreditCardNumber);
                if (creditCard == null || !creditCard.IsActive)
                    throw new InvalidOperationException($"Tarjeta de crédito {dto.CreditCardNumber} no válida o inactiva.");

                // Verificar fondos suficientes en la cuenta origen
                if (senderAccount.Balance < dto.Amount)
                    throw new InvalidOperationException("Fondos insuficientes en la cuenta origen.");

                // Realizar la transacción
                senderAccount.Balance -= dto.Amount;
                creditCard.CreditLimit -= dto.Amount;

                // Crear la transacción
                var transaction = new Transaction
                {
                    Amount = dto.Amount,
                    Origin = dto.FromAccountNumber,
                    Beneficiary = dto.CreditCardNumber,
                    Date = DateTime.Now,
                    Type = FrTransactionType.CreditCard.ToString(), // Usamos el enum aquí
                    Description = $"Pago de tarjeta de crédito RD$ {dto.Amount} desde {dto.FromAccountNumber} a {dto.CreditCardNumber}.",
                    SavingsAccountId = senderAccount.Id // Asignar el ID de la cuenta origen
                };

                // Guardar la transacción
                await _transactionRepo.AddAsync(transaction);

                // Actualizar las cuentas
                await _savingsAccountRepo.UpdateAsync(senderAccount.Id, senderAccount);
                await _creditCardRepo.UpdateAsync(creditCard.Id, creditCard);

                // Confirmar la transacción
                await dbTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                throw new InvalidOperationException("Error ejecutando la transacción de pago de tarjeta de crédito", ex);
            }
        }

        //Validacion de CreditCard

        // Verificar existencia y estado activo de tarjetas de crédito
        public async Task<bool> ValidateCreditCardExistsAndActive(string cardNumber)
        {
            var creditCard = await _creditCardRepo.GetByCardNumberAsync(cardNumber);
            return creditCard != null && creditCard.IsActive;
        }


        // Transacción de Pago de Préstamo
        public async Task ExecutePayLoanTransactionAsync(PayLoanDTO dto)
        {
            using var dbTransaction = await _transactionRepo.BeginTransactionAsync();

            try
            {
                // Verificar existencia de cuenta y préstamo
                var senderAccount = await _savingsAccountRepo.GetByAccountNumberAsync(dto.FromAccountNumber);
                var loan = await _loanRepo.GetLoanByIdentifierAsync(dto.LoanCode);

                if (senderAccount == null || !senderAccount.IsActive)
                    throw new InvalidOperationException("Cuenta origen inválida o inactiva.");

                if (loan == null || !loan.IsActive)
                    throw new InvalidOperationException("Préstamo inválido o inactivo.");

                // Verificar fondos
                if (senderAccount.Balance < dto.Amount)
                    throw new InvalidOperationException("Fondos insuficientes en la cuenta origen.");

                // Realizar la transacción
                senderAccount.Balance -= dto.Amount;
                loan.Amount -= dto.Amount;

                var transaction = new Transaction
                {
                    Amount = dto.Amount,
                    Origin = dto.FromAccountNumber,
                    Beneficiary = dto.LoanCode,
                    Date = DateTime.Now,
                    Type = FrTransactionType.Loan.ToString(), // Usamos el enum aquí
                    Description = $"Pago de préstamo RD$ {dto.Amount} desde {dto.FromAccountNumber} al préstamo {dto.LoanCode}."
                };

                await _transactionRepo.AddAsync(transaction);
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


        // Verificar existencia de préstamos
        public async Task<bool> ValidateLoanExistsAndActive(string loanIdentifier)
        {
            var loan = await _loanRepo.GetLoanByIdentifierAsync(loanIdentifier);
            return loan != null && loan.IsActive; // Asumiendo que 'IsActive' es un campo en la entidad Loan
        }


        //Pago de beneficiario
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
                    Type = FrTransactionType.Beneficiary.ToString(),
                    Description = $"Transferencia a beneficiario RD$ {dto.Amount} desde {dto.FromAccountNumber} a {dto.BeneficiaryAccountNumber}."
                };

                // Guardar la transacción
                await _transactionRepo.AddAsync(transaction);
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


        // Verificar existencia de beneficiarios
        public async Task<bool> ValidateBeneficiaryExists(string beneficiaryAccountNumber)
        {
            var beneficiary = await _beneficiaryRepo.GetBeneficiariesByClientIdAsync(beneficiaryAccountNumber); // Asumiendo que tienes el repositorio y método adecuado
            return beneficiary != null;
        }


        public async Task<TransactionDTO?> GetByIdAsync(int id)
        {
            var transaction = await _transactionRepo.GetByIdAsync(id);
            return _mapper.Map<TransactionDTO>(transaction);
        }


        public async Task<bool> RegisterTransactionAsync(TransactionDTO transactionDto)
        {
            var transaction = _mapper.Map<Transaction>(transactionDto);
            await _transactionRepo.AddAsync(transaction);
            return true;
        }

        



    }
}
