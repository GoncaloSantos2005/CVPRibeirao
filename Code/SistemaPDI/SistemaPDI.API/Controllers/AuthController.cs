using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Application.Interfaces.IUtilizadorServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SistemaPDI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        // POST /api/auth/register
        // CdU05.2 - Registar Utilizador
        [HttpPost("register")]
        [Authorize(Roles = "ADMINISTRADOR")] // Só admins podem criar utilizadores
        public async Task<IActionResult> Register([FromBody] RegistarUtilizadorDto dto)
        {
            var utilizador = await _authService.RegistarAsync(dto);

            if (utilizador == null)
                return BadRequest(new { message = "Email já existe" });

            return Ok(new { message = "Utilizador criado com sucesso", utilizador });
        }

        // POST /api/auth/login
        // CdU05.1 - Autenticar Utilizador
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (result == null)
                return Unauthorized(new { message = "Email ou senha inválidos" });

            // Gerar JWT Token
            var token = GerarTokenJWT(result.Utilizador);

            return Ok(new
            {
                token,
                utilizador = result.Utilizador
            });
        }

        // Método auxiliar para gerar JWT
        private string GerarTokenJWT(UtilizadorDto utilizador)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, utilizador.Id.ToString()),
                new Claim(ClaimTypes.Email, utilizador.Email),
                new Claim(ClaimTypes.Name, utilizador.NomeCompleto),
                new Claim(ClaimTypes.Role, utilizador.Perfil)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(jwtSettings["ExpiryMinutes"]!)),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}