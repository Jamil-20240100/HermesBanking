using AutoMapper;
using HermesBanking.Core.Application.DTOs.Beneficiary;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;

namespace HermesBanking.Core.Application.Services
{
    public class BeneficiaryService : GenericService<Beneficiary, BeneficiaryDTO>, IBeneficiaryService
    {
        private readonly IBeneficiaryRepository _repository;
        private readonly IMapper _mapper;

        public BeneficiaryService(IBeneficiaryRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<BeneficiaryDTO>> GetBeneficiariesByUserIdAsync(string userId)
        {
            // Assuming your beneficiary entity has a UserId property or can be filtered by client ID
            var beneficiaries = await _repository.GetByConditionAsync(b => b.ClientId == userId); // Adjust ClientId to match your User/Client relationship
            return _mapper.Map<List<BeneficiaryDTO>>(beneficiaries);
        }

        public async Task<IEnumerable<Beneficiary>> GetAvailableBeneficiariesAsync(string clientId)
        {
            return await _repository.GetAll();  // Devuelve todos los beneficiarios, puedes agregar algún filtro si es necesario
        }

        public async Task<List<BeneficiaryDTO>> GetAllByClientIdAsync(string clientId)
        {
            var allBeneficiaries = await _repository.GetAll();
            var clientBeneficiaries = allBeneficiaries.Where(b => b.ClientId == clientId).ToList();
            return _mapper.Map<List<BeneficiaryDTO>>(clientBeneficiaries);
        }
    }
}