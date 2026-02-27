using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaPDI.Application.Interfaces.IServices;
using SistemaPDI.Contracts.DTOs;

namespace SistemaPDI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriasController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        // GET /api/categorias
        [HttpGet]
        public async Task<IActionResult> ObterTodos([FromQuery] bool incluirInativos = false)
        {
            // Só ADMIN pode ver inativas
            if (incluirInativos && !User.IsInRole("ADMINISTRADOR"))
                return Forbid();

            var resultado = await _categoriaService.ObterTodosAsync(incluirInativos);
            return Ok(resultado.Dados);
        }

        // GET /api/categorias/ativas
        [HttpGet("ativas")]
        public async Task<IActionResult> ObterAtivas()
        {
            var resultado = await _categoriaService.ObterAtivasAsync();
            return Ok(resultado.Dados);
        }

        // GET /api/categorias/5
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var resultado = await _categoriaService.ObterPorIdAsync(id);

            if (!resultado.Sucesso)
                return NotFound(resultado.Erro);

            return Ok(resultado.Dados);
        }

        // POST /api/categorias
        [HttpPost]
        [Authorize(Roles = "GESTOR,ADMINISTRADOR")]
        public async Task<IActionResult> Criar([FromBody] CriarCategoriaDto dto)
        {
            var resultado = await _categoriaService.CriarAsync(dto);

            if (!resultado.Sucesso)
                return BadRequest(resultado.Erro);

            return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Dados!.Id }, resultado.Dados);
        }

        // PUT /api/categorias/5
        [HttpPut("{id}")]
        [Authorize(Roles = "GESTOR,ADMINISTRADOR")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarCategoriaDto dto)
        {
            var resultado = await _categoriaService.AtualizarAsync(id, dto);

            if (!resultado.Sucesso)
                return BadRequest(resultado.Erro);

            return Ok(resultado.Dados);
        }

        // PATCH /api/categorias/5/toggle-ativo
        [HttpPatch("{id}/toggle-ativo")]
        [Authorize(Roles = "GESTOR,ADMINISTRADOR")]
        public async Task<IActionResult> ToggleAtivo(int id)
        {
            var categoriaAtual = await _categoriaService.ObterPorIdAsync(id);
            if (!categoriaAtual.Sucesso)
                return NotFound();

            var novoEstado = !categoriaAtual.Dados!.Ativo;
            var resultado = await _categoriaService.AlterarEstadoAsync(id, novoEstado);

            if (!resultado.Sucesso)
                return BadRequest(resultado.Erro);

            return Ok(new { ativo = novoEstado });
        }

        // DELETE /api/categorias/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> Apagar(int id)
        {
            var resultado = await _categoriaService.ApagarAsync(id);

            if (!resultado.Sucesso)
                return BadRequest(resultado.Erro);

            return NoContent();
        }
    }
}