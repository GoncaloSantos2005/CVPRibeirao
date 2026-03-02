using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Domain.Enums;
using SistemaPDI.Web.Services;

namespace SistemaPDI.Web.Controllers
{
    public class LocalizacoesController : BaseController
    {
        public LocalizacoesController(IPdiApiService pdiService) : base(pdiService)
        {
        }

        // ══════════════════════════════════════════════════════════════════════
        // FILTRO DE ACESSO
        // ══════════════════════════════════════════════════════════════════════

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            RequererPerfil(context, "GESTOR", "ADMINISTRADOR");
        }

        // ══════════════════════════════════════════════════════════════════════
        // LEITURA
        // ══════════════════════════════════════════════════════════════════════

        // GET /Localizacoes
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var resultado = await _pdiService.ObterLocalizacoesAsync();

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return View(new List<LocalizacaoDto>());
            }

            ViewData["Title"] = "Gestão de Localizações";
            return View(resultado.Dados ?? new List<LocalizacaoDto>());
        }

        // GET /Localizacoes/Desativadas
        [HttpGet]
        public async Task<IActionResult> Desativadas()
        {
            if (!VerificarPerfil("ADMINISTRADOR", "GESTOR"))
            {
                MensagemErro("Não tens permissão para aceder a esta funcionalidade.");
                return RedirectToAction(nameof(Index));
            }

            var resultado = await _pdiService.ObterLocalizacoesAsync(incluirInativos: true);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return View("Index", new List<LocalizacaoDto>());
            }

            var desativadas = resultado.Dados?.Where(l => !l.Ativo).ToList() ?? new List<LocalizacaoDto>();

            ViewData["Title"] = "Localizações Desativadas";
            ViewData["Filtro"] = "desativadas";
            return View("Index", desativadas);
        }

        // ══════════════════════════════════════════════════════════════════════
        // CRIAÇÃO
        // ══════════════════════════════════════════════════════════════════════

        // GET /Localizacoes/Criar
        [HttpGet]
        public IActionResult Criar()
        {
            ViewData["Title"] = "Nova Localização";
            CarregarTiposLocalizacao();
            return View(new CriarLocalizacaoDto(string.Empty, TipoLocalizacao.PRATELEIRA, string.Empty, null, null, null));
        }

        // POST /Localizacoes/Criar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(CriarLocalizacaoDto dto)
        {
            if (!ModelState.IsValid)
            {
                MensagemErro("Verifique os campos assinalados.");
                CarregarTiposLocalizacao();
                return View(dto);
            }

            var resultado = await _pdiService.CriarLocalizacaoAsync(dto);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                CarregarTiposLocalizacao();
                return View(dto);
            }

            MensagemSucesso($"Localização '{resultado.Dados!.Label}' criada com sucesso!");
            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════════════
        // EDIÇÃO
        // ══════════════════════════════════════════════════════════════════════

        // GET /Localizacoes/Editar/5
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var resultado = await _pdiService.ObterLocalizacaoPorIdAsync(id);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return RedirectToAction(nameof(Index));
            }

            var localizacao = resultado.Dados!;
            var dto = new AtualizarLocalizacaoDto(
                localizacao.Codigo,
                localizacao.Tipo,
                localizacao.Zona,
                localizacao.Prateleira,
                localizacao.Nivel,
                localizacao.Observacoes
            );

            ViewData["Title"] = "Editar Localização";
            ViewBag.LocalizacaoAtual = localizacao;
            CarregarTiposLocalizacao();
            return View(dto);
        }

        // POST /Localizacoes/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, AtualizarLocalizacaoDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Editar Localização";
                ViewBag.LocalizacaoAtual = (await _pdiService.ObterLocalizacaoPorIdAsync(id)).Dados;
                CarregarTiposLocalizacao();
                return View(dto);
            }

            var resultado = await _pdiService.AtualizarLocalizacaoAsync(id, dto);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                ViewBag.LocalizacaoAtual = (await _pdiService.ObterLocalizacaoPorIdAsync(id)).Dados;
                CarregarTiposLocalizacao();
                return View(dto);
            }

            MensagemSucesso("Localização atualizada com sucesso!");
            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════════════
        // ALTERNAR ESTADO
        // ══════════════════════════════════════════════════════════════════════

        // POST /Localizacoes/AlternarEstado/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarEstado(int id)
        {
            var resultado = await _pdiService.ToggleAtivoLocalizacaoAsync(id);

            if (!resultado.Sucesso)
                MensagemErro(resultado.Erro!);
            else
                MensagemSucesso("Estado da localização alterado com sucesso!");

            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════════════
        // REMOÇÃO - Apenas ADMINISTRADOR
        // ══════════════════════════════════════════════════════════════════════

        // POST /Localizacoes/Remover/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remover(int id)
        {
            if (!VerificarPerfil("ADMINISTRADOR"))
            {
                MensagemErro("Apenas Administradores podem remover localizações.");
                return RedirectToAction(nameof(Index));
            }

            var resultado = await _pdiService.ApagarLocalizacaoAsync(id);

            if (!resultado.Sucesso)
                MensagemErro(resultado.Erro!);
            else
                MensagemSucesso("Localização removida com sucesso.");

            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════════════════════════════════════

        private void CarregarTiposLocalizacao()
        {
            ViewBag.TiposLocalizacao = new List<SelectListItem>
            {
                new SelectListItem { Value = ((int)TipoLocalizacao.PRATELEIRA).ToString(), Text = "Prateleira" },
                new SelectListItem { Value = ((int)TipoLocalizacao.ARMARIO).ToString(), Text = "Armário" },
                new SelectListItem { Value = ((int)TipoLocalizacao.FRIGORIFICO).ToString(), Text = "Frigorífico" },
                new SelectListItem { Value = ((int)TipoLocalizacao.ZONA).ToString(), Text = "Zona" }
            };
        }
    }
}