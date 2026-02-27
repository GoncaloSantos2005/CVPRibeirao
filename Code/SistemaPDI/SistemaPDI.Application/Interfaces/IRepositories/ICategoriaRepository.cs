using SistemaPDI.Domain.Entities;

namespace SistemaPDI.Application.Interfaces.IRepositories
{
    public interface ICategoriaRepository
    {
        Task<Categoria?> ObterPorIdAsync(int id);
        Task<Categoria?> ObterPorNomeAsync(string nome);
        Task<List<Categoria>> ObterTodosAsync(bool incluirInativos = false);
        Task<List<Categoria>> ObterAtivasAsync();
        Task AdicionarAsync(Categoria categoria);
        Task AtualizarAsync(Categoria categoria);
        Task<bool> NomeJaExisteAsync(string nome, int? ignorarId = null);
        Task<int> ContarArtigosPorCategoriaAsync(int categoriaId);
        Task<int> SaveChangesAsync();
    }
}