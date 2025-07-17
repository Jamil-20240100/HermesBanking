using HermesBanking.Core.Application.DTOs.Beneficiary;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface IBeneficiaryService : IGenericService<BeneficiaryDTO>
    {
        Task<List<BeneficiaryDTO>> GetAllByClientIdAsync(string clientId);
    }
}