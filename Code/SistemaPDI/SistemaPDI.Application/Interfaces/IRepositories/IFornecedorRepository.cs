using SistemaPDI.Domain.Entities;

namespace SistemaPDI.Application.Interfaces.IRepositories
{
    public interface IFornecedorRepository
    {
        Task<Fornecedor?> ObterPorIdAsync(int id);
        Task<Fornecedor?> ObterPorNIFAsync(string nif);
        Task<Fornecedor?> ObterPorNomeAsync(string nome);
        Task<List<Fornecedor>> ObterTodosAsync(bool incluirInativos = false);
        Task<List<Fornecedor>> ObterAtivosAsync();
        Task<List<Fornecedor>> ObterPreferenciaisAsync();
        Task AdicionarAsync(Fornecedor fornecedor);
        Task AtualizarAsync(Fornecedor fornecedor);
        Task<bool> NIFJaExisteAsync(string nif, int? ignorarId = null);
        Task<bool> NomeJaExisteAsync(string nome, int? ignorarId = null);
        Task<int> ContarEncomendasAsync(int fornecedorId);
        Task<int> SaveChangesAsync();
    }
}