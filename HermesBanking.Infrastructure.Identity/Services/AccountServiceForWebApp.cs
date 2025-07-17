using AutoMapper; 
using HermesBanking.Core.Application.DTOs.User;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.ViewModels.Loan;
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
        public AccountServiceForWebApp(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IEmailService emailService,
            IMapper mapper)
            : base(userManager, emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
        }

        public async Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto)
        {
            LoginResponseDto response = new()
            {
                Email = "",
                Id = "",
                LastName = "",
                Name = "",
                UserName = "",
                HasError = false,
                Errors = []
            };

            var user = await _userManager.FindByNameAsync(loginDto.UserName);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"There is no acccount registered with this username: {loginDto.UserName}");
                return response;
            }

            if (!user.EmailConfirmed)
            {
                response.HasError = true;
                response.Errors.Add($"This account {loginDto.UserName} is not active, you should check your email");
                return response;
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName ?? "", loginDto.Password, false, true);

            if (!result.Succeeded)
            {
                response.HasError = true;
                if (result.IsLockedOut)
                {
                    response.Errors.Add($"Your account {loginDto.UserName} has been locked due to multiple failed attempts." +
                                        $" Please try again in 10 minutes. If you don’t remember your password, you can go through the password " +
                                        $"reset process.");
                }
                else
                {
                    response.Errors.Add($"these credentials are invalid for this user: {user.UserName}");
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

            return response;
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<UserDto?> GetUserById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return null;

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

        public async Task<AppUser?> GetUserByUserName(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            return user;
        }

        public async Task<string?> GetUserEmailAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.Email;
        }

        public async Task<string> GetUserFullNameAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user == null ? "" : $"{user.Name} {user.LastName}";
        }

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

        public async Task<UserDto?> GetUserByIdentificationNumber(string identificationNumber)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserId == identificationNumber);
            if(user == null)
            {
                return null;
            }
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

        public async Task UpdateSavingsAccountBalance(string clientId, decimal amount)
        {
            var client = await _userManager.FindByIdAsync(clientId);
            if (client == null)
            {
                throw new InvalidOperationException($"Cliente con ID {clientId} no encontrado.");
            }

           
            Console.WriteLine($"Simulando actualización de balance para cliente {client.UserName}: Se añadieron {amount:C}");
            await Task.CompletedTask;
        }

        public async Task<string> GetUserEmailByClientIdAsync(string clientId)
        {
            var user = await _userManager.FindByIdAsync(clientId); // Asumiendo que usas UserManager para acceder a los datos del usuario
            return user?.Email ?? string.Empty; // Devuelve el email o una cadena vacía si no se encuentra
        }

    }
}