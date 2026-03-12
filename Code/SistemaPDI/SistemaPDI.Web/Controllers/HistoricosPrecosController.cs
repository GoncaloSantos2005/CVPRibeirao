using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Web.Services;

namespace SistemaPDI.Web.Controllers
{
    public class HistoricosPrecosController : BaseController
    {
        public HistoricosPrecosController(IPdiApiService pdiService) : base(pdiService)
        {
        }

        // ══════════════════════════════════════════════════════════════════════
        // FILTRO DE ACESSO
        // ══════════════════════════════════════════════════════════════════════
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            RequererPerfil(context, "GESTOR", "GESTOR_FINANCEIRO", "ADMINISTRADOR");
        }

        // ══════════════════════════════════════════════════════════════════════
        // INDEX - DASHBOARD DE ANÁLISE
        // ══════════════════════════════════════════════════════════════════════

        // GET /HistoricosPrecos
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Por padrão, mostrar últimos 30 dias
            var dataFim = DateTime.Today;
            var dataInicio = dataFim.AddDays(-30);

            var resultado = await _pdiService.ObterHistoricoPorPeriodoAsync(dataInicio, dataFim);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return View(new List<HistoricoPrecoDto>());
            }

            ViewData["Title"] = "Histórico de Preços";
            ViewBag.DataInicio = dataInicio;
            ViewBag.DataFim = dataFim;

            return View(resultado.Dados ?? new List<HistoricoPrecoDto>());
        }

        // ══════════════════════════════════════════════════════════════════════
        // FILTRAR POR PERÍODO
        // ══════════════════════════════════════════════════════════════════════

        // GET /HistoricosPrecos/Filtrar
        [HttpGet]
        public async Task<IActionResult> Filtrar(DateTime? dataInicio, DateTime? dataFim)
        {
            var inicio = dataInicio ?? DateTime.Today.AddDays(-30);
            var fim = dataFim ?? DateTime.Today;

            var resultado = await _pdiService.ObterHistoricoPorPeriodoAsync(inicio, fim);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Histórico de Preços";
            ViewBag.DataInicio = inicio;
            ViewBag.DataFim = fim;

            return View("Index", resultado.Dados ?? new List<HistoricoPrecoDto>());
        }

        // ══════════════════════════════════════════════════════════════════════
        // ANÁLISE POR ARTIGO
        // ══════════════════════════════════════════════════════════════════════

        // GET /HistoricosPrecos/Artigo/5
        [HttpGet]
        public async Task<IActionResult> Artigo(int id)
        {
            // Obter dados do artigo
            var artigoResultado = await _pdiService.ObterArtigoPorIdAsync(id);
            if (!artigoResultado.Sucesso)
            {
                MensagemErro("Artigo não encontrado");
                return RedirectToAction(nameof(Index));
            }

            // Obter histórico
            var historicoResultado = await _pdiService.ObterHistoricoPorArtigoAsync(id);

            // Obter evolução
            var evolucaoResultado = await _pdiService.ObterEvolucaoPrecosAsync(id);

            // Obter comparação de fornecedores
            var comparacaoResultado = await _pdiService.CompararFornecedoresAsync(id);

            ViewBag.Artigo = artigoResultado.Dados;
            ViewBag.Historico = historicoResultado.Dados ?? new List<HistoricoPrecoDto>();
            ViewBag.Evolucao = evolucaoResultado.Dados ?? new List<EvolucaoPrecoDto>();
            ViewBag.Comparacao = comparacaoResultado.Dados;

            ViewData["Title"] = $"Histórico de Preços - {artigoResultado.Dados!.Nome}";

            return View();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ANÁLISE POR FORNECEDOR
        // ══════════════════════════════════════════════════════════════════════

        // GET /HistoricosPrecos/Fornecedor/5
        [HttpGet]
        public async Task<IActionResult> Fornecedor(int id)
        {
            // Obter dados do fornecedor
            var fornecedorResultado = await _pdiService.ObterFornecedorPorIdAsync(id);
            if (!fornecedorResultado.Sucesso)
            {
                MensagemErro("Fornecedor não encontrado");
                return RedirectToAction(nameof(Index));
            }

            // Obter histórico
            var historicoResultado = await _pdiService.ObterHistoricoPorFornecedorAsync(id);

            ViewBag.Fornecedor = fornecedorResultado.Dados;
            ViewBag.Historico = historicoResultado.Dados ?? new List<HistoricoPrecoDto>();

            ViewData["Title"] = $"Histórico de Preços - {fornecedorResultado.Dados!.Nome}";

            return View();
        }

        // ══════════════════════════════════════════════════════════════════════
        // COMPARAÇÃO ENTRE FORNECEDORES
        // ══════════════════════════════════════════════════════════════════════

        // GET /HistoricosPrecos/Comparar
        [HttpGet]
        public async Task<IActionResult> Comparar()
        {
            // Carregar artigos para dropdown
            var artigos = await _pdiService.ObterArtigosAsync();
            ViewBag.Artigos = artigos.Dados ?? new List<ArtigoDto>();

            ViewData["Title"] = "Comparar Fornecedores";

            return View();
        }

        // POST /HistoricosPrecos/Comparar
        [HttpPost]
        public IActionResult Comparar(int artigoId)
        {
            if (artigoId == 0)
            {
                MensagemErro("Selecione um artigo");
                return RedirectToAction(nameof(Comparar));
            }

            return RedirectToAction(nameof(Artigo), new { id = artigoId });
        }
    }
}