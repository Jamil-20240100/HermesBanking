using HermesBanking.Core.Application.DTOs.Transfer;
using HermesBanking.Core.Application.ViewModels.Transfer;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface ITransferService
    {
        Task<TransferViewModel> ProcessTransferAsync(TransferDTO dto);
    }
}