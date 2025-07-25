using HermesBanking.Core.Application.DTOs.Commerce;
using HermesBanking.Core.Application.DTOs.User;
using HermesBanking.Core.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace HermesBanking.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class CommerceController : ControllerBase
    {
        private readonly ICommerceService _commerceService;

        public CommerceController(ICommerceService commerceService)
        {
            _commerceService = commerceService;
        }

        // 1. Obtener todos los comercios (paginado)
        [HttpGet]
        public async Task<IActionResult> GetAllCommercesAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _commerceService.GetAllCommercesAsync(page, pageSize);
                return Ok(result); // Respuesta exitosa
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message }); // Manejo de error
            }
        }

        // 2. Obtener comercio por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommerceByIdAsync(int id)
        {
            var commerce = await _commerceService.GetCommerceByIdAsync(id);
            if (commerce == null)
            {
                return NotFound();
            }
            return Ok(commerce);
        }


        // 3. Crear nuevo comercio
        [HttpPost]
        public async Task<IActionResult> CreateCommerceAsync(CreateCommerceForApiDTO dto)
        {
            var commerceDTO = await _commerceService.CreateCommerceAsync(dto);

            // Asegúrate de que el nombre de la acción sea correcto.
            return Created("", new { message = "Commerce creado correctamente"});

        }

        // 4. Actualizar comercio existente
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCommerceAsync(int id, [FromBody] UpdateCommerceForApiDTO dto)
        {
            try
            {
                var commerce = await _commerceService.UpdateCommerceAsync(id, dto);
                return Ok(commerce); // Respuesta exitosa
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message }); // Manejo de error
            }
        }

        // 5. Cambiar estado de un comercio
        [HttpPatch("{id}")]
        public async Task<IActionResult> ChangeCommerceStatusAsync(int id, [FromBody] ChangeCommerceStatusDTO dto)
        {
            try
            {
                var statusChanged = await _commerceService.ChangeCommerceStatusAsync(id, dto);
                if (!statusChanged)
                    return NotFound(new { message = "Comercio no encontrado o estado no cambiado" });

                return Ok(new { message = "Estado del comercio actualizado correctamente" }); // Respuesta exitosa
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message }); // Manejo de error
            }
        }
    }
}
