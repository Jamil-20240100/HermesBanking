using AutoMapper;
using HermesBanking.Core.Application.DTOs.Loan;
using HermesBanking.Core.Application.DTOs.SavingsAccount;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Common.Enums;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        public async Task<object> GetAllLoansAsync(string? cedula, string? status, int page = 1, int pageSize = 10)
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
                    return new
                    {
                        data = new List<object>(),
                        paginacion = new { paginaActual = page, totalPaginas = 0 }
                    };
                }
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.ToLower() == "activos")
                {
                    loans = loans.Where(l => l.IsActive).ToList();
                }
                else if (status.ToLower() == "completados")
                {
                    loans = loans.Where(l => !l.IsActive).ToList();
                }
            }

            loans = loans.OrderByDescending(l => l.IsActive)
                         .ThenByDescending(l => l.CreatedAt)
                         .ToList();

            int totalItems = loans.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            page = Math.Max(1, Math.Min(page, totalPages));
            var loansPage = loans.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var result = new List<object>();

            foreach (var loan in loansPage)
            {
                var client = await _accountServiceForWebApp.GetUserById(loan.ClientId);
                var cuotasPagadas = await _installmentRepository.GetByConditionAsync(i => i.LoanId == loan.Id && i.IsPaid);
                var cuotasPendientes = await _installmentRepository.GetByConditionAsync(i => i.LoanId == loan.Id && !i.IsPaid);

                string estadoPago = "al_dia";
                if (cuotasPendientes.Any(i => i.IsOverdue))
                {
                    estadoPago = "atrasado";
                }

                result.Add(new
                {
                    id = loan.Id.ToString("D9"),
                    cliente = $"{client?.Name} {client?.LastName}",
                    cedula = client?.UserId,
                    monto = loan.Amount,
                    cuotasTotales = loan.TotalInstallments,
                    cuotasPagadas = cuotasPagadas.Count(),
                    pendiente = cuotasPendientes.Sum(i => i.InstallmentValue),
                    interes = loan.InterestRate,
                    plazo = loan.LoanTermMonths,
                    estadoPago = estadoPago
                });
            }

            return new
            {
                data = result,
                paginacion = new
                {
                    paginaActual = page,
                    totalPaginas = totalPages
                }
            };
        }

        public async Task<LoanDetailDTO?> GetLoanDetailWithAmortizationAsync(string loanId)
        {
            var loan = (await _loanRepository.GetByConditionAsync(l => l.LoanIdentifier == loanId)).FirstOrDefault();
            if (loan == null) return null;

            var installments = await _installmentRepository.GetByConditionAsync(i => i.LoanId == loan.Id);

            var today = DateTime.Today;

            foreach (var installment in installments)
            {
                installment.IsOverdue = !installment.IsPaid && installment.PaymentDate.Date < today;
            }

            var installmentsDto = _mapper.Map<List<AmortizationInstallmentDTO>>(installments);

            return new LoanDetailDTO
            {
                PrestamoId = loan.LoanIdentifier,
                TablaAmortizacion = installmentsDto
                    .OrderBy(x => x.InstallmentNumber)
                    .ToList()
            };
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

        public async Task<(int StatusCode, string? Error)> CreateLoanForClientAsync(CreateLoanDTO dto, string adminId, string adminFullName)
        {
            if (string.IsNullOrWhiteSpace(dto.ClientId)
                || dto.Amount <= 0
                || dto.InterestRate <= 0
                || dto.LoanTermMonths <= 0)
            {
                return (400, "Todos los campos son requeridos y deben ser mayores que cero.");
            }

            if (await HasActiveLoanAsync(dto.ClientId))
            {
                return (400, "El cliente ya tiene un préstamo activo.");
            }

            var averageDebt = await CalculateAverageClientDebtAsync();
            var client = await _accountServiceForWebApp.GetUserById(dto.ClientId);
            if (client != null)
            {
                if (averageDebt < client.TotalDebt)
                {
                    return (409, "El cliente es de alto riesgo.");
                }
            }

            try
            {
                await AddLoanAsync(dto, adminId, adminFullName);
                return (201, null);
            }
            catch (InvalidOperationException ex)
            {
                return (400, ex.Message);
            }
            catch (Exception ex)
            {
                return (500, "Error inesperado: " + ex.Message);
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

        public async Task<LoanDTO?> GetLoanByIdentifierAsync(string loanIdentifier)
        {
            var entities = await _loanRepository.GetAllQuery().FirstOrDefaultAsync(l => l.LoanIdentifier == loanIdentifier);
            var dtos = _mapper.Map<LoanDTO>(entities);
            return dtos;
        }

    }
}