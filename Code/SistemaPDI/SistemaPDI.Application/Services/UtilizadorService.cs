using SistemaPDI.Application.Interfaces.IRepositories;
using SistemaPDI.Application.Interfaces.IUtilizadorServices;
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Domain.Entities;

namespace SistemaPDI.Application.Services
{
    public class UtilizadorService : IUtilizadorService
    {
        private readonly IUtilizadorRepository _utilizadorRepository;

        public UtilizadorService(IUtilizadorRepository utilizadorRepository)
        {
            _utilizadorRepository = utilizadorRepository;
        }

        public async Task<Result<List<UtilizadorDto>>> ObterTodosAsync()
        {
            var utilizadores = await _utilizadorRepository.ObterTodosAsync();
            var dtos = utilizadores.Select(u => MapToDto(u)).ToList();
            return Result<List<UtilizadorDto>>.Ok(dtos);
        }

        public async Task<Result<UtilizadorDto>> ObterPorIdAsync(int id)
        {
            var utilizador = await _utilizadorRepository.ObterPorIdAsync(id);
            if (utilizador == null)
                return Result<UtilizadorDto>.Falhou("Utilizador nao encontrado.");

            return Result<UtilizadorDto>.Ok(MapToDto(utilizador));
        }

        public async Task<Result<UtilizadorDto>> CriarAsync(RegistarUtilizadorDto dto)
        {
            if (await _utilizadorRepository.EmailJaExisteAsync(dto.Email))
                return Result<UtilizadorDto>.Falhou("Ja existe um utilizador com este email.");

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

            return Result<UtilizadorDto>.Ok(MapToDto(utilizador));
        }

        public async Task<Result<UtilizadorDto>> AtualizarAsync(int id, AtualizarUtilizadorDto dto)
        {
            var utilizador = await _utilizadorRepository.ObterPorIdAsync(id);
            if (utilizador == null)
                return Result<UtilizadorDto>.Falhou("Utilizador nao encontrado.");

            utilizador.NomeCompleto = dto.NomeCompleto.Trim();
            utilizador.Perfil = dto.Perfil;
            utilizador.Ativo = dto.Ativo;

            await _utilizadorRepository.AtualizarAsync(utilizador);
            await _utilizadorRepository.SaveChangesAsync();

            return Result<UtilizadorDto>.Ok(MapToDto(utilizador));
        }

        public async Task<Result> DesativarAsync(int id)
        {
            var utilizador = await _utilizadorRepository.ObterPorIdAsync(id);
            if (utilizador == null)
                return Result.Falhou("Utilizador nao encontrado.");

            utilizador.Ativo = false;
            await _utilizadorRepository.AtualizarAsync(utilizador);
            await _utilizadorRepository.SaveChangesAsync();

            return Result.Ok();
        }

        // ── Metodo privado de mapeamento ─────────────────────────────────────
        private static UtilizadorDto MapToDto(Utilizador u) => new(
            u.Id,
            u.Email,
            u.NomeCompleto,
            u.Perfil.ToString(),
            u.Ativo,
            u.CriadoEm
        );
    }
}