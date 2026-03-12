using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaPDI.Application.Interfaces.IServices;
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Web.Models;

namespace SistemaPDI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EncomendasController : ControllerBase
    {
        private readonly IEncomendaService _encomendaService;

        public EncomendasController(IEncomendaService encomendaService)
        {
            _encomendaService = encomendaService;
        }

        // ══════════════════════════════════════════════════════════════════════
        // CONSULTAS
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>GET /api/encomendas</summary>
        [HttpGet]
        public async Task<IActionResult> ObterTodos([FromQuery] bool incluirInativos = false)
        {
            if (incluirInativos && !User.IsInRole("ADMINISTRADOR"))
                return Forbid();

            var resultado = await _encomendaService.ObterTodosAsync(incluirInativos);
            return Ok(resultado.Dados);
        }

        /// <summary>GET /api/encomendas/{id}</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var resultado = await _encomendaService.ObterPorIdAsync(id);

            if (!resultado.Sucesso)
                return NotFound(resultado.Erro);

            return Ok(resultado.Dados);
        }

        /// <summary>GET /api/encomendas/estado/{estado}</summary>
        [HttpGet("estado/{estado}")]
        public async Task<IActionResult> ObterPorEstado(string estado)
        {
            var resultado = await _encomendaService.ObterPorEstadoAsync(estado);

            if (!resultado.Sucesso)
                return BadRequest(resultado.Erro);

            return Ok(resultado.Dados);
        }

        /// <summary>GET /api/encomendas/minhas</summary>
        [HttpGet("minhas")]
        [Authorize(Roles = "GESTOR,ADMINISTRADOR")]
        public async Task<IActionResult> ObterMinhas()
        {
            var emailUtilizador = User.Identity?.Name ?? "sistema";
            var resultado = await _encomendaService.ObterMinhasEncomendasAsync(emailUtilizador);
            return Ok(resultado.Dados);
        }

        /// <summary>GET /api/encomendas/pendentes-aprovacao</summary>
        [HttpGet("pendentes-aprovacao")]
        [Authorize(Roles = "GESTOR_FINANCEIRO,ADMINISTRADOR")]
        public async Task<IActionResult> ObterPendentesAprovacao()
        {
            var resultado = await _encomendaService.ObterPendentesAprovacaoAsync();
            return Ok(resultado.Dados);
        }

        /// <summary>GET /api/encomendas/enviadas</summary>
        [HttpGet("enviadas")]
        public async Task<IActionResult> ObterEnviadas()
        {
            var resultado = await _encomendaService.ObterEncomendasEnviadasAsync();
            return Ok(resultado.Dados);
        }

        /// <summary>GET /api/encomendas/dropdown</summary>
        [HttpGet("dropdown")]
        public async Task<IActionResult> ObterDropdown()
        {
            var resultado = await _encomendaService.ObterPendentesParaDropdownAsync();
            return Ok(resultado.Dados);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 1: CRIAR LISTA (Estado: LISTA)
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>POST /api/encomendas/lista</summary>
        [HttpPost("lista")]
        [Authorize(Roles = "GESTOR,ADMINISTRADOR")]
        public async Task<IActionResult> CriarLista([FromBody] CriarListaDto dto)
        {
            var emailUtilizador = User.Identity?.Name ?? "sistema";
            var resultado = await _encomendaService.CriarListaAsync(dto, emailUtilizador);

            if (!resultado.Sucesso)
                return BadRequest(resultado.Erro);

            return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Dados!.Id }, resultado.Dados);
        }

        /// <summary>PUT /api/encomendas/{id}/lista</summary>
        [HttpPut("{id}/lista")]
        [Authorize(Roles = "GESTOR,ADMINISTRADOR")]
        public async Task<IActionResult> AtualizarLista(int id, [FromBody] CriarListaDto dto)
        {
            var resultado = await _encomendaService.AtualizarListaAsync(id, dto);

            if (!resultado.Sucesso)
                return BadRequest(resultado.Erro);

            return Ok(resultado.Dados);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 2: GERAR PDF E MARCAR COMO RASCUNHO (LISTA → RASCUNHO)
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>GET /api/encomendas/{id}/gerar-pdf</summary>
        [HttpGet("{id}/gerar-pdf")]
        [Authorize(Roles = "GESTOR,ADMINISTRADOR")]
        public async Task<IActionResult> GerarPdf(int id)
        {
            var resultado = await _encomendaService.GerarPdfListaAsync(id);

            if (!resultado.Sucesso)
                return BadRequest(ApiResult.Falhou(resultado.Erro));

            return File(resultado.Dados!, "application/pdf", $"Lista_Encomenda_{id}.pdf");
        }

        /// <summary>POST /api/encomendas/{id}/marcar-rascunho</summary>
        [HttpPost("{id}/marcar-rascunho")]
        [Authorize(Roles = "GESTOR,ADMINISTRADOR")]
        public async Task<IActionResult> MarcarComoRascunho(int id)
        {
            var emailUtilizador = User.Identity?.Name ?? "sistema";
            var resultado = await _encomendaService.MarcarComoRascunhoAsync(id, emailUtilizador);

            if (!resultado.Sucesso)
                return BadRequest(resultado.Erro);

            return Ok(new { message = "Encomenda marcada como rascunho (PDF gerado)", dados = resultado.Dados });
        }

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 3: SUBMETER ORÇAMENTO (RASCUNHO → PENDENTE)
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>POST /api/encomendas/{id}/submeter-orcamento</summary>
        [HttpPost("{id}/submeter-orcamento")]
        [Authorize(Roles = "GESTOR,ADMINISTRADOR")]
        public async Task<IActionResult> SubmeterOrcamento(int id, [FromBody] SubmeterOrcamentoDto dto)
        {
            if (dto.ValorOrcamento <= 0)
                return BadRequest("Valor do orçamento deve ser maior que zero");

            if (string.IsNullOrEmpty(dto.CaminhoOrcamentoPdf))
                return BadRequest("Caminho do ficheiro PDF é obrigatório");

            var emailUtilizador = User.Identity?.Name ?? "sistema";
            var resultado = await _encomendaService.SubmeterOrcamentoSimplesAsync(id, dto, emailUtilizador);

            if (!resultado.Sucesso)
                return BadRequest(new { message = resultado.Erro });

            return Ok(resultado.Dados);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 4: REJEITAR (PENDENTE → LISTA)
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>POST /api/encomendas/{id}/rejeitar</summary>
        [HttpPost("{id}/rejeitar")]
        [Authorize(Roles = "GESTOR_FINANCEIRO,ADMINISTRADOR")]
        public async Task<IActionResult> Rejeitar(int id, [FromBody] RejeitarRequest request)
        {
            var emailGestorLogistica = User.Identity?.Name ?? "sistema";
            var resultado = await _encomendaService.RejeitarAsync(id, emailGestorLogistica, request.Motivo);

            if (!resultado.Sucesso)
                return BadRequest(resultado.Erro);

            return Ok(new { message = "Encomenda rejeitada", dados = resultado.Dados });
        }

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 5: APROVAR E PREENCHER (PENDENTE → CONFIRMADA)
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>POST /api/encomendas/{id}/aprovar-preencher</summary>
        [HttpPost("{id}/aprovar-preencher")]
        [Authorize(Roles = "GESTOR_FINANCEIRO,ADMINISTRADOR")]
        public async Task<IActionResult> AprovarEPreencher(int id, [FromBody] AprovarEPreencherDto dto)
        {
            var emailGestorLogistica = User.Identity?.Name ?? "sistema";
            var resultado = await _encomendaService.AprovarEPreencherAsync(id, dto, emailGestorLogistica);

            if (!resultado.Sucesso)
                return BadRequest(resultado.Erro);

            return Ok(new { message = "Encomenda aprovada e preenchida", dados = resultado.Dados });
        }

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 6: CONFIRMAR E ENVIAR (CONFIRMADA → ENVIADA)
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>POST /api/encomendas/{id}/confirmar-enviar</summary>
        [HttpPost("{id}/confirmar-enviar")]
        [Authorize(Roles = "GESTOR,ADMINISTRADOR")]
        public async Task<IActionResult> ConfirmarEEnviar(int id)
        {
            var emailGestorLogistica = User.Identity?.Name ?? "sistema";
            var resultado = await _encomendaService.ConfirmarEEnviarAsync(id, emailGestorLogistica);

            if (!resultado.Sucesso)
                return BadRequest(resultado.Erro);

            return Ok(new { message = "Encomenda confirmada e enviada ao fornecedor", dados = resultado.Dados });
        }

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 7: REGISTAR RECEÇÃO (ENVIADA → PARCIAL/CONCLUIDA)
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>POST /api/encomendas/registar-recepcao</summary>
        [HttpPost("registar-recepcao")]
        [Authorize(Roles = "GESTOR,SOCORRISTA,ADMINISTRADOR")]
        public async Task<IActionResult> RegistarRececao([FromBody] RegistarRecepcaoDto dto)
        {
            var emailUtilizador = User.Identity?.Name ?? "sistema";
            var resultado = await _encomendaService.RegistarRecepcaoAsync(dto, emailUtilizador);

            if (!resultado.Sucesso)
                return BadRequest(resultado.Erro);

            return Ok(new { message = "Receção registada com sucesso", dados = resultado.Dados });
        }

        // ══════════════════════════════════════════════════════════════════════
        // OUTRAS OPERAÇÕES
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>POST /api/encomendas/{id}/cancelar</summary>
        [HttpPost("{id}/cancelar")]
        [Authorize(Roles = "GESTOR,ADMINISTRADOR")]
        public async Task<IActionResult> Cancelar(int id, [FromBody] CancelarRequest request) 
        {
            var resultado = await _encomendaService.CancelarAsync(id, request.Motivo);

            if (!resultado.Sucesso)
                return BadRequest(resultado.Erro);

            return Ok(new { message = "Encomenda cancelada" });
        }

        /// <summary>PATCH /api/encomendas/{id}/toggle-ativo</summary>
        [HttpPatch("{id}/toggle-ativo")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> ToggleAtivo(int id)
        {
            var encomendaAtual = await _encomendaService.ObterPorIdAsync(id);
            if (!encomendaAtual.Sucesso)
                return NotFound();

            var novoEstado = !encomendaAtual.Dados!.Ativo;
            var resultado = await _encomendaService.AlterarEstadoAsync(id, novoEstado);

            if (!resultado.Sucesso)
                return BadRequest(resultado.Erro);

            return Ok(new { ativo = novoEstado, message = novoEstado ? "Encomenda ativada" : "Encomenda desativada" });
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // DTOs AUXILIARES (Requests)
    // ══════════════════════════════════════════════════════════════════════

    public record RejeitarRequest(string Motivo);

    public record CancelarRequest(string Motivo);
}