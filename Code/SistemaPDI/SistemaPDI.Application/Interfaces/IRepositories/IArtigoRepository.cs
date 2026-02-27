using SistemaPDI.Domain.Entities;

namespace SistemaPDI.Application.Interfaces.IRepositories
{
    public interface IArtigoRepository
    {
        Task<Artigo?> ObterPorIdAsync(int id);
        Task<List<Artigo>> ObterTodosAsync();
        Task<List<Artigo>> ObterComStockBaixoAsync();
        Task<List<Artigo>> ObterComStockCriticoAsync();
        void ToggleAtivo(Artigo artigo, string utilizador);
        Task AdicionarAsync(Artigo artigo);
        Task AtualizarAsync(Artigo artigo);
        void Remover(Artigo artigo);
        Task<int> SaveChangesAsync();  
        Task<List<Artigo>> ObterAtivosAsync();
        Task<List<Artigo>> ObterDesativadosAsync();
    }
}
