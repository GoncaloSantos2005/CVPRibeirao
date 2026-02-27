using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SistemaPDI.Web.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SistemaPDI.Web.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IPdiApiService _pdiService;

        public BaseController(IPdiApiService pdiService)
        {
            _pdiService = pdiService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var token = HttpContext.Session.GetString("Token");

            if (string.IsNullOrEmpty(token))
            {
                TempData["Erro"] = "Tens de fazer login para aceder a esta página.";
                context.Result = RedirectToAction("Login", "Auth");
                return;
            }

            // Validar se o token não expirou
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            if (jwt.ValidTo < DateTime.UtcNow)
            {
                HttpContext.Session.Clear();
                TempData["Erro"] = "A sessão expirou. Faz login novamente.";
                context.Result = RedirectToAction("Login", "Auth");
                return;
            }

            // Ler claims do JWT — fonte de verdade
            var perfil = jwt.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var nome = jwt.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var email = jwt.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            ViewBag.Perfil = perfil;
            ViewBag.NomeUtilizador = nome;
            ViewBag.Email = email;

            _pdiService.ConfigurarToken(token);
            base.OnActionExecuting(context);
        }

        protected void RequererPerfil(ActionExecutingContext context, params string[] perfis)
        {
            if (context.Result != null) return;

            var perfil = ViewBag.Perfil as string;
            if (!perfis.Contains(perfil))
            {
                TempData["Erro"] = "Não tens permissões para aceder a esta área.";
                context.Result = RedirectToAction("Index", "Dashboard");
            }
        }

        /// <summary>
        /// Verifica se o utilizador atual possui um dos perfis especificados.
        /// Use dentro de actions individuais para controlo de acesso granular.
        /// </summary>
        protected bool VerificarPerfil(params string[] perfis)
        {
            var perfil = ViewBag.Perfil as string;
            return perfis.Contains(perfil);
        }

        protected void MensagemSucesso(string mensagem) =>
            TempData["Sucesso"] = mensagem;

        protected void MensagemErro(string mensagem) =>
            TempData["Erro"] = mensagem;
    }
}