using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaPDI.Application.Interfaces.IServices;
using SistemaPDI.Contracts.DTOs;

namespace SistemaPDI.API.Controllers
{
    /// <summary>
    /// Controller para gestão de lotes.
    /// Expõe operações CRUD e algoritmo FEFO para reservas.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LotesController : ControllerBase
    {
        private readonly ILoteService _loteService;

        public LotesController(ILoteService loteService)
        {
            _loteService = loteService;
        }

        // ══════════════════════════════════════════════════════════════════════
        // LEITURA
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/lotes
        /// Obtém todos os lotes ativos.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var resultado = await _loteService.ObterTodosAsync();
            return Ok(resultado.Dados);
        }

        /// <summary>
        /// GET /api/lotes/5
        /// Obtém um lote por ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var resultado = await _loteService.ObterPorIdAsync(id);

            if (!resultado.Sucesso)
                return NotFound(new { message = resultado.Erro });

            return Ok(resultado.Dados);
        }

        /// <summary>
        /// GET /api/lotes/artigo/5
        /// Obtém todos os lotes de um artigo.
        /// </summary>
        [HttpGet("artigo/{artigoId}")]
        public async Task<IActionResult> GetByArtigo(int artigoId)
        {
            var resultado = await _loteService.ObterPorArtigoAsync(artigoId);

            if (!resultado.Sucesso)
                return NotFound(new { message = resultado.Erro });

            return Ok(resultado.Dados);
        }

        /// <summary>
        /// GET /api/lotes/alertas-validade?dias=15
        /// Obtém lotes com validade próxima ou expirados (RN13).
        /// </summary>
        [HttpGet("alertas-validade")]
        public async Task<IActionResult> GetAlertasValidade([FromQuery] int dias = 15)
        {
            var resultado = await _loteService.ObterAlertasValidadeAsync(dias);
            return Ok(resultado.Dados);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ESCRITA
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// POST /api/lotes
        /// Cria um novo lote (receção de stock).
        /// Valida RN06 (duplicado) e RN17 (data validade).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR,GESTOR")]
        public async Task<IActionResult> Criar([FromBody] CriarLoteDto dto)
        {
            var resultado = await _loteService.CriarAsync(dto);

            if (!resultado.Sucesso)
                return BadRequest(new { message = resultado.Erro });

            return CreatedAtAction(nameof(GetById), new { id = resultado.Dados!.Id }, resultado.Dados);
        }

        /// <summary>
        /// PUT /api/lotes/5
        /// Atualiza um lote existente.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMINISTRADOR,GESTOR")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarLoteDto dto)
        {
            var resultado = await _loteService.AtualizarAsync(id, dto);

            if (!resultado.Sucesso)
                return BadRequest(new { message = resultado.Erro });

            return Ok(resultado.Dados);
        }

        /// <summary>
        /// DELETE /api/lotes/5
        /// Desativa um lote.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> Desativar(int id)
        {
            var resultado = await _loteService.DesativarAsync(id);

            if (!resultado.Sucesso)
                return BadRequest(new { message = resultado.Erro });

            return NoContent();
        }

        // ══════════════════════════════════════════════════════════════════════
        // FEFO - RESERVAS (RN03, RN04, RN05)
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// POST /api/lotes/reservar
        /// Reserva stock usando algoritmo FEFO (RN03, RN04).
        /// </summary>
        [HttpPost("reservar")]
        [Authorize(Roles = "ADMINISTRADOR,GESTOR")]
        public async Task<IActionResult> ReservarStock([FromBody] ReservaStockDto dto)
        {
            var resultado = await _loteService.ReservarStockFEFOAsync(dto);

            if (!resultado.Sucesso)
                return BadRequest(new { message = resultado.Erro });

            return Ok(resultado.Dados);
        }

        /// <summary>
        /// POST /api/lotes/libertar-reserva
        /// Liberta reservas de um lote (RN05).
        /// </summary>
        [HttpPost("libertar-reserva")]
        [Authorize(Roles = "ADMINISTRADOR,GESTOR")]
        public async Task<IActionResult> LibertarReserva([FromBody] LibertarReservaDto dto)
        {
            var resultado = await _loteService.LibertarReservaAsync(dto);

            if (!resultado.Sucesso)
                return BadRequest(new { message = resultado.Erro });

            return NoContent();
        }

        /// <summary>
        /// POST /api/lotes/libertar-reservas-expiradas
        /// Liberta todas as reservas expiradas (job automático - RN05).
        /// </summary>
        [HttpPost("libertar-reservas-expiradas")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> LibertarReservasExpiradas()
        {
            var resultado = await _loteService.LibertarReservasExpiradasAsync();

            if (!resultado.Sucesso)
                return BadRequest(new { message = resultado.Erro });

            return Ok(new { libertadas = resultado.Dados });
        }

        /// <summary>
        /// POST /api/lotes/5/confirmar-saida
        /// Confirma a saída de stock (converte reserva em saída definitiva).
        /// </summary>
        [HttpPost("{loteId}/confirmar-saida")]
        [Authorize(Roles = "ADMINISTRADOR,GESTOR")]
        public async Task<IActionResult> ConfirmarSaida(int loteId, [FromBody] ConfirmarSaidaRequest request)
        {
            var resultado = await _loteService.ConfirmarSaidaAsync(loteId, request.Quantidade);

            if (!resultado.Sucesso)
                return BadRequest(new { message = resultado.Erro });

            return NoContent();
        }

        // ══════════════════════════════════════════════════════════════════════
        // STOCK
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/lotes/stock-disponivel/5
        /// Calcula o stock total disponível de um artigo.
        /// </summary>
        [HttpGet("stock-disponivel/{artigoId}")]
        public async Task<IActionResult> GetStockDisponivel(int artigoId)
        {
            var resultado = await _loteService.CalcularStockDisponivelAsync(artigoId);

            if (!resultado.Sucesso)
                return NotFound(new { message = resultado.Erro });

            return Ok(new { artigoId, stockDisponivel = resultado.Dados });
        }
    }

    /// <summary>
    /// Request para confirmar saída de stock.
    /// </summary>
    public record ConfirmarSaidaRequest(int Quantidade);
}