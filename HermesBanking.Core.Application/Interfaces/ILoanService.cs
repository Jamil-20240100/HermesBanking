using HermesBanking.Core.Application.DTOs.Loan;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface ILoanService
    {
        Task<List<LoanDTO>> GetAllLoansAsync(string? cedula, string? status);
        Task<LoanDTO> GetLoanByIdAsync(int loanId);
        Task AddLoanAsync(CreateLoanDTO loanDto, string adminId, string adminFullName);
        Task UpdateLoanInterestRateAsync(int loanId, decimal newInterestRate);
        Task<decimal> CalculateAverageClientDebtAsync();
        Task<bool> HasActiveLoanAsync(string clientId);
        Task<List<AmortizationInstallmentDTO>> GetAmortizationTableByLoanIdAsync(int loanId);
        List<AmortizationInstallmentDTO> CalculateAmortizationSchedule(
            decimal loanAmount, decimal annualInterestRate, int loanTermMonths, DateTime startDate);
        Task CheckOverdueInstallmentsAsync();
        Task<decimal> CalculateLoanTotalInterestAmount(decimal loanAmount, decimal annualInterestRate, int loanTermMonths);
    }
}