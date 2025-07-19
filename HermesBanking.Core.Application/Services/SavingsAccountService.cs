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
            foreach (var dto in returnDTOsList)
            {
                var user = clientDTOsList.FirstOrDefault(u => u.Id == dto.ClientId);
                var admin = adminDTOsList.FirstOrDefault(u => u.Id == dto.CreatedByAdminId);
                if (user != null)
                {
                    dto.ClientFullName = $"{user.Name} {user.LastName}";
                    dto.ClientUserId = $"{user.UserId}";
                    dto.AdminFullName = $"{admin?.Name} {admin?.LastName}";
                }
            }
            return returnDTOsList;
        }

        public async Task<string> GenerateUniqueAccountNumberAsync()
        {
            const int maxAttempts = 9999;
            var random = new Random();

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                string candidate = random.Next(100_000_000, 1_000_000_000).ToString();

                var existingAccount = await _repository.GetByAccountNumberAsync(candidate);
                if (existingAccount == null)
                {
                    return candidate;
                }
            }

            throw new Exception("No se pudo generar un número de cuenta único luego de varios intentos.");
        }

        public async Task CancelAsync(int id)
        {
            var account = await _repository.GetById(id);
            if (account == null) throw new Exception("La cuenta no existe.");

            account.IsActive = false;

            await _repository.UpdateAsync(account.Id, account);
        }

        public async Task TransferBalanceAndCancelAsync(int accountId)
        {
            var account = await _repository.GetById(accountId);
            if (account == null || account.AccountType != AccountType.Secondary)
                throw new Exception("Cuenta no válida para transferencia.");

            if (!account.IsActive)
                throw new Exception("La cuenta ya está inactiva.");

            if (account.Balance > 0)
            {
                var allAccounts = await _repository.GetAll();

                var primaryAccount = allAccounts
                    .FirstOrDefault(a =>
                        a.ClientId == account.ClientId &&
                        a.AccountType == AccountType.Primary &&
                        a.IsActive);

                if (primaryAccount == null)
                    throw new Exception("Cuenta principal activa no encontrada.");

                primaryAccount.Balance += account.Balance;
                account.Balance = 0;

                await _repository.UpdateAsync(primaryAccount.Id, primaryAccount);
            }

            account.IsActive = false;
            await _repository.UpdateAsync(account.Id, account);
        }

        public async Task<SavingsAccountDTO?> GetByAccountNumberAsync(string beneficiaryAccountNumber)
        {
            var clientDTOsList = await _accountServiceForWebApp.GetAllUserByRole(Roles.Client.ToString());
            var adminDTOsList = await _accountServiceForWebApp.GetAllUserByRole(Roles.Admin.ToString());

            var clientIds = clientDTOsList.Select(x => x.Id).ToList();
            var adminIds = adminDTOsList.Select(x => x.Id).ToList();

            var savingsAccountsList = await _repository.GetAll();

            var clientAccounts = savingsAccountsList
                .Where(sa =>
                    (clientIds.Contains(sa.ClientId) || adminIds.Contains(sa.CreatedByAdminId ?? "")) &&
                    sa.AccountType == AccountType.Primary &&
                    sa.AccountNumber == beneficiaryAccountNumber)
                .ToList();

            var targetAccount = clientAccounts.FirstOrDefault();
            if (targetAccount == null)
                return null;

            var dto = _mapper.Map<SavingsAccountDTO>(targetAccount);

            var user = clientDTOsList.FirstOrDefault(u => u.Id == dto.ClientId);
            var admin = adminDTOsList.FirstOrDefault(u => u.Id == dto.CreatedByAdminId);

            if (user != null)
            {
                dto.ClientFullName = $"{user.Name} {user.LastName}";
                dto.ClientUserId = user.UserId;
                dto.AdminFullName = admin != null ? $"{admin.Name} {admin.LastName}" : null;
            }

            return dto;
        }

        public async Task<IEnumerable<SavingsAccount>> GetAllActiveAccounts()
        {
            // Obtener todas las cuentas de ahorro
            var allAccounts = await _repository.GetAll();

            // Filtrar solo las cuentas activas
            var activeAccounts = allAccounts.Where(account => account.IsActive).ToList();

            return activeAccounts;
        }


    }
}
