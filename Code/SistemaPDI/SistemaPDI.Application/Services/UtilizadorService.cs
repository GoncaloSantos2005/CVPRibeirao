using SistemaPDI.Application.Interfaces.IRepositories;
using SistemaPDI.Application.Interfaces.IUtilizadorServices;
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Domain.Entities;

namespace SistemaPDI.Application.Services
{
    /// <summary>
    /// Serviço de gestão de utilizadores.
    /// Fornece operações CRUD e gestão de contas (ativar/desativar/reset password).
    /// </summary>
    public class UtilizadorService : IUtilizadorService
    {
        private readonly IUtilizadorRepository _utilizadorRepository;

        /// <summary>
        /// Inicializa o serviço de utilizadores.
        /// </summary>
        /// <param name="utilizadorRepository">Repositório de utilizadores.</param>
        public UtilizadorService(IUtilizadorRepository utilizadorRepository)
        {
            _utilizadorRepository = utilizadorRepository;
        }

        /// <summary>
        /// Obtém todos os utilizadores do sistema.
        /// </summary>
        /// <returns>Lista de utilizadores (ativos e inativos).</returns>
        public async Task<Result<List<UtilizadorDto>>> ObterTodosAsync()
        {
            var utilizadores = await _utilizadorRepository.ObterTodosAsync();
            var dtos = utilizadores.Select(u => MapToDto(u)).ToList();
            return Result<List<UtilizadorDto>>.Ok(dtos);
        }

        /// <summary>
        /// Obtém um utilizador pelo seu identificador.
        /// </summary>
        /// <param name="id">ID do utilizador.</param>
        /// <returns>Dados do utilizador ou erro se não encontrado.</returns>
        public async Task<Result<UtilizadorDto>> ObterPorIdAsync(int id)
        {
            var utilizador = await _utilizadorRepository.ObterPorIdAsync(id);
            if (utilizador == null)
                return Result<UtilizadorDto>.Falhou("Utilizador nao encontrado.");

            return Result<UtilizadorDto>.Ok(MapToDto(utilizador));
        }

        /// <summary>
        /// Cria um novo utilizador no sistema.
        /// </summary>
        /// <param name="dto">Dados do novo utilizador.</param>
        /// <returns>Utilizador criado ou erro se o email já existir.</returns>
        /// <remarks>
        /// - Password encriptada com BCrypt (custo 11).
        /// - Email normalizado para minúsculas.
        /// </remarks>
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

        /// <summary>
        /// Atualiza os dados de um utilizador existente.
        /// </summary>
        /// <param name="id">ID do utilizador.</param>
        /// <param name="dto">Novos dados (nome, perfil, estado).</param>
        /// <returns>Utilizador atualizado ou erro se não encontrado.</returns>
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

        /// <summary>
        /// Desativa a conta de um utilizador (soft delete).
        /// </summary>
        /// <param name="id">ID do utilizador.</param>
        /// <returns>Sucesso ou erro se não encontrado.</returns>
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

        /// <summary>
        /// Reativa a conta de um utilizador desativado.
        /// </summary>
        /// <param name="id">ID do utilizador.</param>
        /// <returns>Sucesso ou erro se não encontrado.</returns>
        public async Task<Result> AtivarAsync(int id)
        {
            var utilizador = await _utilizadorRepository.ObterPorIdAsync(id);
            if (utilizador == null)
                return Result.Falhou("Utilizador nao encontrado.");

            utilizador.Ativo = true;
            await _utilizadorRepository.AtualizarAsync(utilizador);
            await _utilizadorRepository.SaveChangesAsync();

            return Result.Ok();
        }

        /// <summary>
        /// Redefine a password de um utilizador (apenas administradores).
        /// </summary>
        /// <param name="id">ID do utilizador.</param>
        /// <param name="novaPassword">Nova password (mínimo 6 caracteres).</param>
        /// <returns>Sucesso ou erro de validação.</returns>
        public async Task<Result> ResetPasswordAsync(int id, string novaPassword)
        {
            var utilizador = await _utilizadorRepository.ObterPorIdAsync(id);
            if (utilizador == null)
                return Result.Falhou("Utilizador nao encontrado.");

            if (string.IsNullOrWhiteSpace(novaPassword) || novaPassword.Length < 6)
                return Result.Falhou("A password deve ter pelo menos 6 caracteres.");

            utilizador.PasswordHash = BCrypt.Net.BCrypt.HashPassword(novaPassword, 11);
            await _utilizadorRepository.AtualizarAsync(utilizador);
            await _utilizadorRepository.SaveChangesAsync();

            return Result.Ok();
        }

        /// <summary>
        /// Mapeia uma entidade Utilizador para DTO.
        /// </summary>
        /// <param name="u">Entidade Utilizador.</param>
        /// <returns>DTO com dados públicos do utilizador.</returns>
        private static UtilizadorDto MapToDto(Utilizador u) => new(
            u.Id,
            u.Email,
            u.NomeCompleto,
            u.Perfil.ToString(),
            u.Ativo,
            u.CriadoEm,
            u.UltimoLogin
        );
    }
}