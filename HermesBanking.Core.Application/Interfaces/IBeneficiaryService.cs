using HermesBanking.Core.Application.DTOs.Beneficiary;
using HermesBanking.Core.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface IBeneficiaryService : IGenericService<BeneficiaryDTO>
    {
        Task<List<BeneficiaryDTO>> GetBeneficiariesByUserIdAsync(string userId);

        Task<List<BeneficiaryDTO>> GetAllByClientIdAsync(string clientId);
        Task<IEnumerable<Beneficiary>> GetAvailableBeneficiariesAsync(string clientId);
    }
}