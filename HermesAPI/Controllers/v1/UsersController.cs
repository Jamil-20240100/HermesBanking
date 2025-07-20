using Asp.Versioning;
using HermesBanking.Core.Application.DTOs.User;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Common.Enums;
using HermesBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HermesAPI.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IAccountServiceForWebApi _accountServiceForWebApi;
        private readonly UserManager<AppUser> _userManager;
        private readonly ISavingsAccountService _savingsAccountService;

        public UsersController(IAccountServiceForWebApi accountService, UserManager<AppUser> user, ISavingsAccountService savingsAccountService)
        {
            _accountServiceForWebApi = accountService;
            _userManager = user;
            _savingsAccountService = savingsAccountService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? rol = null)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var pagedUsers = await _accountServiceForWebApi.GetPagedUsersAsync(page, pageSize, rol);

            return Ok(pagedUsers);
        }

        [HttpGet("commerce")]
        public async Task<IActionResult> GetCommerceUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? rol = null)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var pagedUsers = await _accountServiceForWebApi.GetPagedCommerceUsersAsync(page, pageSize, rol);

            return Ok(pagedUsers);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, errorMessage) = await _accountServiceForWebApi.CreateUserAsync(request);

            if (!success)
            {
                if (errorMessage.Contains("registrado"))
                    return Conflict(new { message = errorMessage });

                return BadRequest(new { message = errorMessage });
            }

            return Created("", new { message = "Usuario creado exitosamente." });
        }

        [HttpPost("commerce/{commerceId}")]
        public async Task<IActionResult> CreateCommerceUser([FromRoute] string commerceId, [FromBody] SaveUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var alreadyHasUser = await _accountServiceForWebApi.CommerceHasUserAsync(commerceId);
            if (alreadyHasUser)
                return BadRequest($"El comercio con ID {commerceId} ya tiene un usuario asignado.");

            var userExists = await _userManager.FindByNameAsync(dto.UserName) != null;
            var emailExists = await _userManager.FindByEmailAsync(dto.Email) != null;

            if (userExists || emailExists)
            {
                return Conflict(new
                {
                    Errors = new List<string?>
            {
                userExists ? "El nombre de usuario ya está registrado." : null,
                emailExists ? "El correo ya está registrado." : null
            }.Where(e => e != null)
                });
            }

            dto.Role = Roles.Commerce.ToString();
            dto.CommerceId = commerceId;

            var result = await _accountServiceForWebApi.CreateCommerceUserAsync(dto);

            if (result.HasError)
                return BadRequest(new { Errors = result.Errors });

            return Created("", null);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser([FromRoute] string id, [FromBody] SaveUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var emailExists = await _userManager.Users
                .AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower() && u.Id != id);

            if (emailExists)
                return Conflict(new { Errors = new[] { "El correo ya está en uso por otro usuario." } });

            var userNameExists = await _userManager.Users
                .AnyAsync(u => u.UserName.ToLower() == dto.UserName.ToLower() && u.Id != id);

            if (userNameExists)
                return Conflict(new { Errors = new[] { "El nombre de usuario ya está en uso por otro usuario." } });

            var currentRoles = await _userManager.GetRolesAsync(user);
            user.Name = dto.Name;
            user.LastName = dto.LastName;
            user.UserId = dto.UserId;
            user.Email = dto.Email;
            user.UserName = dto.UserName;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(e => e.Description);
                return BadRequest(new { Errors = errors });
            }

            if (currentRoles.Contains(Roles.Client.ToString()) && dto.AdditionalAmount > 0)
            {
                var accounts = await _savingsAccountService.GetAll();
                var principalAccount = accounts
                    .FirstOrDefault(a => a.ClientId == user.Id && a.AccountType == AccountType.Primary);

                if (principalAccount != null)
                {
                    principalAccount.Balance += dto.AdditionalAmount;

                    await _savingsAccountService.UpdateAsync(principalAccount, principalAccount.Id);
                }
            }

            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeUserStatus(string id, [FromBody] ChangeUserStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == id)
                return Forbid("No puedes cambiar tu propio estado.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound("Usuario no encontrado.");

            user.IsActive = dto.Status;
            await _userManager.UpdateAsync(user);

            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound("Usuario no encontrado.");

            var clientAccounts = await _savingsAccountService.GetAllSavingsAccountsOfClients();

            var mainAccount = clientAccounts.FirstOrDefault(a =>
                a.ClientId == user.Id && a.AccountType == AccountType.Primary && a.IsActive);

            var result = new
            {
                usuario = user.UserName,
                nombre = user.Name,
                apellido = user.LastName,
                cedula = user.UserId,
                correo = user.Email,
                estado = user.IsActive ? "activo" : "inactivo",
                cuentaPrincipal = mainAccount == null ? null : new
                {
                    numeroCuenta = mainAccount.AccountNumber,
                    balance = mainAccount.Balance
                }
            };

            return Ok(result);
        }
    }
}
