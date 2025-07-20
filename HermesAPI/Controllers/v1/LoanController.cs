using HermesBanking.Core.Application.DTOs.Loan;
using HermesBanking.Core.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HermesAPI.Controllers.v1
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoanController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLoans([FromQuery] int pagina = 1, [FromQuery] string? estado = null, [FromQuery] string? cedula = null)
        {
            var loans = await _loanService.GetAllLoansAsync(cedula, estado, pagina);
            return Ok(loans);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLoan([FromBody] CreateLoanDTO dto)
        {
            var adminId = User.FindFirstValue("uid")!;
            var adminFullName = $"{User.FindFirst("name")?.Value}";

            var (status, error) = await _loanService.CreateLoanForClientAsync(dto, adminId, adminFullName);

            return status switch
            {
                201 => StatusCode(201),
                400 => BadRequest(new { message = error }),
                409 => Conflict(new { message = error }),
                401 => Unauthorized(),
                403 => Forbid(),
                _ => StatusCode(500, new { message = error })
            };
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLoanDetail(string id)
        {
            var loanDetail = await _loanService.GetLoanDetailWithAmortizationAsync(id);

            if (loanDetail == null)
            {
                return NotFound(new { message = "Préstamo no encontrado." });
            }

            return Ok(loanDetail);
        }

        [HttpPatch("{id}/rate")]
        public async Task<IActionResult> UpdateLoanInterestRate(string id, [FromBody] UpdateInterestRateForApiDTO dto)
        {
            if (dto == null || dto.NuevaTasa <= 0)
            {
                return BadRequest(new { message = "Tasa no proporcionada o no válida." });
            }

            var loan = await _loanService.GetLoanByIdentifierAsync(id);
            if (loan == null)
            {
                return NotFound(new { message = "Préstamo no encontrado." });
            }

            try
            {
                await _loanService.UpdateLoanInterestRateAsync(loan.Id, dto.NuevaTasa);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}