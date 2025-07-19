using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HermesBanking.Infrastructure.Persistence.Repositories
{
    public class BeneficiaryRepository : GenericRepository<Beneficiary>, IBeneficiaryRepository
    {
        public BeneficiaryRepository(HermesBankingContext context) : base(context)
        {
        }

        // Método que utiliza GetByConditionAsync para obtener beneficiarios por ClientId
        public async Task<List<Beneficiary>> GetBeneficiariesByClientIdAsync(string clientId)
        {
            return (List<Beneficiary>)await GetByConditionAsync(b => b.ClientId == clientId);
        }


    }
}
