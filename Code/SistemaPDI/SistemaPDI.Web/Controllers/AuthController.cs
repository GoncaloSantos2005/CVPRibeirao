using Microsoft.AspNetCore.Mvc;
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Web.Services;
using System.Text.Json;

namespace SistemaPDI.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IPdiApiService _pdiService;

        public AuthController(IPdiApiService pdiService)
        {
            _pdiService = pdiService;
        }

        // GET /Auth/Login
        public IActionResult Login() => View();

        // POST /Auth/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var resultado = await _pdiService.LoginAsync(dto);

            if (resultado == null)
            {
                ModelState.AddModelError("", "Email ou password inválidos");
                return View(dto);
            }

            HttpContext.Session.SetString("Token", resultado.Token);
            HttpContext.Session.SetString("Utilizador",
                JsonSerializer.Serialize(resultado.Utilizador));

            TempData["Sucesso"] = "Login efetuado com sucesso!";
            return RedirectToAction("Index", "Dashboard");
        }

        // GET /Auth/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Sucesso"] = "Sessão terminada com sucesso!";
            return RedirectToAction("Login");
        }
    }
}