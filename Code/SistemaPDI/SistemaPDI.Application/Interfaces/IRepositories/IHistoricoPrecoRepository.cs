using SistemaPDI.Domain.Entities;

namespace SistemaPDI.Application.Interfaces.IRepositories
{
    public interface IHistoricoPrecoRepository
    {
        // ══════════════════════════════════════════════════════════════════════
        // CONSULTAS
        // ══════════════════════════════════════════════════════════════════════

        Task<List<HistoricoPreco>> ObterTodosAsync();
        Task<HistoricoPreco?> ObterPorIdAsync(int id);
        Task<List<HistoricoPreco>> ObterPorArtigoAsync(int artigoId);
        Task<List<HistoricoPreco>> ObterPorFornecedorAsync(int fornecedorId);
        Task<List<HistoricoPreco>> ObterPorEncomendaAsync(int encomendaId);
        Task<List<HistoricoPreco>> ObterPorArtigoEFornecedorAsync(int artigoId, int fornecedorId);
        Task<List<HistoricoPreco>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);

        // Consultas específicas
        Task<HistoricoPreco?> ObterUltimoPrecoAsync(int artigoId, int? fornecedorId = null);
        Task<decimal?> ObterPrecoMedioAsync(int artigoId, int? fornecedorId = null);

        // ══════════════════════════════════════════════════════════════════════
        // OPERAÇÕES
        // ══════════════════════════════════════════════════════════════════════

        Task AdicionarAsync(HistoricoPreco historicoPreco);
        Task AdicionarVariosAsync(List<HistoricoPreco> historicos);
        Task<int> SaveChangesAsync();
    }
}