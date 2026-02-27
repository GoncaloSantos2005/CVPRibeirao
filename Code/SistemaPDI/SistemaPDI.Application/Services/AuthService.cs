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
    public class AuthService : IAuthService
    {
        private readonly IUtilizadorRepository _utilizadorRepository;

        public AuthService(IUtilizadorRepository utilizadorRepository)
        {
            _utilizadorRepository = utilizadorRepository;
        }

        // CdU05.2 - Registar Utilizador
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
                utilizador.CriadoEm
            );
        }

        // CdU05.1 - Login (validar na BD)
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
                utilizador.CriadoEm
            );

            return new LoginResponseDto("", utilizadorDto);
        }
    }
}
