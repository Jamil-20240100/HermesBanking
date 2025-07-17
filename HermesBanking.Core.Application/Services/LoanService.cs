using AutoMapper;
using HermesBanking.Core.Application.DTOs.Loan;
using HermesBanking.Core.Application.DTOs.SavingsAccount;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Common.Enums;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;

namespace HermesBanking.Core.Application.Services
{
    public class LoanService : ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IAmortizationInstallmentRepository _installmentRepository;
        private readonly IAccountServiceForWebApp _accountServiceForWebApp;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ISavingsAccountRepository _savingsAccountRepository;

        public LoanService(
            ILoanRepository loanRepository,
            IAmortizationInstallmentRepository installmentRepository,
            IAccountServiceForWebApp accountServiceForWebApp,
            IEmailService emailService,
            IMapper mapper, ISavingsAccountRepository savingsAccountRepository)
        {
            _loanRepository = loanRepository;
            _installmentRepository = installmentRepository;
            _accountServiceForWebApp = accountServiceForWebApp;
            _emailService = emailService;
            _mapper = mapper;
            _savingsAccountRepository = savingsAccountRepository;
        }

        public async Task<List<LoanDTO>> GetAllLoansAsync(string? cedula, string? status)
        {
            var loans = await _loanRepository.GetAll();

            if (!string.IsNullOrWhiteSpace(cedula))
            {
                var user = await _accountServiceForWebApp.GetUserByIdentificationNumber(cedula);
                if (user != null)
                {
                    loans = loans.Where(l => l.ClientId == user.Id).ToList();
                }
                else
                {
                    return new List<LoanDTO>();
                }
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "all")
            {
                if (status.ToLower() == "active")
                {
                    loans = loans.Where(l => l.IsActive).ToList();
                }
                else if (status.ToLower() == "completed")
                {
                    loans = loans.Where(l => !l.IsActive).ToList();
                }
            }

            loans = loans.OrderByDescending(l => l.IsActive)
                         .ThenByDescending(l => l.CreatedAt)
                         .ToList();

            return _mapper.Map<List<LoanDTO>>(loans);
        }

        public async Task<LoanDTO> GetLoanByIdAsync(int loanId)
        {
            var loan = await _loanRepository.GetById(loanId);
            return _mapper.Map<LoanDTO>(loan);
        }

        public async Task AddLoanAsync(CreateLoanDTO loanDto, string adminId, string adminFullName)
        {
            var loan = _mapper.Map<Loan>(loanDto);
            loan.AssignedByAdminId = adminId;
            loan.AdminFullName = adminFullName;
            loan.CreatedAt = DateTime.Now;
            loan.TotalInstallments = loan.LoanTermMonths;

            var client = await _accountServiceForWebApp.GetUserById(loanDto.ClientId);
            if (client == null)
            {
                throw new InvalidOperationException("Cliente no encontrado para la asignación del préstamo.");
            }
            loan.ClientFullName = $"{client.Name} {client.LastName}";

            loan.MonthlyInstallmentValue = CalculateAmortizationSchedule(
                loan.Amount, loan.InterestRate, loan.LoanTermMonths, loan.CreatedAt).First().InstallmentValue;

            await _loanRepository.AddAsync(loan);

            var installments = CalculateAmortizationSchedule(
                loan.Amount, loan.InterestRate, loan.LoanTermMonths, loan.CreatedAt);

            foreach (var instDto in installments)
            {
                var installment = _mapper.Map<AmortizationInstallment>(instDto);
                installment.LoanId = loan.Id;
                await _installmentRepository.AddAsync(installment);
            }

            await _accountServiceForWebApp.UpdateSavingsAccountBalance(loan.ClientId, loan.Amount);

            //
            //assign loan to savings account
            //

            var accounts = await _savingsAccountRepository
                .GetByConditionAsync(sa => sa.ClientId == loan.ClientId && sa.AccountType == AccountType.Primary);

            var account = accounts.FirstOrDefault();

            if (account == null)
                throw new InvalidOperationException("No se encontró una cuenta de ahorro principal para este cliente.");

            account.Balance += loan.Amount;

            await _savingsAccountRepository.UpdateAsync(account);

            if (client.Email != null)
            {
                await _emailService.SendLoanApprovedEmail(
                    client.Email, loan.Amount, loan.LoanTermMonths, loan.InterestRate, loan.MonthlyInstallmentValue);
            }
        }

        public async Task UpdateLoanInterestRateAsync(int loanId, decimal newInterestRate)
        {
            var loan = await _loanRepository.GetById(loanId);
            if (loan == null)
            {
                throw new InvalidOperationException("Préstamo no encontrado.");
            }

            decimal oldMonthlyInstallment = loan.MonthlyInstallmentValue;
            loan.InterestRate = newInterestRate;

            await _loanRepository.UpdateAsync(loan);

            var existingInstallments = (await _installmentRepository.GetByConditionAsync(i => i.LoanId == loanId)).ToList();
            var futureInstallments = existingInstallments.Where(i => i.PaymentDate.Date > DateTime.Today && !i.IsPaid).ToList();

            if (!futureInstallments.Any() && existingInstallments.Any(i => !i.IsPaid))
            {
                futureInstallments = existingInstallments.Where(i => !i.IsPaid).ToList();
            }

            if (futureInstallments.Any())
            {
                decimal remainingLoanBalance = futureInstallments.First().RemainingBalance + futureInstallments.First().PrincipalAmount;

                if (existingInstallments.Any(i => i.IsPaid))
                {
                    var lastPaidInstallment = existingInstallments.Where(i => i.IsPaid).OrderByDescending(i => i.InstallmentNumber).FirstOrDefault();
                    if (lastPaidInstallment != null)
                    {
                        remainingLoanBalance = lastPaidInstallment.RemainingBalance;
                    }
                }

                int remainingTerm = futureInstallments.Count();
                DateTime nextPaymentDate = futureInstallments.First().PaymentDate;

                var recalculatedInstallments = CalculateAmortizationSchedule(
                    remainingLoanBalance, newInterestRate, remainingTerm, nextPaymentDate.AddMonths(-1));

                int currentInstallmentNumber = futureInstallments.First().InstallmentNumber;
                foreach (var recInst in recalculatedInstallments)
                {
                    var existing = futureInstallments.FirstOrDefault(i => i.InstallmentNumber == currentInstallmentNumber);
                    if (existing != null)
                    {
                        existing.InstallmentValue = recInst.InstallmentValue;
                        existing.PrincipalAmount = recInst.PrincipalAmount;
                        existing.InterestAmount = recInst.InterestAmount;
                        existing.RemainingBalance = recInst.RemainingBalance;
                        await _installmentRepository.UpdateAsync(existing);
                    }
                    currentInstallmentNumber++;
                }

                loan.MonthlyInstallmentValue = recalculatedInstallments.First().InstallmentValue;
                await _loanRepository.UpdateAsync(loan);

                var client = await _accountServiceForWebApp.GetUserById(loan.ClientId);
                if (client != null && client.Email != null)
                {
                    await _emailService.SendLoanInterestRateUpdatedEmail(
                        client.Email, newInterestRate, loan.MonthlyInstallmentValue, oldMonthlyInstallment);
                }
            }
        }

        public async Task<decimal> CalculateAverageClientDebtAsync()
        {
            var allLoans = await _loanRepository.GetAll();
            if (!allLoans.Any())
            {
                return 0;
            }

            return allLoans.Sum(l => l.PendingAmount) / allLoans.Count();
        }

        public async Task<bool> HasActiveLoanAsync(string clientId)
        {
            var clientLoans = await _loanRepository.GetByConditionAsync(l => l.ClientId == clientId && l.IsActive);
            return clientLoans.Any();
        }

        public async Task<List<AmortizationInstallmentDTO>> GetAmortizationTableByLoanIdAsync(int loanId)
        {
            var installments = await _installmentRepository.GetByConditionAsync(i => i.LoanId == loanId);
            return _mapper.Map<List<AmortizationInstallmentDTO>>(installments.OrderBy(i => i.InstallmentNumber).ToList());
        }

        public async Task CheckOverdueInstallmentsAsync()
        {
            var unpaidInstallments = await _installmentRepository.GetByConditionAsync(i => !i.IsPaid);

            foreach (var installment in unpaidInstallments)
            {
                if (installment.PaymentDate.Date < DateTime.Today && !installment.IsPaid)
                {
                    installment.IsOverdue = true;
                    await _installmentRepository.UpdateAsync(installment);

                    var loan = await _loanRepository.GetById(installment.LoanId);
                    if (loan != null && !loan.IsOverdue)
                    {
                        loan.IsOverdue = true;
                        await _loanRepository.UpdateAsync(loan);
                    }
                }
                else if (installment.IsPaid && installment.IsOverdue)
                {
                    installment.IsOverdue = false;
                    await _installmentRepository.UpdateAsync(installment);

                    var remainingUnpaidInstallments = await _installmentRepository
                        .GetByConditionAsync(i => i.LoanId == installment.LoanId && !i.IsPaid);

                    if (!remainingUnpaidInstallments.Any(i => i.PaymentDate.Date < DateTime.Today))
                    {
                        var loan = await _loanRepository.GetById(installment.LoanId);
                        if (loan != null && loan.IsOverdue)
                        {
                            loan.IsOverdue = false;
                            await _loanRepository.UpdateAsync(loan);
                        }
                    }
                }
            }
        }

        public async Task<decimal> CalculateLoanTotalInterestAmount(decimal loanAmount, decimal annualInterestRate, int loanTermMonths)
        {
            var schedule = CalculateAmortizationSchedule(loanAmount, annualInterestRate, loanTermMonths, DateTime.Now);
            return schedule.Sum(i => i.InterestAmount);
        }

        public List<AmortizationInstallmentDTO> CalculateAmortizationSchedule(
            decimal loanAmount, decimal annualInterestRate, int loanTermMonths, DateTime startDate)
        {
            var installments = new List<AmortizationInstallmentDTO>();
            decimal monthlyInterestRate = annualInterestRate / 100 / 12;

            decimal monthlyInstallment = 0;
            if (monthlyInterestRate == 0)
            {
                monthlyInstallment = loanAmount / loanTermMonths;
            }
            else
            {
                monthlyInstallment = loanAmount * monthlyInterestRate * (decimal)Math.Pow(1 + (double)monthlyInterestRate, loanTermMonths) /
                                     ((decimal)Math.Pow(1 + (double)monthlyInterestRate, loanTermMonths) - 1);
            }

            decimal remainingBalance = loanAmount;
            for (int i = 1; i <= loanTermMonths; i++)
            {
                decimal interestForPeriod = remainingBalance * monthlyInterestRate;
                decimal principalAmortized = monthlyInstallment - interestForPeriod;

                if (i == loanTermMonths)
                {
                    principalAmortized = remainingBalance;
                    monthlyInstallment = principalAmortized + interestForPeriod;
                    remainingBalance = 0;
                }
                else
                {
                    remainingBalance -= principalAmortized;
                }

                installments.Add(new AmortizationInstallmentDTO
                {
                    InstallmentNumber = i,
                    PaymentDate = startDate.AddMonths(i),
                    InstallmentValue = monthlyInstallment,
                    PrincipalAmount = principalAmortized,
                    InterestAmount = interestForPeriod,
                    RemainingBalance = remainingBalance,
                    IsPaid = false,
                    IsOverdue = false
                });
            }
            return installments;
        }

        public async Task<decimal> GetCurrentDebtForClient(string clientId)
        {
            var loans = await _loanRepository.GetByConditionAsync(l => l.ClientId == clientId && l.IsActive);
            decimal totalDeuda = 0;

            foreach (var loan in loans)
            {
                var cuotasPendientes = await _installmentRepository
                    .GetByConditionAsync(c => c.LoanId == loan.Id && !c.IsPaid);

                totalDeuda += cuotasPendientes.Sum(c => c.InstallmentValue);
            }

            return totalDeuda;
        }
    }
}