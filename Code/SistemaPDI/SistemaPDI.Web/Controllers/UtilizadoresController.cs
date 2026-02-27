using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Web.Services;

namespace SistemaPDI.Web.Controllers
{
    public class UtilizadoresController : BaseController
    {
        public UtilizadoresController(IPdiApiService pdiService) : base(pdiService)
        {
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            RequererPerfil(context, "ADMINISTRADOR");
        }

        // GET /Utilizadores
        public async Task<IActionResult> Index()
        {
            var resultado = await _pdiService.ObterUtilizadoresAsync();
            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return View(new List<UtilizadorDto>());
            }
            return View(resultado.Dados);
        }

        // GET /Utilizadores/Criar
        public IActionResult Criar() => View(new RegistarUtilizadorDto("", "", "", default));

        // POST /Utilizadores/Criar
        [HttpPost]
        public async Task<IActionResult> Criar(RegistarUtilizadorDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var resultado = await _pdiService.CriarUtilizadorAsync(dto);
            if (!resultado.Sucesso)
            {
                ModelState.AddModelError("Email", resultado.Erro!);
                return View(dto);
            }

            MensagemSucesso("Utilizador criado com sucesso!");
            return RedirectToAction(nameof(Index));
        }

        // GET /Utilizadores/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            var resultado = await _pdiService.ObterUtilizadorPorIdAsync(id);
            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return RedirectToAction(nameof(Index));
            }

            var utilizador = resultado.Dados!;
            var dto = new AtualizarUtilizadorDto(
                utilizador.NomeCompleto,
                Enum.Parse<SistemaPDI.Domain.Enums.Perfil>(utilizador.Perfil),
                utilizador.Ativo
            );
            ViewBag.UtilizadorId = id;
            ViewBag.Email = utilizador.Email;
            return View(dto);
        }

        // POST /Utilizadores/Editar/5
        [HttpPost]
        public async Task<IActionResult> Editar(int id, AtualizarUtilizadorDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UtilizadorId = id;
                var u = await _pdiService.ObterUtilizadorPorIdAsync(id);
                ViewBag.Email = u.Dados?.Email;
                return View(dto);
            }

            var resultado = await _pdiService.AtualizarUtilizadorAsync(id, dto);
            if (!resultado.Sucesso)
            {
                ModelState.AddModelError("", resultado.Erro!);
                ViewBag.UtilizadorId = id;
                var u = await _pdiService.ObterUtilizadorPorIdAsync(id);
                ViewBag.Email = u.Dados?.Email;
                return View(dto);
            }

            MensagemSucesso("Utilizador atualizado com sucesso!");
            return RedirectToAction(nameof(Index));
        }

        // POST /Utilizadores/Desativar/5
        [HttpPost]
        public async Task<IActionResult> Desativar(int id)
        {
            var resultado = await _pdiService.DesativarUtilizadorAsync(id);
            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return RedirectToAction(nameof(Index));
            }

            MensagemSucesso("Utilizador desativado com sucesso!");
            return RedirectToAction(nameof(Index));
        }
    }
}