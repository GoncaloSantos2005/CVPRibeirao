using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Web.Services;

namespace SistemaPDI.Web.Controllers
{
    public class ArtigosController : BaseController
    {
        public ArtigosController(IPdiApiService pdiService, IImagemService imagemService) : base(pdiService)
        {
            _imagemService = imagemService;
        }

        private readonly IImagemService _imagemService;

        // ══════════════════════════════════════════════════════════════════════
        // FILTRO DE ACESSO - Executado antes de cada action
        // ══════════════════════════════════════════════════════════════════════

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            // Acesso base: apenas GESTOR e ADMINISTRADOR
            // SOCORRISTA terá acesso controlado por action específica
            RequererPerfil(context,"SOCORRISTA", "GESTOR", "ADMINISTRADOR");
        }

        // ══════════════════════════════════════════════════════════════════════
        // 1. LEITURA (GET) - Socorrista também pode acessar
        // ══════════════════════════════════════════════════════════════════════

        // GET /Artigos
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var resultado = await _pdiService.ObterArtigosAsync();

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return View(new List<ArtigoDto>());
            }

            ViewData["Title"] = "Catálogo de Artigos";
            return View(resultado.Dados ?? new List<ArtigoDto>());
        }

        // GET /Artigos/Detalhes/5
        [HttpGet]
        public async Task<IActionResult> Detalhes(int id)
        {
            var resultado = await _pdiService.ObterArtigoPorIdAsync(id);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Detalhes do Artigo";
            return View(resultado.Dados);
        }

        // GET /Artigos/StockBaixo
        [HttpGet]
        public async Task<IActionResult> StockBaixo()
        {
            var resultado = await _pdiService.ObterArtigosComStockBaixoAsync();

            if (!resultado.Sucesso)
            {
                MensagemErro("Aviso: " + resultado.Erro);
                return View("Index", new List<ArtigoDto>());
            }

            ViewData["Title"] = "Artigos com Stock Baixo";
            ViewData["Filtro"] = "stock-baixo";
            return View("Index", resultado.Dados);
        }

        // GET /Artigos/StockCritico
        [HttpGet]
        public async Task<IActionResult> StockCritico()
        {
            var resultado = await _pdiService.ObterArtigosComStockCriticoAsync();

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return View("Index", new List<ArtigoDto>());
            }

            ViewData["Title"] = "Artigos em Nível Crítico";
            ViewData["Filtro"] = "stock-critico";
            return View("Index", resultado.Dados);
        }

        // GET /Artigos/Desativados
        [HttpGet]
        public async Task<IActionResult> Desativados()
        {
            // Permite SOCORRISTA ver desativados (apenas leitura)
            if (!VerificarPerfil("ADMINISTRADOR", "GESTOR"))
            {
                MensagemErro("Não tens permissão para aceder a esta funcionalidade.");
                return RedirectToAction(nameof(Index));
            }

            var resultado = await _pdiService.ObterArtigosDesativadosAsync();

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return View("Index", new List<ArtigoDto>());
            }

            ViewData["Title"] = "Artigos Desativados";
            ViewData["Filtro"] = "desativados";
            return View("Index", resultado.Dados);
        }

        // ══════════════════════════════════════════════════════════════════════
        // 2. CRIAÇÃO (POST) - Apenas GESTOR e ADMINISTRADOR
        // ══════════════════════════════════════════════════════════════════════

        // GET /Artigos/Criar
        [HttpGet]
        public async Task<IActionResult> Criar()
        {
            if (!VerificarPerfil("ADMINISTRADOR", "GESTOR"))
            {
                MensagemErro("Não tens permissão para criar artigos.");
                return RedirectToAction(nameof(Index));
            }

            // Carregar categorias ativas para o dropdown
            var categorias = await _pdiService.ObterCategoriasAtivasAsync();
            ViewBag.Categorias = categorias.Dados ?? new List<CategoriaDto>();

            ViewData["Title"] = "Novo Artigo";
            return View(new CriarArtigoDto("", "", "", null, 0, null, 10, 5));
        }

        // POST /Artigos/Criar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(CriarArtigoDto dto, IFormFile? imagem)
        {
            if (!VerificarPerfil("ADMINISTRADOR", "GESTOR"))
            {
                MensagemErro("Não tens permissão para criar artigos.");
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                MensagemErro("Verifique os campos assinalados.");
                ViewBag.Categorias = (await _pdiService.ObterCategoriasAtivasAsync()).Dados ?? new List<CategoriaDto>();
                return View(dto);
            }

            try
            {
                // Upload de imagem (se existir)
                string? urlImagem = null;
                if (imagem != null && imagem.Length > 0)
                {
                    if (!_imagemService.ValidarImagem(imagem))
                    {
                        MensagemErro("Imagem inválida (máx 5MB, formatos: JPG, PNG, GIF, WEBP)");
                        ViewBag.Categorias = (await _pdiService.ObterCategoriasAtivasAsync()).Dados ?? new List<CategoriaDto>();
                        return View(dto);
                    }

                    urlImagem = await _imagemService.GuardarImagemAsync(imagem, "artigos");
                }

                // Criar novo DTO com a URL da imagem
                var dtoComImagem = dto with { UrlImagem = urlImagem };

                // Chamar API para criar artigo
                var resultado = await _pdiService.CriarArtigoAsync(dtoComImagem, urlImagem);

                if (!resultado.Sucesso)
                {
                    MensagemErro(resultado.Erro!);
                    ViewBag.Categorias = (await _pdiService.ObterCategoriasAtivasAsync()).Dados ?? new List<CategoriaDto>();
                    return View(dto);
                }

                MensagemSucesso($"Artigo '{dto.Nome}' criado com sucesso!");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                MensagemErro($"Erro ao criar artigo: {ex.Message}");
                ViewBag.Categorias = (await _pdiService.ObterCategoriasAtivasAsync()).Dados ?? new List<CategoriaDto>();
                return View(dto);
            }
        }

        // GET /Artigos/Editar/5
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            if (!VerificarPerfil("ADMINISTRADOR", "GESTOR"))
            {
                MensagemErro("Não tens permissão para editar artigos.");
                return RedirectToAction(nameof(Index));
            }

            var resultado = await _pdiService.ObterArtigoPorIdAsync(id);
            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return RedirectToAction(nameof(Index));
            }

            var artigo = resultado.Dados!;
            var dto = new AtualizarArtigoDto(
                artigo.Nome,
                artigo.Descricao,
                artigo.SKU,
                artigo.UrlImagem,
                artigo.CategoriaId ?? 0,
                artigo.CategoriaNome,
                artigo.StockMinimo,
                artigo.StockCritico
            );

            // Carregar categorias ativas para o dropdown
            ViewBag.Categorias = (await _pdiService.ObterCategoriasAtivasAsync()).Dados ?? new List<CategoriaDto>();

            ViewData["Title"] = "Editar Artigo";
            ViewBag.ArtigoAtual = artigo;
            return View(dto);
        }

        // POST /Artigos/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, AtualizarArtigoDto dto, IFormFile? imagem, bool removerImagem = false)
        {
            if (!VerificarPerfil("ADMINISTRADOR", "GESTOR"))
            {
                MensagemErro("Não tens permissão para editar artigos.");
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Editar Artigo";
                ViewBag.ArtigoAtual = (await _pdiService.ObterArtigoPorIdAsync(id)).Dados;
                ViewBag.Categorias = (await _pdiService.ObterCategoriasAtivasAsync()).Dados ?? new List<CategoriaDto>();
                return View(dto);
            }

            try
            {
                string? urlImagemAtual = dto.UrlImagem;
                string? urlImagemNova = urlImagemAtual;

                if (removerImagem && !string.IsNullOrEmpty(urlImagemAtual))
                {
                    await _imagemService.ApagarImagemAsync(urlImagemAtual);
                    urlImagemNova = null;
                }
                else if (imagem != null && imagem.Length > 0)
                {
                    if (!_imagemService.ValidarImagem(imagem))
                    {
                        MensagemErro("Imagem inválida (máx 5MB, formatos: JPG, PNG, GIF, WEBP)");
                        ViewBag.ArtigoAtual = (await _pdiService.ObterArtigoPorIdAsync(id)).Dados;
                        ViewBag.Categorias = (await _pdiService.ObterCategoriasAtivasAsync()).Dados ?? new List<CategoriaDto>();
                        return View(dto);
                    }

                    if (!string.IsNullOrEmpty(urlImagemAtual))
                    {
                        await _imagemService.ApagarImagemAsync(urlImagemAtual);
                    }

                    urlImagemNova = await _imagemService.GuardarImagemAsync(imagem, "artigos");
                }
                var dtoFinal = dto with { UrlImagem = urlImagemNova };

                var resultado = await _pdiService.AtualizarArtigoAsync(id, dtoFinal);

                if (!resultado.Sucesso)
                {
                    MensagemErro(resultado.Erro!);
                    ViewBag.ArtigoAtual = (await _pdiService.ObterArtigoPorIdAsync(id)).Dados;
                    ViewBag.Categorias = (await _pdiService.ObterCategoriasAtivasAsync()).Dados ?? new List<CategoriaDto>();
                    return View(dto);
                }

                MensagemSucesso("Artigo atualizado com sucesso!");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Categorias = (await _pdiService.ObterCategoriasAtivasAsync()).Dados ?? new List<CategoriaDto>();

                MensagemErro($"Erro ao atualizar artigo: {ex.Message}");
                ViewBag.ArtigoAtual = (await _pdiService.ObterArtigoPorIdAsync(id)).Dados;
                return View(dto);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // 4. ALTERNAR ESTADO (PATCH)
        // ══════════════════════════════════════════════════════════════════════

        // POST /Artigos/AlternarEstado/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarEstado(int id)
        {
            if (!VerificarPerfil("ADMINISTRADOR", "GESTOR"))
            {
                MensagemErro("Não tens permissão para alterar o estado de artigos.");
                return RedirectToAction(nameof(Index));
            }

            var resultado = await _pdiService.AlternarEstadoArtigoAsync(id);

            if (!resultado.Sucesso)
                MensagemErro(resultado.Erro!);
            else
                MensagemSucesso("Estado do artigo alterado com sucesso!");

            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════════════
        // 5. REMOÇÃO (DELETE) - Apenas ADMINISTRADOR
        // ══════════════════════════════════════════════════════════════════════

        // POST /Artigos/Remover/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remover(int id)
        {
            if (!VerificarPerfil("ADMINISTRADOR"))
            {
                MensagemErro("Apenas Administradores podem remover artigos permanentemente.");
                return RedirectToAction(nameof(Index));
            }

            var resultado = await _pdiService.RemoverArtigoAsync(id);

            if (!resultado.Sucesso)
                MensagemErro(resultado.Erro!);
            else
                MensagemSucesso("Artigo removido permanentemente do sistema.");

            return RedirectToAction(nameof(Index));
        }
    }
}