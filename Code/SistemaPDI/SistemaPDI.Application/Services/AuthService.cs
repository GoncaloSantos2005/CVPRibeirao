using BCrypt.Net;
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Application.Interfaces.IRepositories;
using SistemaPDI.Application.Interfaces.IUtilizadorServices;
using SistemaPDI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaPDI.Application.Services
{
    /// <summary>
    /// Serviço responsável pela autenticação e registo de utilizadores.
    /// Implementa os casos de uso CdU05.1 (Login) e CdU05.2 (Registo).
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUtilizadorRepository _utilizadorRepository;

        /// <summary>
        /// Inicializa o serviço de autenticação.
        /// </summary>
        /// <param name="utilizadorRepository">Repositório de utilizadores.</param>
        public AuthService(IUtilizadorRepository utilizadorRepository)
        {
            _utilizadorRepository = utilizadorRepository;
        }

        /// <summary>
        /// Regista um novo utilizador no sistema (CdU05.2).
        /// </summary>
        /// <param name="dto">Dados do novo utilizador.</param>
        /// <returns>DTO do utilizador criado ou null se o email já existir.</returns>
        /// <remarks>
        /// - O email é normalizado (minúsculas e sem espaços).
        /// - A password é encriptada com BCrypt (custo 11).
        /// - O utilizador é criado como ativo por defeito.
        /// </remarks>
        public async Task<UtilizadorDto?> RegistarAsync(RegistarUtilizadorDto dto)
        {
            if (await _utilizadorRepository.EmailJaExisteAsync(dto.Email))
                return null; 

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, 11);

            var utilizador = new Utilizador
            {
                Email = dto.Email.ToLower().Trim(),
                NomeCompleto = dto.NomeCompleto.Trim(),
                PasswordHash = passwordHash,
                Perfil = dto.Perfil,
                Ativo = true,
                CriadoEm = DateTime.UtcNow
            };

            await _utilizadorRepository.AdicionarAsync(utilizador);
            await _utilizadorRepository.SaveChangesAsync();

            return new UtilizadorDto(
                utilizador.Id,
                utilizador.Email,
                utilizador.NomeCompleto,
                utilizador.Perfil.ToString(),
                utilizador.Ativo,
                utilizador.CriadoEm,
                utilizador.UltimoLogin
            );
        }

        /// <summary>
        /// Autentica um utilizador no sistema (CdU05.1).
        /// </summary>
        /// <param name="dto">Credenciais de login.</param>
        /// <returns>Resposta com token e dados do utilizador, ou null se falhar.</returns>
        /// <remarks>
        /// Validações realizadas:
        /// - Verifica se o email existe na base de dados.
        /// - Valida a password com BCrypt.
        /// - Verifica se a conta está ativa.
        /// - Atualiza a data do último login em caso de sucesso.
        /// </remarks>
        public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
        {
            var utilizador = await _utilizadorRepository.ObterPorEmailAsync(dto.Email);

            if (utilizador == null)
                return null;

            var senhaValida = BCrypt.Net.BCrypt.Verify(dto.Password, utilizador.PasswordHash);

            if (!senhaValida)
                return null;

            if (!utilizador.Ativo)
                return null;

            utilizador.UltimoLogin = DateTime.UtcNow;
            await _utilizadorRepository.AtualizarAsync(utilizador);
            await _utilizadorRepository.SaveChangesAsync();

            var utilizadorDto = new UtilizadorDto(
                utilizador.Id,
                utilizador.Email,
                utilizador.NomeCompleto,
                utilizador.Perfil.ToString(),
                utilizador.Ativo,
                utilizador.CriadoEm,
                utilizador.UltimoLogin
            );

            return new LoginResponseDto("", utilizadorDto);
        }
    }
}
