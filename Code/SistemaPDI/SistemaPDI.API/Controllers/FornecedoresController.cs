using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaPDI.Application.Interfaces.IServices;
using SistemaPDI.Contracts.DTOs;

namespace SistemaPDI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FornecedoresController : ControllerBase
    {
        private readonly IFornecedorService _fornecedorService;

        public FornecedoresController(IFornecedorService fornecedorService)
        {
            _fornecedorService = fornecedorService;
        }

        // GET /api/fornecedores
        [HttpGet]
        public async Task<IActionResult> ObterTodos([FromQuery] bool incluirInativos = false)
        {
            // Só ADMIN pode ver inativos
            if (incluirInativos && !User.IsInRole("ADMINISTRADOR"))
                return Forbid();

            var resultado = await _fornecedorService.ObterTodosAsync(incluirInativos);
            return Ok(resultado.Dados);
        }

        // GET /api/fornecedores/dropdown
        [HttpGet("dropdown")]
        public async Task<IActionResult> ObterDropdown()
        {
            var resultado = await _fornecedorService.ObterAtivosParaDropdownAsync();
            return Ok(resultado.Dados);
        }

        // GET /api/fornecedores/preferenciais
        [HttpGet("preferenciais")]
        public async Task<IActionResult> ObterPreferenciais()
        {
            var resultado = await _fornecedorService.ObterPreferenciaisParaDropdownAsync();
            return Ok(resultado.Dados);
        }

        // GET /api/fornecedores/5
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var resultado = await _fornecedorService.ObterPorIdAsync(id);

            if (!resultado.Sucesso)
                return NotFound(resultado.Erro);

            return Ok(resultado.Dados);
        }

        // POST /api/fornecedores
        [HttpPost]
        [Authorize(Roles = "GESTOR,ADMINISTRADOR")]
        public async Task<IActionResult> Criar([FromBody] CriarFornecedorDto dto)
        {
            var resultado = await _fornecedorService.CriarAsync(dto);

            if (!resultado.Sucesso)
                return BadRequest(resultado.Erro);

            return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Dados!.Id }, resultado.Dados);
        }

        // PUT /api/fornecedores/5
        [HttpPut("{id}")]
        [Authorize(Roles = "GESTOR,ADMINISTRADOR")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarFornecedorDto dto)
        {
            var resultado = await _fornecedorService.AtualizarAsync(id, dto);

            if (!resultado.Sucesso)
                return BadRequest(resultado.Erro);

            return Ok(resultado.Dados);
        }

        // PATCH /api/fornecedores/5/toggle-ativo
        [HttpPatch("{id}/toggle-ativo")]
        [Authorize(Roles = "GESTOR,ADMINISTRADOR")]
        public async Task<IActionResult> ToggleAtivo(int id)
        {
            var fornecedorAtual = await _fornecedorService.ObterPorIdAsync(id);
            if (!fornecedorAtual.Sucesso)
                return NotFound();

            var novoEstado = !fornecedorAtual.Dados!.Ativo;
            var resultado = await _fornecedorService.AlterarEstadoAsync(id, novoEstado);

            if (!resultado.Sucesso)
                return BadRequest(resultado.Erro);

            return Ok(new { ativo = novoEstado, message = novoEstado ? "Fornecedor ativado" : "Fornecedor desativado" });
        }

        // PATCH /api/fornecedores/5/toggle-preferencial
        [HttpPatch("{id}/toggle-preferencial")]
        [Authorize(Roles = "GESTOR,ADMINISTRADOR")]
        public async Task<IActionResult> TogglePreferencial(int id)
        {
            var resultado = await _fornecedorService.TogglePreferencialAsync(id);

            if (!resultado.Sucesso)
                return BadRequest(resultado.Erro);

            var fornecedorAtualizado = await _fornecedorService.ObterPorIdAsync(id);
            return Ok(new
            {
                preferencial = fornecedorAtualizado.Dados!.Preferencial,
                message = fornecedorAtualizado.Dados.Preferencial ? "Marcado como preferencial" : "Removido de preferenciais"
            });
        }

        // DELETE /api/fornecedores/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> Apagar(int id)
        {
            var resultado = await _fornecedorService.ApagarAsync(id);

            if (!resultado.Sucesso)
                return BadRequest(resultado.Erro);

            return NoContent();
        }
    }
}