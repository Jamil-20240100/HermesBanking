using HermesBanking.Core.Application.DTOs.SavingsAccount;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HermesAPI.Controllers.v1
{
    [ApiController]
    [Route("api/savings-account")]
    [Authorize(Roles = "Admin")]
    public class SavingsAccountController : ControllerBase
    {
        private readonly ISavingsAccountService _savingsAccountService;
        private readonly IAccountServiceForWebApp _accountService;

        public SavingsAccountController(
            ISavingsAccountService savingsAccountService,
            IAccountServiceForWebApp accountService)
        {
            _savingsAccountService = savingsAccountService;
            _accountService = accountService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? cedula,
            [FromQuery] string? estado,
            [FromQuery] string? tipo,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 10)
        {
            var cuentas = await _savingsAccountService.GetAllSavingsAccountsOfClients();

            if (!string.IsNullOrEmpty(cedula))
            {
                var user = await _accountService.GetUserByIdentificationNumber(cedula);
                if (user != null)
                {
                    cuentas = cuentas.Where(c => c.ClientUserId == user.Id).ToList();
                }
                else
                {
                    cuentas = [];
                }
            }

            if (!string.IsNullOrEmpty(estado))
            {
                bool isActive = estado.ToLower() == "activa";
                cuentas = cuentas.Where(c => c.IsActive == isActive).ToList();
            }

            if (!string.IsNullOrEmpty(tipo))
            {
                tipo = tipo.ToLower();
                if (tipo == "principal")
                    cuentas = cuentas.Where(c => c.AccountType == AccountType.Primary).ToList();
                else if (tipo == "secundaria")
                    cuentas = cuentas.Where(c => c.AccountType == AccountType.Secondary).ToList();
            }

            int totalRegistros = cuentas.Count;
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanoPagina);

            var resultadoPaginado = cuentas
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .Select(c => new
                {
                    numeroCuenta = c.AccountNumber,
                    nombreCliente = c.ClientFullName?.Split(" ").FirstOrDefault(),
                    apellidoCliente = c.ClientFullName?.Split(" ").Skip(1).FirstOrDefault(),
                    balance = c.Balance,
                    tipoCuenta = c.AccountType.ToString().ToLower() == "primary" ? "principal" : "secundaria",
                    estado = c.IsActive ? "activa" : "cancelada"
                })
                .ToList();

            var response = new
            {
                data = resultadoPaginado,
                paginacion = new
                {
                    paginaActual = pagina,
                    totalPaginas,
                    totalRegistros
                }
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSecondaryAccount([FromBody] CreateSecondarySavingsAccountRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.CedulaCliente) || request.BalanceInicial < 0)
                return BadRequest("Datos inválidos o campos faltantes.");

            var user = await _accountService.GetUserByIdentificationNumber(request.CedulaCliente);
            if (user == null)
                return BadRequest("Cliente no encontrado.");

            string accountNumber;
            try
            {
                accountNumber = await _savingsAccountService.GenerateUniqueAccountNumberAsync();
            }
            catch
            {
                return Conflict("Número de cuenta no disponible.");
            }

            var nuevaCuenta = new SavingsAccountDTO
            {
                AccountNumber = accountNumber,
                AccountType = AccountType.Secondary,
                Balance = request.BalanceInicial,
                ClientId = user.Id,
                IsActive = true,
                ClientFullName = $"{user.Name} {user.LastName}",
                CreatedAt = DateTime.UtcNow,
                CreatedByAdminId = ""
            };

            var creada = await _savingsAccountService.AddAsync(nuevaCuenta);
            if (creada == null)
                return BadRequest("No se pudo crear la cuenta.");

            return Created(string.Empty, new
            {
                mensaje = "Cuenta secundaria asignada correctamente.",
                numeroCuenta = creada.AccountNumber
            });
        }
    }
}
