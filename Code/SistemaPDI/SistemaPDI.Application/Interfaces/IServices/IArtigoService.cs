using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Domain.Entities;

namespace SistemaPDI.Application.Interfaces.IServices
{
    public interface IArtigoService
    {
        // ── LEITURA ───────────────────────────────────────────────────────────
        Task<Result<List<ArtigoDto>>> ObterTodosAsync();
        Task<Result<ArtigoDto>> ObterPorIdAsync(int id);
        Task<Result<List<ArtigoDto>>> ObterComStockBaixoAsync();     
        Task<Result<List<ArtigoDto>>> ObterComStockCriticoAsync();
        Task<Result<List<ArtigoDto>>> ObterDesativadosAsync();

        // ── ESCRITA ───────────────────────────────────────────────────────────
        Task<Result<ArtigoDto>> CriarAsync(CriarArtigoDto dto);
        Task<Result> AtualizarAsync(int id, AtualizarArtigoDto dto);
        Task<Result> AlternarEstadoAtivoAsync(int id, string nomeUtilizador);

        // ── STOCK (verificação individual) ────────────────────────────────────
        Task<Result<int>> CalcularStockVirtualAsync(int artigoId);
        Task<Result> AtualizarStockFisicoAsync(int artigoId, int quantidade);
        Task<Result<bool>> VerificarNecessidadeReposicaoAsync(int artigoId);
        Task<Result<bool>> VerificarStockCriticoAsync(int artigoId);
        Task<Result<int>> SugerirQuantidadeEncomendaAsync(int artigoId);
        Task<Result> AtualizarPrecoMedioAsync(int artigoId, int qtdEntrada, decimal precoEntrada);
    }
}