using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Web.Services;

namespace SistemaPDI.Web.Controllers
{
    public class CategoriasController : BaseController
    {
        public CategoriasController(IPdiApiService pdiService) : base(pdiService)
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

        // GET /Categorias
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var resultado = await _pdiService.ObterCategoriasAsync();

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return View(new List<CategoriaDto>());
            }

            ViewData["Title"] = "Gestão de Categorias";
            return View(resultado.Dados ?? new List<CategoriaDto>());
        }

        // GET /Categorias/Desativadas
        [HttpGet]
        public async Task<IActionResult> Desativadas()
        {
            if (!VerificarPerfil("ADMINISTRADOR", "GESTOR"))
            {
                MensagemErro("Não tens permissão para aceder a esta funcionalidade.");
                return RedirectToAction(nameof(Index));
            }

            var resultado = await _pdiService.ObterCategoriasAsync(incluirInativos: true);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return View("Index", new List<CategoriaDto>());
            }

            // Filtrar apenas as inativas
            var desativadas = resultado.Dados?.Where(c => !c.Ativo).ToList() ?? new List<CategoriaDto>();

            ViewData["Title"] = "Categorias Desativadas";
            ViewData["Filtro"] = "desativadas";
            return View("Index", desativadas);
        }

        // ══════════════════════════════════════════════════════════════════════
        // CRIAÇÃO
        // ══════════════════════════════════════════════════════════════════════

        // GET /Categorias/Criar
        [HttpGet]
        public IActionResult Criar()
        {
            ViewData["Title"] = "Nova Categoria";
            return View(new CriarCategoriaDto("", null));
        }

        // POST /Categorias/Criar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(CriarCategoriaDto dto)
        {
            if (!ModelState.IsValid)
            {
                MensagemErro("Verifique os campos assinalados.");
                return View(dto);
            }

            var resultado = await _pdiService.CriarCategoriaAsync(dto);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return View(dto);
            }

            MensagemSucesso($"Categoria '{dto.Nome}' criada com sucesso!");
            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════════════
        // EDIÇÃO
        // ══════════════════════════════════════════════════════════════════════

        // GET /Categorias/Editar/5
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var resultado = await _pdiService.ObterCategoriaPorIdAsync(id);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return RedirectToAction(nameof(Index));
            }

            var categoria = resultado.Dados!;
            var dto = new AtualizarCategoriaDto(categoria.Nome, categoria.Descricao);

            ViewData["Title"] = "Editar Categoria";
            ViewBag.CategoriaAtual = categoria;
            ViewBag.Categorias = (await _pdiService.ObterCategoriasAtivasAsync()).Dados ?? new List<CategoriaDto>();
            return View(dto);
        }

        // POST /Categorias/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, AtualizarCategoriaDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Editar Categoria";
                ViewBag.CategoriaAtual = (await _pdiService.ObterCategoriaPorIdAsync(id)).Dados;
                ViewBag.Categorias = (await _pdiService.ObterCategoriasAtivasAsync()).Dados ?? new List<CategoriaDto>();
                return View(dto);
            }

            var resultado = await _pdiService.AtualizarCategoriaAsync(id, dto);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                ViewBag.CategoriaAtual = (await _pdiService.ObterCategoriaPorIdAsync(id)).Dados;
                ViewBag.Categorias = (await _pdiService.ObterCategoriasAtivasAsync()).Dados ?? new List<CategoriaDto>();
                return View(dto);
            }

            MensagemSucesso("Categoria atualizada com sucesso!");
            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════════════
        // ALTERNAR ESTADO
        // ══════════════════════════════════════════════════════════════════════

        // POST /Categorias/AlternarEstado/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarEstado(int id)
        {
            var resultado = await _pdiService.AlternarEstadoCategoriaAsync(id);

            if (!resultado.Sucesso)
                MensagemErro(resultado.Erro!);
            else
                MensagemSucesso("Estado da categoria alterado com sucesso!");

            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════════════
        // REMOÇÃO - Apenas ADMINISTRADOR
        // ══════════════════════════════════════════════════════════════════════

        // POST /Categorias/Remover/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remover(int id)
        {
            if (!VerificarPerfil("ADMINISTRADOR"))
            {
                MensagemErro("Apenas Administradores podem remover categorias.");
                return RedirectToAction(nameof(Index));
            }

            var resultado = await _pdiService.RemoverCategoriaAsync(id);

            if (!resultado.Sucesso)
                MensagemErro(resultado.Erro!);
            else
                MensagemSucesso("Categoria removida permanentemente.");

            return RedirectToAction(nameof(Index));
        }
    }
}