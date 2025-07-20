using HermesBanking.Core.Application.DTOs.CreditCard;
using HermesBanking.Core.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HermesAPI.Controllers.v1
{
    [ApiController]
    [Route("api/credit-card")]
    [Authorize(Roles = "Admin")]
    public class CreditCardController : ControllerBase
    {
        private readonly ICreditCardService _creditCardService;

        public CreditCardController(ICreditCardService creditCardService)
        {
            _creditCardService = creditCardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCreditCards(
            [FromQuery] string? cedula,
            [FromQuery] string? estado,
            [FromQuery] int pagina = 1)
        {
            var result = await _creditCardService.GetCreditCardsAsync(cedula, estado, pagina);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AssignCard([FromBody] CreateCreditCardForApiDTO dto)
        {
            if (dto.ClienteId == null || dto.Limite <= 0)
                return BadRequest("Datos inválidos");

            try
            {
                await _creditCardService.CreateCreditCardAsync(dto);
                return Created("", new { message = "Tarjeta creada exitosamente" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("ya existe"))
            {
                return Conflict(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        [HttpPatch("{id}/limit")]
        public async Task<IActionResult> UpdateCreditLimit(int id, [FromBody] UpdateCreditLimitForApiDTO dto)
        {
            var card = await _creditCardService.GetById(id);
            if (card == null)
                return NotFound();

            if (dto.NuevoLimite < card.TotalOwedAmount)
                return BadRequest("El nuevo límite no puede ser menor a la deuda actual.");

            card.CreditLimit = dto.NuevoLimite;
            await _creditCardService.UpdateAsync(card, id);

            return NoContent();
        }

        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> CancelCreditCard(int id)
        {
            var card = await _creditCardService.GetById(id);
            if (card == null)
                return NotFound();

            if (card.TotalOwedAmount > 0)
                return BadRequest("La tarjeta no puede cancelarse si tiene deuda pendiente.");

            card.IsActive = false;
            await _creditCardService.UpdateAsync(card, id);

            return NoContent();
        }

    }
}