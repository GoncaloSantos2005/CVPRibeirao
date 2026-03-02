using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Domain.Entities;

namespace SistemaPDI.Application.Interfaces.IUtilizadorServices
{
    /// <summary>
    /// Interface do serviço de gestão de utilizadores.
    /// Define operações CRUD e gestão de contas.
    /// </summary>
    public interface IUtilizadorService
    {
        /// <summary>Obtém todos os utilizadores.</summary>
        Task<Result<List<UtilizadorDto>>> ObterTodosAsync();

        /// <summary>Obtém um utilizador por ID.</summary>
        Task<Result<UtilizadorDto>> ObterPorIdAsync(int id);

        /// <summary>Cria um novo utilizador.</summary>
        Task<Result<UtilizadorDto>> CriarAsync(RegistarUtilizadorDto dto);

        /// <summary>Atualiza os dados de um utilizador.</summary>
        Task<Result<UtilizadorDto>> AtualizarAsync(int id, AtualizarUtilizadorDto dto);

        /// <summary>Desativa a conta de um utilizador.</summary>
        Task<Result> DesativarAsync(int id);

        /// <summary>Ativa a conta de um utilizador.</summary>
        Task<Result> AtivarAsync(int id);

        /// <summary>Redefine a password de um utilizador.</summary>
        Task<Result> ResetPasswordAsync(int id, string novaPassword);
    }
}