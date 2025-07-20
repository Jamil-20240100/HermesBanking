using HermesBanking.Core.Application.DTOs.Loan;
using HermesBanking.Core.Domain.Entities;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface ILoanService
    {
        Task<List<LoanDTO>> GetAllLoansAsync(string? cedula, string? status);
        Task<LoanDetailDTO?> GetLoanDetailWithAmortizationAsync(string loanId);
        Task<object> GetAllLoansAsync(string? cedula, string? status, int page = 1, int pageSize = 10);
        Task<LoanDTO> GetLoanByIdAsync(int loanId);
        Task<LoanDTO?> GetLoanByIdentifierAsync(string loanIdentifier);
        Task AddLoanAsync(CreateLoanDTO loanDto, string adminId, string adminFullName);
        Task<(int StatusCode, string? Error)> CreateLoanForClientAsync(CreateLoanDTO dto, string adminId, string adminFullName);
        Task UpdateLoanInterestRateAsync(int loanId, decimal newInterestRate);
        Task<decimal> CalculateAverageClientDebtAsync();
        Task<bool> HasActiveLoanAsync(string clientId);
        Task<List<AmortizationInstallmentDTO>> GetAmortizationTableByLoanIdAsync(int loanId);
        List<AmortizationInstallmentDTO> CalculateAmortizationSchedule(decimal loanAmount, decimal annualInterestRate, int loanTermMonths, DateTime startDate);
        Task CheckOverdueInstallmentsAsync();
        Task<decimal> CalculateLoanTotalInterestAmount(decimal loanAmount, decimal annualInterestRate, int loanTermMonths);
        Task<decimal> GetCurrentDebtForClient(string clientId);
        Task<decimal> GetClientTotalDebt(string clientId);
        Task<decimal> GetAverageSystemDebt();
    }
}