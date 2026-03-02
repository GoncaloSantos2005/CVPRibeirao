using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering; // ← Adicionado
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Web.Services;

namespace SistemaPDI.Web.Controllers
{
    /// <summary>
    /// Controller MVC para gestão de lotes.
    /// </summary>
    public class LotesController : BaseController
    {
        public LotesController(IPdiApiService pdiService) : base(pdiService)
        {
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            RequererPerfil(context, "ADMINISTRADOR", "GESTOR");
        }

        // ══════════════════════════════════════════════════════════════════════
        // LISTAGEM
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /Lotes
        /// Lista todos os lotes.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var resultado = await _pdiService.ObterLotesAsync();

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return View(new List<LoteDto>());
            }

            return View(resultado.Dados);
        }

        /// <summary>
        /// GET /Lotes/PorArtigo/5
        /// Lista lotes de um artigo específico.
        /// </summary>
        public async Task<IActionResult> PorArtigo(int id)
        {
            var resultado = await _pdiService.ObterLotesPorArtigoAsync(id);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return RedirectToAction("Index", "Artigos");
            }

            var artigo = await _pdiService.ObterArtigoPorIdAsync(id);
            ViewBag.Artigo = artigo.Dados;

            return View(resultado.Dados);
        }

        /// <summary>
        /// GET /Lotes/AlertasValidade
        /// Lista lotes com validade próxima ou expirados (RN13).
        /// </summary>
        public async Task<IActionResult> AlertasValidade()
        {
            var resultado = await _pdiService.ObterAlertasValidadeAsync(15);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return View(new List<AlertaValidadeDto>());
            }

            return View(resultado.Dados);
        }

        // ══════════════════════════════════════════════════════════════════════
        // DETALHES
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /Lotes/Detalhes/5
        /// Mostra detalhes de um lote.
        /// </summary>
        public async Task<IActionResult> Detalhes(int id)
        {
            var resultado = await _pdiService.ObterLotePorIdAsync(id);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return RedirectToAction(nameof(Index));
            }

            return View(resultado.Dados);
        }

        // ══════════════════════════════════════════════════════════════════════
        // CRIAR (Receção de Stock)
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /Lotes/Criar
        /// Formulário para criar novo lote.
        /// </summary>
        public async Task<IActionResult> Criar(int? artigoId)
        {
            await CarregarArtigos();
            await CarregarLocalizacoesAsync(); // ← Adicionado

            if (artigoId.HasValue)
                ViewBag.ArtigoIdSelecionado = artigoId.Value;

            return View(new CriarLoteDto(
                artigoId ?? 0,
                string.Empty,
                DateTime.Today.AddMonths(6),
                0,
                0,
                null
            ));
        }

        /// <summary>
        /// POST /Lotes/Criar
        /// Cria um novo lote.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Criar(CriarLoteDto dto)
        {
            if (!ModelState.IsValid)
            {
                await CarregarArtigos();
                return View(dto);
            }

            var resultado = await _pdiService.CriarLoteAsync(dto);

            if (!resultado.Sucesso)
            {
                await CarregarArtigos();
                ModelState.AddModelError("", resultado.Erro!);
                return View(dto);
            }

            MensagemSucesso($"Lote '{dto.NumeroLote}' criado com sucesso!");
            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════════════
        // EDITAR
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /Lotes/Editar/5
        /// Formulário para editar lote.
        /// </summary>
        public async Task<IActionResult> Editar(int id)
        {
            var resultado = await _pdiService.ObterLotePorIdAsync(id);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return RedirectToAction(nameof(Index));
            }

            var lote = resultado.Dados!;
            ViewBag.Lote = lote;
            await CarregarLocalizacoesAsync();  

            return View(new AtualizarLoteDto(
                lote.DataValidade,
                lote.PrecoUnitario,
                lote.LocalizacaoId,
                lote.QtdDisponivel
            ));
        }

        /// <summary>
        /// POST /Lotes/Editar/5
        /// Atualiza um lote.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Editar(int id, AtualizarLoteDto dto)
        {
            if (!ModelState.IsValid)
            {
                var loteAtual = await _pdiService.ObterLotePorIdAsync(id);
                ViewBag.Lote = loteAtual.Dados;
                return View(dto);
            }

            var resultado = await _pdiService.AtualizarLoteAsync(id, dto);

            if (!resultado.Sucesso)
            {
                var loteAtual = await _pdiService.ObterLotePorIdAsync(id);
                ViewBag.Lote = loteAtual.Dados;
                ModelState.AddModelError("", resultado.Erro!);
                return View(dto);
            }

            MensagemSucesso("Lote atualizado com sucesso!");
            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════════════
        // DESATIVAR
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// POST /Lotes/Desativar/5
        /// Desativa um lote.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Desativar(int id)
        {
            var resultado = await _pdiService.DesativarLoteAsync(id);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return RedirectToAction(nameof(Index));
            }

            MensagemSucesso("Lote desativado com sucesso!");
            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════════════════════════════════════

        private async Task CarregarArtigos()
        {
            var artigos = await _pdiService.ObterArtigosAsync();
            ViewBag.Artigos = artigos.Dados ?? new List<ArtigoDto>();
        }

        private async Task CarregarLocalizacoesAsync()
        {
            var resultado = await _pdiService.ObterLocalizacoesDropdownAsync();
            
            if (resultado.Sucesso && resultado.Dados != null)
            {
                ViewBag.Localizacoes = resultado.Dados
                    .Select(l => new SelectListItem { Value = l.Id.ToString(), Text = l.Label })
                    .ToList();
            }
            else
            {
                ViewBag.Localizacoes = new List<SelectListItem>();
            }
        }
    }
}