using AutoMapper;
using HermesBanking.Core.Application.DTOs.SavingsAccount;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Common.Enums;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;

namespace HermesBanking.Core.Application.Services
{
    public class SavingsAccountService : GenericService<SavingsAccount, SavingsAccountDTO>, ISavingsAccountService
    {
        private readonly ISavingsAccountRepository _repository;
        private readonly IAccountServiceForWebApp _accountServiceForWebApp;
        private readonly IMapper _mapper;

        public SavingsAccountService(ISavingsAccountRepository repository, IMapper mapper, IAccountServiceForWebApp accountServiceForWebApp) : base(repository, mapper)
        {
            _mapper = mapper;
            _repository = repository;
            _accountServiceForWebApp = accountServiceForWebApp;
        }

        public async Task<List<SavingsAccountDTO>> GetAllSavingsAccountsOfClients()
        {
            //get users
            var clientDTOsList = await _accountServiceForWebApp.GetAllUserByRole(Roles.Client.ToString());
            var adminDTOsList = await _accountServiceForWebApp.GetAllUserByRole(Roles.Admin.ToString());

            //get users' ids
            var clientIds = clientDTOsList.Select(x => x.Id).ToList();
            var adminIds = adminDTOsList.Select(x => x.Id).ToList(); 

            //get all savings accounts
            var savingsAccountsList = await _repository.GetAll();

            //get clients' accounts
            var clientsAccounts = savingsAccountsList
                .Where(sa => clientIds.Contains(sa.ClientId) || adminIds.Contains(sa.CreatedByAdminId ?? ""))
                .ToList();

            //map entities to dtos
            var returnDTOsList = _mapper.Map<List<SavingsAccountDTO>>(clientsAccounts);

            //add full names
            foreach(var dto in returnDTOsList)
            {
                var user = clientDTOsList.FirstOrDefault(u => u.Id == dto.ClientId);
                var admin = adminDTOsList.FirstOrDefault(u => u.Id == dto.CreatedByAdminId);
                if (user != null)
                    dto.ClientFullName = $"{user.Name} {user.LastName}";
                    dto.AdminFullName = $"{admin?.Name} {admin?.LastName}";
            }
            return returnDTOsList;
        }
    }
}
