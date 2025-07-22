using AutoMapper;
using HermesBanking.Core.Application.DTOs.CreditCard;
using HermesBanking.Core.Application.DTOs.Loan;
using HermesBanking.Core.Application.DTOs.SavingsAccount;
using HermesBanking.Core.Application.DTOs.User;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.ViewModels.Loan;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HermesBanking.Infrastructure.Identity.Services
{
    public class AccountServiceForWebApp : BaseAccountService, IAccountServiceForWebApp
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IMapper _mapper;
        private readonly ISavingsAccountRepository _savingsAccountRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly IEmailService _emailService;

        public AccountServiceForWebApp(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IEmailService emailService,
            IMapper mapper,
            ISavingsAccountRepository savingsAccountRepository,
            ILoanRepository loanRepository,
            ICreditCardRepository creditCardRepository)
            : base(userManager, emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _savingsAccountRepository = savingsAccountRepository;
            _loanRepository = loanRepository;
            _creditCardRepository = creditCardRepository;
            _emailService = emailService;
        }

        // Authenticate a user

        public async Task<List<SavingsAccountDTO>> GetSavingsAccountsByUserIdAsync(string userId)
        {
            // Assuming your repository has a method like GetAccountsByClientId or GetByConditionAsync
            // You might need to adjust based on how ClientId relates to UserId in your domain.
            var accounts = await _savingsAccountRepository.GetByConditionAsync(sa => sa.ClientId == userId && sa.IsActive);
            return _mapper.Map<List<SavingsAccountDTO>>(accounts);
        }

        public async Task<List<CreditCardDTO>> GetCreditCardsByUserIdAsync(string userId)
        {
            var creditCards = await _creditCardRepository.GetByConditionAsync(cc => cc.ClientId == userId && cc.IsActive);
            return _mapper.Map<List<CreditCardDTO>>(creditCards);
        }

        public async Task<List<LoanDTO>> GetLoansByUserIdAsync(string userId)
        {
            var loans = await _loanRepository.GetByConditionAsync(l => l.ClientId == userId && l.IsActive);
            return _mapper.Map<List<LoanDTO>>(loans);
        }
        public async Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto)
        {
            var response = new LoginResponseDto
            {
                Email = "",
                Id = "",
                LastName = "",
                Name = "",
                UserName = "",
                HasError = false,
                Errors = new List<string>()
            };

            var user = await _userManager.FindByNameAsync(loginDto.UserName);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"There is no account registered with this username: {loginDto.UserName}");
                return response;
            }

            if (!user.EmailConfirmed)
            {
                response.HasError = true;
                response.Errors.Add($"This account {loginDto.UserName} is not active, you should check your email.");
                return response;
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName ?? "", loginDto.Password, false, true);

            if (!result.Succeeded)
            {
                response.HasError = true;
                if (result.IsLockedOut)
                {
                    response.Errors.Add($"Your account {loginDto.UserName} has been locked due to multiple failed attempts." +
                                         " Please try again in 10 minutes. If you don’t remember your password, you can go through the password reset process.");
                }
                else
                {
                    response.Errors.Add($"These credentials are invalid for this user: {user.UserName}");
                }
                return response;
            }

            var rolesList = await _userManager.GetRolesAsync(user);

            response.Id = user.Id;
            response.Email = user.Email ?? "";
            response.UserName = user.UserName ?? "";
            response.Name = user.Name;
            response.LastName = user.LastName;
            response.IsVerified = user.EmailConfirmed;
            response.Roles = rolesList.ToList();

            // Send a success email notification after login
            //await _emailService.SendEmailAsync(user.Email, "Login Successful", "You have successfully logged in.");

            return response;
        }

        // Sign out a user
        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        // Get user by ID
        public async Task<UserDto?> GetUserById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                UserId = user.Id,
                UserName = user.UserName ?? "Unknown",
                Name = user.Name ?? "Unknown",
                LastName = user.LastName ?? "Unknown",
                Email = user.Email ?? "Unknown",
                Role = roles.FirstOrDefault() ?? "Unknown",
                IsActive = user.IsActive,
                isVerified = user.EmailConfirmed,
                InitialAmount = null
            };
        }

        // Get user by Username
        public async Task<AppUser?> GetUserByUserName(string userName)
        {
            return await _userManager.FindByNameAsync(userName);
        }

        // Get user's email by user ID
        public async Task<string?> GetUserEmailAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.Email;
        }

        // Get user's full name by user ID
        public async Task<string> GetUserFullNameAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user == null ? "" : $"{user.Name} {user.LastName}";
        }

        // Get client details for loan assignment
        public async Task<List<ClientSelectionViewModel>> GetClientDetailsForLoanAssignment(string? identificationNumber)
        {
            var clients = await _userManager.GetUsersInRoleAsync("Client");

            if (!string.IsNullOrWhiteSpace(identificationNumber))
            {
                clients = clients.Where(u => u.UserId != null && u.UserId.Contains(identificationNumber)).ToList();
            }

            return clients.Select(c => new ClientSelectionViewModel
            {
                Id = c.Id,
                IdentificationNumber = c.UserId ?? "N/A",
                Name = c.Name,
                LastName = c.LastName,
                Email = c.Email,
                CurrentDebtAmount = 0,
                IsSelected = false
            }).ToList();
        }

        // Get user by identification number
        public async Task<UserDto?> GetUserByIdentificationNumber(string identificationNumber)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserId == identificationNumber);
            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                UserId = user.Id,
                UserName = user.UserName ?? "",
                Name = user.Name ?? "",
                LastName = user.LastName ?? "",
                Email = user.Email ?? "",
                Role = roles.FirstOrDefault() ?? "Unknown",
                IsActive = user.IsActive,
                isVerified = user.EmailConfirmed,
                InitialAmount = null
            };
        }

        // Update savings account balance
        public async Task UpdateSavingsAccountBalance(string clientId, decimal amount)
        {
            var client = await _userManager.FindByIdAsync(clientId);
            if (client == null)
            {
                throw new InvalidOperationException($"Client with ID {clientId} not found.");
            }

            // Simulate the update of the account balance for the client
            Console.WriteLine($"Simulating balance update for client {client.UserName}: {amount:C} added.");
            await Task.CompletedTask;
        }

        // Get active accounts for a client
        public async Task<IEnumerable<SavingsAccount>> GetActiveAccountsAsync(string clientId)
        {
            var accounts = await _savingsAccountRepository.GetAccountsByClientIdAsync(clientId);
            var activeAccounts = accounts.Where(account => account.IsActive).ToList();
            Console.WriteLine($"Active Accounts for Client {clientId}: {string.Join(", ", activeAccounts.Select(a => a.AccountNumber))}");
            return activeAccounts;
        }

        // Get credit card by number
        public async Task<CreditCard?> GetCreditCardByNumberAsync(string cardNumber)
        {
            return await _creditCardRepository.GetByCardNumberAsync(cardNumber);
        }

        // Get savings account by number
        public async Task<SavingsAccount?> GetSavingsAccountByNumberAsync(string accountNumber)
        {
            return await _savingsAccountRepository.GetByAccountNumberAsync(accountNumber);
        }

        // Get loan information by identifier
        public async Task<Loan?> GetLoanInfoAsync(string loanIdentifier)
        {
            return await _loanRepository.GetLoanByIdentifierAsync(loanIdentifier);
        }

        // Get user email by client ID
        public async Task<string> GetUserEmailByClientIdAsync(string clientId)
        {
            var user = await _userManager.FindByIdAsync(clientId);
            return user?.Email ?? string.Empty;
        }

        // Update savings account details
        public async Task UpdateAsync(SavingsAccount account)
        {
            await _savingsAccountRepository.UpdateAsync(account.Id, account);
        }

        public async Task<string?> GetSavingsAccountHolderFullNameAsync(string accountNumber)
        {
            var savingsAccount = await _savingsAccountRepository.GetByAccountNumberAsync(accountNumber);
            if (savingsAccount == null)
            {
                return null;
            }
            var user = await _userManager.FindByIdAsync(savingsAccount.ClientId);
            return user == null ? null : $"{user.Name} {user.LastName}";
        }

        public async Task<string?> GetSavingsAccountHolderEmailAsync(string accountNumber)
        {
            var savingsAccount = await _savingsAccountRepository.GetByAccountNumberAsync(accountNumber);
            if (savingsAccount == null)
            {
                return null;
            }
            var user = await _userManager.FindByIdAsync(savingsAccount.ClientId);
            return user?.Email;
        }

    }
}
