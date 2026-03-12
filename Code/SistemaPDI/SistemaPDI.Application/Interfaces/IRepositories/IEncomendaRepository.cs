using SistemaPDI.Domain.Entities;
using SistemaPDI.Domain.Enums;

namespace SistemaPDI.Application.Interfaces.IRepositories
{
    public interface IEncomendaRepository
    {
        Task<Encomenda?> ObterPorIdAsync(int id);
        Task<Encomenda?> ObterPorNumeroAsync(string numeroEncomenda);
        Task<List<Encomenda>> ObterTodosAsync(bool incluirInativos = false);
        Task<List<Encomenda>> ObterPorFornecedorAsync(int fornecedorId);
        Task<List<Encomenda>> ObterPorEstadoAsync(EstadoEncomenda estado);
        Task<List<Encomenda>> ObterPendentesAsync();
        Task AdicionarAsync(Encomenda encomenda);
        Task AtualizarAsync(Encomenda encomenda);
        Task<bool> NumeroJaExisteAsync(string numeroEncomenda);
        Task<string> GerarProximoNumeroAsync();
        Task<int> SaveChangesAsync();
    }
}