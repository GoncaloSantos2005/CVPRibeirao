using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaPDI.Application.Interfaces.IServices;

namespace SistemaPDI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HistoricosPrecosController : ControllerBase
    {
        private readonly IHistoricoPrecoService _historicoPrecoService;

        public HistoricosPrecosController(IHistoricoPrecoService historicoPrecoService)
        {
            _historicoPrecoService = historicoPrecoService;
        }

        // ══════════════════════════════════════════════════════════════════════
        // CONSULTAS
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>GET /api/historicosprecos</summary>
        [HttpGet]
        [Authorize(Roles = "GESTOR,GESTOR_FINANCEIRO,ADMINISTRADOR")]
        public async Task<IActionResult> ObterTodos()
        {
            var resultado = await _historicoPrecoService.ObterTodosAsync();
            return Ok(resultado.Dados);
        }

        /// <summary>GET /api/historicosprecos/{id}</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var resultado = await _historicoPrecoService.ObterPorIdAsync(id);

            if (!resultado.Sucesso)
                return NotFound(resultado.Erro);

            return Ok(resultado.Dados);
        }

        /// <summary>GET /api/historicosprecos/artigo/{artigoId}</summary>
        [HttpGet("artigo/{artigoId}")]
        public async Task<IActionResult> ObterPorArtigo(int artigoId)
        {
            var resultado = await _historicoPrecoService.ObterPorArtigoAsync(artigoId);
            return Ok(resultado.Dados);
        }

        /// <summary>GET /api/historicosprecos/fornecedor/{fornecedorId}</summary>
        [HttpGet("fornecedor/{fornecedorId}")]
        public async Task<IActionResult> ObterPorFornecedor(int fornecedorId)
        {
            var resultado = await _historicoPrecoService.ObterPorFornecedorAsync(fornecedorId);
            return Ok(resultado.Dados);
        }

        /// <summary>GET /api/historicosprecos/encomenda/{encomendaId}</summary>
        [HttpGet("encomenda/{encomendaId}")]
        public async Task<IActionResult> ObterPorEncomenda(int encomendaId)
        {
            var resultado = await _historicoPrecoService.ObterPorEncomendaAsync(encomendaId);
            return Ok(resultado.Dados);
        }

        /// <summary>GET /api/historicosprecos/periodo?dataInicio=2026-01-01&dataFim=2026-12-31</summary>
        [HttpGet("periodo")]
        [Authorize(Roles = "GESTOR,GESTOR_FINANCEIRO,ADMINISTRADOR")]
        public async Task<IActionResult> ObterPorPeriodo([FromQuery] DateTime dataInicio, [FromQuery] DateTime dataFim)
        {
            var resultado = await _historicoPrecoService.ObterPorPeriodoAsync(dataInicio, dataFim);

            if (!resultado.Sucesso)
                return BadRequest(resultado.Erro);

            return Ok(resultado.Dados);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ANÁLISES
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>GET /api/historicosprecos/evolucao/{artigoId}</summary>
        [HttpGet("evolucao/{artigoId}")]
        [Authorize(Roles = "GESTOR,GESTOR_FINANCEIRO,ADMINISTRADOR")]
        public async Task<IActionResult> ObterEvolucaoPrecos(int artigoId)
        {
            var resultado = await _historicoPrecoService.ObterEvolucaoPrecosAsync(artigoId);

            if (!resultado.Sucesso)
                return BadRequest(resultado.Erro);

            return Ok(resultado.Dados);
        }

        /// <summary>GET /api/historicosprecos/comparar-fornecedores/{artigoId}</summary>
        [HttpGet("comparar-fornecedores/{artigoId}")]
        [Authorize(Roles = "GESTOR,GESTOR_FINANCEIRO,ADMINISTRADOR")]
        public async Task<IActionResult> CompararFornecedores(int artigoId)
        {
            var resultado = await _historicoPrecoService.CompararFornecedoresAsync(artigoId);

            if (!resultado.Sucesso)
                return BadRequest(resultado.Erro);

            return Ok(resultado.Dados);
        }

        /// <summary>POST /api/historicosprecos/sugestoes</summary>
        [HttpPost("sugestoes")]
        [Authorize(Roles = "GESTOR,ADMINISTRADOR")]
        public async Task<IActionResult> ObterSugestoesPrecos([FromBody] SugestoesPrecosRequest request)
        {
            var resultado = await _historicoPrecoService.ObterSugestoesPrecosAsync(
                request.ArtigosIds,
                request.FornecedorId
            );

            return Ok(resultado.Dados);
        }
    }

    // DTO auxiliar
    public record SugestoesPrecosRequest(List<int> ArtigosIds, int? FornecedorId);
}