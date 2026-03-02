using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaPDI.Application.Interfaces.IUtilizadorServices;
using SistemaPDI.Contracts.DTOs;

namespace SistemaPDI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMINISTRADOR")]
    public class UtilizadoresController : ControllerBase
    {
        private readonly IUtilizadorService _utilizadorService;

        public UtilizadoresController(IUtilizadorService utilizadorService)
        {
            _utilizadorService = utilizadorService;
        }

        // GET /api/utilizadores
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var resultado = await _utilizadorService.ObterTodosAsync();
            return Ok(resultado.Dados);
        }

        // GET /api/utilizadores/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var resultado = await _utilizadorService.ObterPorIdAsync(id);
            if (!resultado.Sucesso)
                return NotFound(new { message = resultado.Erro });

            return Ok(resultado.Dados);
        }

        // POST /api/utilizadores
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] RegistarUtilizadorDto dto)
        {
            var resultado = await _utilizadorService.CriarAsync(dto);
            if (!resultado.Sucesso)
                return BadRequest(new { message = resultado.Erro });

            return CreatedAtAction(nameof(GetById), new { id = resultado.Dados!.Id }, resultado.Dados);
        }

        // PUT /api/utilizadores/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarUtilizadorDto dto)
        {
            var resultado = await _utilizadorService.AtualizarAsync(id, dto);
            if (!resultado.Sucesso)
                return NotFound(new { message = resultado.Erro });

            return Ok(resultado.Dados);
        }

        // DELETE /api/utilizadores/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Desativar(int id)
        {
            var resultado = await _utilizadorService.DesativarAsync(id);
            if (!resultado.Sucesso)
                return NotFound(new { message = resultado.Erro });

            return NoContent();
        }

        // PATCH /api/utilizadores/5/ativar
        [HttpPatch("{id}/ativar")]
        public async Task<IActionResult> Ativar(int id)
        {
            var resultado = await _utilizadorService.AtivarAsync(id);
            if (!resultado.Sucesso)
                return NotFound(new { message = resultado.Erro });

            return NoContent();
        }

        // PATCH /api/utilizadores/5/reset-password
        [HttpPatch("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
        {
            var resultado = await _utilizadorService.ResetPasswordAsync(id, dto.NovaPassword);
            if (!resultado.Sucesso)
                return BadRequest(new { message = resultado.Erro });

            return NoContent();
        }
    }
}