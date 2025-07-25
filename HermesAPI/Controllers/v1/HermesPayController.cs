using HermesBanking.Core.Application.DTOs.HermesPay;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Transactions;

namespace HermesAPI.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class HermesPayController : ControllerBase
    {
        private readonly IHermesPayService _hermesPayService;
        private readonly ITransactionRepository _transactionRepository;

        public HermesPayController(IHermesPayService hermesPayService, ITransactionRepository transactionRepository)
        {
            _hermesPayService = hermesPayService;
            _transactionRepository = transactionRepository;
        }

        [HttpGet("get-transactions/{commerceId}")]
        public async Task<IActionResult> GetTransactionsByCommerceAsync(int commerceId, int page = 1, int pageSize = 20)
        {
            var skip = (page - 1) * pageSize;

            var filtered = await _transactionRepository
                .GetByConditionAsync(t => t.CommerceId == commerceId);

            var paged = filtered
                .OrderByDescending(t => t.TransactionDate)
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            return Ok(paged); // ✔️ ahora encaja con IActionResult
        }





        /// <summary>
        /// Endpoint para que un comercio realice un consumo con tarjeta.
        /// </summary>
        /// <param name="request">Contiene número de tarjeta, monto y comercioId</param>
        /// <returns>Resultado del procesamiento del pago</returns>
        [HttpPost("consume")]
        public async Task<IActionResult> Consume([FromBody] HermesPayRequest request)
        {
            var resultado = await _hermesPayService.ProccesHermesPay(request, User);

            if (!resultado.Exitoso)
            {
                return BadRequest(new
                {
                    Exito = false,
                    Mensaje = resultado.Mensaje
                });
            }

            return Ok(new
            {
                Exito = true,
                Mensaje = resultado.Mensaje
            });
        }
    }
}
