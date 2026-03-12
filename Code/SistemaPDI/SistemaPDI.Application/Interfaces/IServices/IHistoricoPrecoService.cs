using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Domain.Entities;

namespace SistemaPDI.Application.Interfaces.IServices
{
    public interface IHistoricoPrecoService
    {
        // ══════════════════════════════════════════════════════════════════════
        // CONSULTAS
        // ══════════════════════════════════════════════════════════════════════

        Task<Result<List<HistoricoPrecoDto>>> ObterTodosAsync();
        Task<Result<HistoricoPrecoDto>> ObterPorIdAsync(int id);
        Task<Result<List<HistoricoPrecoDto>>> ObterPorArtigoAsync(int artigoId);
        Task<Result<List<HistoricoPrecoDto>>> ObterPorFornecedorAsync(int fornecedorId);
        Task<Result<List<HistoricoPrecoDto>>> ObterPorEncomendaAsync(int encomendaId);
        Task<Result<List<HistoricoPrecoDto>>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);

        // ══════════════════════════════════════════════════════════════════════
        // ANÁLISES
        // ══════════════════════════════════════════════════════════════════════

        Task<Result<List<EvolucaoPrecoDto>>> ObterEvolucaoPrecosAsync(int artigoId);
        Task<Result<ComparacaoFornecedorDto>> CompararFornecedoresAsync(int artigoId);
        Task<Result<List<SugestaoPrecoDto>>> ObterSugestoesPrecosAsync(List<int> artigosIds, int? fornecedorId = null);

        // ══════════════════════════════════════════════════════════════════════
        // OPERAÇÕES (Chamadas internamente pela Receção)
        // ══════════════════════════════════════════════════════════════════════

        Task<Result<bool>> RegistarHistoricoDeEncomendaAsync(int encomendaId, string emailUtilizador);
    }
}