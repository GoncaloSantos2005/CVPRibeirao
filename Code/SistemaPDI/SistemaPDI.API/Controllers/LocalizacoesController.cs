using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaPDI.Application.Interfaces.IServices;
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Domain.Enums;

namespace SistemaPDI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LocalizacoesController : ControllerBase
    {
        private readonly ILocalizacaoService _localizacaoService;

        public LocalizacoesController(ILocalizacaoService localizacaoService)
        {
            _localizacaoService = localizacaoService;
        }

        // ══════════════════════════════════════════════════════════════════════
        // LEITURA
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtém todas as localizações.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObterTodos([FromQuery] bool incluirInativos = false)
        {
            var resultado = await _localizacaoService.ObterTodosAsync(incluirInativos);

            if (!resultado.Sucesso)
                return BadRequest(new { erro = resultado.Erro });

            return Ok(resultado.Dados);
        }

        /// <summary>
        /// Obtém apenas as localizações ativas.
        /// </summary>
        [HttpGet("ativas")]
        public async Task<IActionResult> ObterAtivas()
        {
            var resultado = await _localizacaoService.ObterAtivasAsync();

            if (!resultado.Sucesso)
                return BadRequest(new { erro = resultado.Erro });

            return Ok(resultado.Dados);
        }

        /// <summary>
        /// Obtém uma localização pelo ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var resultado = await _localizacaoService.ObterPorIdAsync(id);

            if (!resultado.Sucesso)
                return NotFound(new { erro = resultado.Erro });

            return Ok(resultado.Dados);
        }

        /// <summary>
        /// Obtém localizações por tipo.
        /// </summary>
        [HttpGet("tipo/{tipo}")]
        public async Task<IActionResult> ObterPorTipo(TipoLocalizacao tipo)
        {
            var resultado = await _localizacaoService.ObterPorTipoAsync(tipo);

            if (!resultado.Sucesso)
                return BadRequest(new { erro = resultado.Erro });

            return Ok(resultado.Dados);
        }

        /// <summary>
        /// Obtém localizações por zona.
        /// </summary>
        [HttpGet("zona/{zona}")]
        public async Task<IActionResult> ObterPorZona(string zona)
        {
            var resultado = await _localizacaoService.ObterPorZonaAsync(zona);

            if (!resultado.Sucesso)
                return BadRequest(new { erro = resultado.Erro });

            return Ok(resultado.Dados);
        }

        /// <summary>
        /// Obtém localizações para dropdown/select.
        /// </summary>
        [HttpGet("dropdown")]
        public async Task<IActionResult> ObterParaDropdown()
        {
            var resultado = await _localizacaoService.ObterParaDropdownAsync();

            if (!resultado.Sucesso)
                return BadRequest(new { erro = resultado.Erro });

            return Ok(resultado.Dados);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ESCRITA
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Cria uma nova localização.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR,GESTOR")]
        public async Task<IActionResult> Criar([FromBody] CriarLocalizacaoDto dto)
        {
            var resultado = await _localizacaoService.CriarAsync(dto);

            if (!resultado.Sucesso)
                return BadRequest(new { erro = resultado.Erro });

            return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Dados!.Id }, resultado.Dados);
        }

        /// <summary>
        /// Atualiza uma localização existente.
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "ADMINISTRADOR,GESTOR")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarLocalizacaoDto dto)
        {
            var resultado = await _localizacaoService.AtualizarAsync(id, dto);

            if (!resultado.Sucesso)
                return BadRequest(new { erro = resultado.Erro });

            return Ok(resultado.Dados);
        }

        /// <summary>
        /// Alterna o estado ativo/inativo de uma localização.
        /// </summary>
        [HttpPatch("{id:int}/toggle-ativo")]
        [Authorize(Roles = "ADMINISTRADOR,GESTOR")]
        public async Task<IActionResult> AlternarEstado(int id)
        {
            var resultado = await _localizacaoService.AlternarEstadoAtivoAsync(id);

            if (!resultado.Sucesso)
                return BadRequest(new { erro = resultado.Erro });

            return NoContent();
        }

        /// <summary>
        /// Apaga (soft delete) uma localização.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> Apagar(int id)
        {
            var resultado = await _localizacaoService.ApagarAsync(id);

            if (!resultado.Sucesso)
                return BadRequest(new { erro = resultado.Erro });

            return NoContent();
        }
    }
}