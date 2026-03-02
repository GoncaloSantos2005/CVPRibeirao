using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Web.Services;

namespace SistemaPDI.Web.Controllers
{
    public class FornecedoresController : BaseController
    {
        public FornecedoresController(IPdiApiService pdiService) : base(pdiService)
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

        // GET /Fornecedores
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var resultado = await _pdiService.ObterFornecedoresAsync();

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return View(new List<FornecedorDto>());
            }

            ViewData["Title"] = "Gestão de Fornecedores";
            return View(resultado.Dados ?? new List<FornecedorDto>());
        }

        // GET /Fornecedores/Preferenciais
        [HttpGet]
        public async Task<IActionResult> Preferenciais()
        {
            var resultado = await _pdiService.ObterFornecedoresAsync();

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return View("Index", new List<FornecedorDto>());
            }

            var preferenciais = resultado.Dados?.Where(f => f.Preferencial).ToList() ?? new List<FornecedorDto>();

            ViewData["Title"] = "Fornecedores Preferenciais";
            ViewData["Filtro"] = "preferenciais";
            return View("Index", preferenciais);
        }

        // GET /Fornecedores/Desativados
        [HttpGet]
        public async Task<IActionResult> Desativados()
        {
            if (!VerificarPerfil("ADMINISTRADOR", "GESTOR"))
            {
                MensagemErro("Não tens permissão para aceder a esta funcionalidade.");
                return RedirectToAction(nameof(Index));
            }

            var resultado = await _pdiService.ObterFornecedoresAsync(incluirInativos: true);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return View("Index", new List<FornecedorDto>());
            }

            var desativados = resultado.Dados?.Where(f => !f.Ativo).ToList() ?? new List<FornecedorDto>();

            ViewData["Title"] = "Fornecedores Desativados";
            ViewData["Filtro"] = "desativados";
            return View("Index", desativados);
        }

        // ══════════════════════════════════════════════════════════════════════
        // DETALHES
        // ══════════════════════════════════════════════════════════════════════

        // GET /Fornecedores/Detalhes/5
        [HttpGet]
        public async Task<IActionResult> Detalhes(int id)
        {
            var resultado = await _pdiService.ObterFornecedorPorIdAsync(id);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Detalhes do Fornecedor";
            return View(resultado.Dados);
        }

        // ══════════════════════════════════════════════════════════════════════
        // CRIAÇÃO
        // ══════════════════════════════════════════════════════════════════════

        // GET /Fornecedores/Criar
        [HttpGet]
        public IActionResult Criar()
        {
            ViewData["Title"] = "Novo Fornecedor";
            return View();
        }

        // POST /Fornecedores/Criar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(CriarFornecedorDto dto)
        {
            if (!ModelState.IsValid)
            {
                MensagemErro("Verifique os campos assinalados.");
                return View(dto);
            }

            var resultado = await _pdiService.CriarFornecedorAsync(dto);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return View(dto);
            }

            MensagemSucesso($"Fornecedor '{dto.Nome}' criado com sucesso!");
            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════════════
        // EDIÇÃO
        // ══════════════════════════════════════════════════════════════════════

        // GET /Fornecedores/Editar/5
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var resultado = await _pdiService.ObterFornecedorPorIdAsync(id);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return RedirectToAction(nameof(Index));
            }

            var fornecedor = resultado.Dados!;

            var dto = new AtualizarFornecedorDto(
                fornecedor.Nome,
                fornecedor.NIF,
                fornecedor.Email,
                fornecedor.Telefone,
                fornecedor.PessoaContacto,
                fornecedor.Morada,
                fornecedor.CodigoPostal,
                fornecedor.Localidade,
                fornecedor.TempoEntrega,
                fornecedor.Observacoes
            );

            ViewData["Title"] = "Editar Fornecedor";
            ViewBag.FornecedorAtual = fornecedor;
            return View(dto);
        }

        // POST /Fornecedores/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, AtualizarFornecedorDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Editar Fornecedor";
                ViewBag.FornecedorAtual = (await _pdiService.ObterFornecedorPorIdAsync(id)).Dados;
                return View(dto);
            }

            var resultado = await _pdiService.AtualizarFornecedorAsync(id, dto);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                ViewBag.FornecedorAtual = (await _pdiService.ObterFornecedorPorIdAsync(id)).Dados;
                return View(dto);
            }

            MensagemSucesso("Fornecedor atualizado com sucesso!");
            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════════════
        // ALTERNAR ESTADO
        // ══════════════════════════════════════════════════════════════════════

        // POST /Fornecedores/AlternarEstado/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarEstado(int id)
        {
            var resultado = await _pdiService.ToggleAtivoFornecedorAsync(id);

            if (!resultado.Sucesso)
                MensagemErro(resultado.Erro!);
            else
                MensagemSucesso("Estado do fornecedor alterado com sucesso!");

            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════════════
        // ALTERNAR PREFERENCIAL
        // ══════════════════════════════════════════════════════════════════════

        // POST /Fornecedores/AlternarPreferencial/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarPreferencial(int id)
        {
            var resultado = await _pdiService.TogglePreferencialFornecedorAsync(id);

            if (!resultado.Sucesso)
                MensagemErro(resultado.Erro!);
            else
                MensagemSucesso("Estado preferencial alterado com sucesso!");

            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════════════
        // REMOÇÃO - Apenas ADMINISTRADOR
        // ══════════════════════════════════════════════════════════════════════

        // POST /Fornecedores/Remover/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remover(int id)
        {
            if (!VerificarPerfil("ADMINISTRADOR"))
            {
                MensagemErro("Apenas Administradores podem remover fornecedores.");
                return RedirectToAction(nameof(Index));
            }

            var resultado = await _pdiService.ApagarFornecedorAsync(id);

            if (!resultado.Sucesso)
                MensagemErro(resultado.Erro!);
            else
                MensagemSucesso("Fornecedor removido permanentemente.");

            return RedirectToAction(nameof(Index));
        }
    }
}