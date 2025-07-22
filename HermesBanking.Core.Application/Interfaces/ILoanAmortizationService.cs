// HermesBanking.Core.Application.Interfaces/ILoanAmortizationService.cs
namespace HermesBanking.Core.Application.Interfaces
{
    public interface ILoanAmortizationService
    {
        /// <summary>
        /// Aplica un pago a un préstamo, distribuyendo el monto entre las cuotas pendientes.
        /// </summary>
        /// <param name="loanId">El ID del préstamo.</param>
        /// <param name="paymentAmount">El monto que se desea abonar.</param>
        /// <returns>El monto total que fue aplicado al préstamo.</returns>
        Task<decimal> ApplyPaymentToLoanAsync(int loanId, decimal paymentAmount);
    }
}