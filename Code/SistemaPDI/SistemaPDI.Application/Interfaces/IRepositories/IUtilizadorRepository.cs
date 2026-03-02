using SistemaPDI.Domain.Entities;

namespace SistemaPDI.Application.Interfaces.IRepositories
{
    /// <summary>
    /// Interface do repositório de utilizadores.
    /// Define o contrato para operações de acesso a dados.
    /// </summary>
    public interface IUtilizadorRepository
    {
        /// <summary>Obtém um utilizador pelo email.</summary>
        Task<Utilizador?> ObterPorEmailAsync(string email);

        /// <summary>Obtém um utilizador pelo ID.</summary>
        Task<Utilizador?> ObterPorIdAsync(int id);

        /// <summary>Obtém todos os utilizadores.</summary>
        Task<List<Utilizador>> ObterTodosAsync();

        /// <summary>Adiciona um novo utilizador.</summary>
        Task AdicionarAsync(Utilizador utilizador);

        /// <summary>Atualiza um utilizador existente.</summary>
        Task AtualizarAsync(Utilizador utilizador);

        /// <summary>Verifica se o email já existe.</summary>
        Task<bool> EmailJaExisteAsync(string email);

        /// <summary>Persiste as alterações na base de dados.</summary>
        Task<int> SaveChangesAsync();

        /// <summary>Desativa um utilizador pelo ID.</summary>
        Task<bool> DesativarAsync(int id);
    }
}
