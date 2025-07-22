// HermesBanking.Core.Application.Services/LoanAmortizationService.cs
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Common.Enums;
using System.Linq; // Add this for LINQ methods like Sum

namespace HermesBanking.Core.Application.Services
{
    public class LoanAmortizationService : ILoanAmortizationService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IAmortizationInstallmentRepository _installmentRepository;
        private readonly ISavingsAccountRepository _savingsAccountRepository;
        private readonly IAccountServiceForWebApp _accountServiceForWebApp;
        private readonly IEmailService _emailService;

        public LoanAmortizationService(
            ILoanRepository loanRepository,
            IAmortizationInstallmentRepository installmentRepository,
            ISavingsAccountRepository savingsAccountRepository,
            IAccountServiceForWebApp accountServiceForWebApp,
            IEmailService emailService)
        {
            _loanRepository = loanRepository;
            _installmentRepository = installmentRepository;
            _savingsAccountRepository = savingsAccountRepository;
            _accountServiceForWebApp = accountServiceForWebApp;
            _emailService = emailService;
        }

        public async Task<decimal> ApplyPaymentToLoanAsync(int loanId, decimal paymentAmount)
        {
            // 1. Get the loan. Make sure it loads related AmortizationInstallments if possible
            // If _loanRepository.GetById(loanId) doesn't include installments, you'll need
            // to fetch them separately, or modify GetById to include them.
            // For now, we're fetching installments separately using _installmentRepository.
            var loan = await _loanRepository.GetById(loanId);
            if (loan == null)
            {
                throw new InvalidOperationException("Préstamo no encontrado.");
            }

            if (!loan.IsActive)
            {
                throw new InvalidOperationException("El préstamo no está activo o ya ha sido completado.");
            }

            // Get outstanding installments, ordered by DueDate (or InstallmentNumber)
            var outstandingInstallments = (await _installmentRepository
                                                .GetByConditionAsync(i => i.LoanId == loanId && !i.IsPaid))
                                                .OrderBy(i => i.DueDate) // Assuming DueDate for scheduled payment
                                                .ToList();

            decimal amountApplied = 0;
            decimal remainingPayment = paymentAmount;

            // 2. Apply payment to installments
            foreach (var installment in outstandingInstallments)
            {
                if (remainingPayment <= 0) break; // No more payment left to apply

                // The amount still needed for THIS specific installment to be fully paid
                decimal amountNeededForThisInstallment = installment.InstallmentValue - installment.AmountPaid;

                if (amountNeededForThisInstallment <= 0)
                {
                    // This installment is already fully paid or overpaid, skip it.
                    continue;
                }

                // Determine how much of the current payment applies to this installment
                decimal actualAmountToApply = Math.Min(remainingPayment, amountNeededForThisInstallment);

                // Apply the payment
                installment.AmountPaid += actualAmountToApply;
                remainingPayment -= actualAmountToApply;
                amountApplied += actualAmountToApply;

                // Update installment status
                if (installment.AmountPaid >= installment.InstallmentValue)
                {
                    installment.IsPaid = true;
                    installment.PaidDate = DateTime.Now; // Set actual payment date
                    installment.IsOverdue = false; // Once paid, it's no longer overdue
                }
                else
                {
                    // It's a partial payment, so it's not fully paid yet
                    installment.IsPaid = false;
                    installment.PaidDate = null; // No full payment date yet
                    // Check if it's still overdue after partial payment
                    installment.IsOverdue = installment.DueDate < DateTime.Today; // Use Today to avoid issues with time component
                }

                // Update the individual installment in the repository
                // It's crucial that this update actually persists the changes to AmountPaid, IsPaid, PaidDate
                await _installmentRepository.UpdateAsync(installment);
            }

            // 3. Update the main loan entity's properties
            // Recalculate PendingAmount based on the *updated* installments
            // You might need to re-fetch or re-evaluate outstandingInstallments after updates,
            // or simply sum up the *remaining* amount on all installments.
            // Let's re-fetch outstanding installments to ensure accuracy if UpdateAsync detaches them.
            var updatedOutstandingInstallments = (await _installmentRepository
                                                        .GetByConditionAsync(i => i.LoanId == loanId))
                                                        .ToList();

            // Calculate current total outstanding from the installments
            loan.PendingAmount = updatedOutstandingInstallments.Where(i => !i.IsPaid)
                                                               .Sum(i => i.InstallmentValue - i.AmountPaid);

            // If you have a different definition for PendingAmount (e.g., in Loan entity itself), adjust here.
            // For now, assuming it reflects the sum of remaining amounts on installments.

            if (loan.PendingAmount <= 0)
            {
                loan.IsActive = false;
                loan.CompletedAt = DateTime.Now;
            }

            // Check if loan is no longer overdue in general
            loan.IsOverdue = updatedOutstandingInstallments.Any(i => !i.IsPaid && i.DueDate < DateTime.Today);

            await _loanRepository.UpdateAsync(loan);

            // 4. Handle remainingPayment (excess)
            if (remainingPayment > 0)
            {
                var clientPrimaryAccount = (await _savingsAccountRepository
                                                .GetByConditionAsync(sa => sa.ClientId == loan.ClientId && sa.AccountType == AccountType.Primary))
                                                .FirstOrDefault();

                if (clientPrimaryAccount != null)
                {
                    clientPrimaryAccount.Balance += remainingPayment;
                    await _savingsAccountRepository.UpdateAsync(clientPrimaryAccount);
                    amountApplied += remainingPayment; // Add excess returned to total applied
                }
                else
                {
                    Console.WriteLine($"Warning: No primary savings account found for client {loan.ClientId} to return excess payment of {remainingPayment:C}.");
                }
            }

            // 5. Send loan payment notification email
            var clientEmail = await _accountServiceForWebApp.GetUserEmailAsync(loan.ClientId);
            if (!string.IsNullOrEmpty(clientEmail))
            {
                await _emailService.SendEmailAsync(clientEmail, $"Confirmación de Pago de Préstamo {loan.LoanIdentifier}",
                    $"Se ha aplicado un pago de {paymentAmount:C} a su préstamo {loan.LoanIdentifier}. Saldo pendiente: {loan.PendingAmount:C}.");
            }

            return amountApplied;
        }
    }
}