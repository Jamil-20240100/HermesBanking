using HermesBanking.Core.Application.DTOs.CashAdvance;
using HermesBanking.Core.Application.ViewModels.CashAdvance;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface ICashAdvanceService
    {
        Task<CashAdvanceViewModel> ProcessCashAdvanceAsync(CashAdvanceDTO dto);
    }
}