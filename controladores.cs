// Controllers/EstacionamentoController.cs
using Microsoft.AspNetCore.Mvc;
using SistemaEstacionamento.DTOs;
using SistemaEstacionamento.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaEstacionamento.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstacionamentoController : ControllerBase
    {
        private readonly EstacionamentoService _service;

        public EstacionamentoController(EstacionamentoService service)
        {
            _service = service;
        }

        /// <summary>
        /// Registra entrada de veículo.
        /// </summary>
        [HttpPost("entradas")]
        public async Task<IActionResult> RegistrarEntrada([FromBody] EntradaDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _service.RegistrarEntradaAsync(dto.Placa!, dto.Modelo!);
                return CreatedAtAction(nameof(ObterPorPlaca), new { placa = dto.Placa }, new { mensagem = "Entrada registrada" });
            }
            catch (System.InvalidOperationException ex)
            {
                return Conflict(new { erro = ex.Message });
            }
        }

        /// <summary>
        /// Registra saída de veículo e retorna valor cobrado.
        /// </summary>
        [HttpPost("saidas")]
        public async Task<IActionResult> RegistrarSaida([FromBody] SaidaDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var valor = await _service.RegistrarSaidaAsync(dto.Placa!);
                return Ok(new { mensagem = "Saída registrada", valorCobrado = valor });
            }
            catch (System.InvalidOperationException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
        }

        /// <summary>
        /// Retorna número de vagas disponíveis.
        /// </summary>
        [HttpGet("vagas")]
        public async Task<IActionResult> VagasDisponiveis()
        {
            var vagas = await _service.VagasDisponiveisAsync();
            return Ok(new { vagasDisponiveis = vagas });
        }

        /// <summary>
        /// Lista histórico de veículos (entradas e saídas).
        /// </summary>
        [HttpGet("veiculos")]
        public async Task<IActionResult> ListarVeiculos()
        {
            var lista = await _service.ListarVeiculosAsync();
            var dto = lista.Select(v => new DTOs.VeiculoViewDto
            {
                Placa = v.Placa,
                Modelo = v.Modelo,
                EntradaUtc = v.EntradaUtc,
                SaidaUtc = v.SaidaUtc,
                ValorPago = v.ValorPago,
                Pago = v.Pago
            }).ToArray();

            return Ok(dto);
        }

        /// <summary>
        /// Obtém por placa (último registro).
        /// </summary>
        [HttpGet("veiculos/{placa}")]
        public async Task<IActionResult> ObterPorPlaca(string placa)
        {
            var lista = await _service.ListarVeiculosAsync();
            var v = lista.LastOrDefault(x => x.Placa == placa.ToUpperInvariant());
            if (v == null) return NotFound();
            var dto = new DTOs.VeiculoViewDto
            {
                Placa = v.Placa,
                Modelo = v.Modelo,
                EntradaUtc = v.EntradaUtc,
                SaidaUtc = v.SaidaUtc,
                ValorPago = v.ValorPago,
                Pago = v.Pago
            };
            return Ok(dto);
        }
    }
}
