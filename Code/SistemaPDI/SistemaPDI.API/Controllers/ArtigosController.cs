using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaPDI.Application.Interfaces.IServices;
using SistemaPDI.Contracts.DTOs;

namespace SistemaPDI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ArtigosController : ControllerBase
    {
        private readonly IArtigoService _artigoService;

        public ArtigosController(IArtigoService artigoService)
        {
            _artigoService = artigoService;
        }

        // ══════════════════════════════════════════════════════════════════════
        // CATÁLOGO DE ARTIGOS
        // ══════════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> ObterTodos()
        {
            var resultado = await _artigoService.ObterTodosAsync();
            return Ok(resultado.Dados);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var resultado = await _artigoService.ObterPorIdAsync(id);

            if (!resultado.Sucesso)
                return NotFound(new { erro = resultado.Erro });

            return Ok(resultado.Dados);
        }

        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR,GESTOR")]
        public async Task<IActionResult> Criar([FromBody] CriarArtigoDto dto)
        {
            var resultado = await _artigoService.CriarAsync(dto);

            if (!resultado.Sucesso)
                return BadRequest(new { erro = resultado.Erro });

            return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Dados!.Id }, resultado.Dados);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMINISTRADOR,GESTOR")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarArtigoDto dto)
        {
            var resultado = await _artigoService.AtualizarAsync(id, dto);

            if (!resultado.Sucesso)
                return BadRequest(new { erro = resultado.Erro });

            return NoContent();
        }

        [HttpPatch("{id}/toggle-ativo")]
        [Authorize(Roles = "ADMINISTRADOR,GESTOR")]
        public async Task<IActionResult> AlternarEstado(int id)
        {
            var nomeUtilizador = User.Identity?.Name ?? "Sistema";
            var resultado = await _artigoService.AlternarEstadoAtivoAsync(id, nomeUtilizador);

            if (!resultado.Sucesso)
                return BadRequest(new { erro = resultado.Erro });

            return Ok(new { mensagem = "Estado do artigo alterado com sucesso." });
        }

        // ══════════════════════════════════════════════════════════════════════
        // STOCK - LISTAS FILTRADAS
        // ══════════════════════════════════════════════════════════════════════

        [HttpGet("stock-baixo")]
        public async Task<IActionResult> ObterComStockBaixo()
        {
            var resultado = await _artigoService.ObterComStockBaixoAsync();
            return Ok(resultado.Dados);
        }

        [HttpGet("stock-critico")]
        public async Task<IActionResult> ObterComStockCritico()
        {
            var resultado = await _artigoService.ObterComStockCriticoAsync();
            return Ok(resultado.Dados);
        }

        [HttpGet("desativados")]
        [Authorize(Roles = "ADMINISTRADOR,GESTOR")]
        public async Task<IActionResult> ObterDesativados()
        {
            var resultado = await _artigoService.ObterDesativadosAsync();
            return Ok(resultado.Dados);
        }

        // ══════════════════════════════════════════════════════════════════════
        // STOCK - OPERAÇÕES INDIVIDUAIS
        // ══════════════════════════════════════════════════════════════════════

        [HttpPatch("{id}/stock")]
        [Authorize(Roles = "ADMINISTRADOR,GESTOR")]
        public async Task<IActionResult> AtualizarStock(int id, [FromBody] int quantidade)
        {
            var resultado = await _artigoService.AtualizarStockFisicoAsync(id, quantidade);

            if (!resultado.Sucesso)
                return BadRequest(new { erro = resultado.Erro });

            return Ok(new { mensagem = "Stock atualizado com sucesso." });
        }

        [HttpGet("{id}/stock/sugestao")]
        public async Task<IActionResult> SugerirQuantidade(int id)
        {
            var resultado = await _artigoService.SugerirQuantidadeEncomendaAsync(id);

            if (!resultado.Sucesso)
                return NotFound(new { erro = resultado.Erro });

            return Ok(new { quantidadeSugerida = resultado.Dados });
        }
    }
}