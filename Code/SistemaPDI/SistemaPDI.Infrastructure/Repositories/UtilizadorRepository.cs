using Microsoft.EntityFrameworkCore;
using SistemaPDI.Application.Interfaces.IRepositories;
using SistemaPDI.Domain.Entities;
using SistemaPDI.Infrastructure.Data;

namespace SistemaPDI.Infrastructure.Repositories
{
    /// <summary>
    /// Repositório de acesso a dados para a entidade Utilizador.
    /// Implementa operações CRUD na base de dados.
    /// </summary>
    public class UtilizadorRepository : IUtilizadorRepository
    {
        private readonly PdiDbContext _context;

        /// <summary>
        /// Inicializa o repositório com o contexto da base de dados.
        /// </summary>
        /// <param name="context">Contexto Entity Framework.</param>
        public UtilizadorRepository(PdiDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém um utilizador pelo email (case-insensitive).
        /// </summary>
        /// <param name="email">Email a pesquisar.</param>
        /// <returns>Utilizador encontrado ou null.</returns>
        public async Task<Utilizador?> ObterPorEmailAsync(string email)
        {
            return await _context.Utilizadores
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        /// <summary>
        /// Obtém um utilizador pelo seu ID.
        /// </summary>
        /// <param name="id">Identificador do utilizador.</param>
        /// <returns>Utilizador encontrado ou null.</returns>
        public async Task<Utilizador?> ObterPorIdAsync(int id)
        {
            return await _context.Utilizadores.FindAsync(id);
        }

        /// <summary>
        /// Obtém todos os utilizadores ordenados por nome.
        /// </summary>
        /// <returns>Lista completa de utilizadores (ativos e inativos).</returns>
        public async Task<List<Utilizador>> ObterTodosAsync()
        {
            return await _context.Utilizadores
                .OrderBy(u => u.NomeCompleto)
                .ToListAsync();
        }

        /// <summary>
        /// Adiciona um novo utilizador à base de dados.
        /// </summary>
        /// <param name="utilizador">Entidade a adicionar.</param>
        public async Task AdicionarAsync(Utilizador utilizador)
        {
            await _context.Utilizadores.AddAsync(utilizador);
        }

        /// <summary>
        /// Marca um utilizador como modificado para atualização.
        /// </summary>
        /// <param name="utilizador">Entidade a atualizar.</param>
        public async Task AtualizarAsync(Utilizador utilizador)
        {
            _context.Utilizadores.Update(utilizador);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Verifica se já existe um utilizador com o email especificado.
        /// </summary>
        /// <param name="email">Email a verificar.</param>
        /// <returns>True se o email já existir.</returns>
        public async Task<bool> EmailJaExisteAsync(string email)
        {
            return await _context.Utilizadores
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        /// <summary>
        /// Desativa um utilizador pelo ID.
        /// </summary>
        /// <param name="id">ID do utilizador.</param>
        /// <returns>True se desativado com sucesso.</returns>
        public async Task<bool> DesativarAsync(int id)
        {
            var utilizador = await _context.Utilizadores.FindAsync(id);
            if (utilizador == null) return false;

            utilizador.Ativo = false;
            _context.Utilizadores.Update(utilizador);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Persiste todas as alterações pendentes na base de dados.
        /// </summary>
        /// <returns>Número de registos afetados.</returns>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
