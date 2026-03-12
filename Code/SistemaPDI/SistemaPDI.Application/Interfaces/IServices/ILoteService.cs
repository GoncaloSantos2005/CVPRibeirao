using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Domain.Entities;

namespace SistemaPDI.Application.Interfaces.IServices
{
    /// <summary>
    /// Interface do serviço de gestão de lotes.
    /// Implementa as regras de negócio RN03, RN04, RN05, RN06, RN13, RN17.
    /// </summary>
    public interface ILoteService
    {
        // ── LEITURA ───────────────────────────────────────────────────────────

        /// <summary>Obtém todos os lotes.</summary>
        Task<Result<List<LoteDto>>> ObterTodosAsync();

        /// <summary>Obtém um lote por ID.</summary>
        Task<Result<LoteDto>> ObterPorIdAsync(int id);

        /// <summary>Obtém todos os lotes de um artigo.</summary>
        Task<Result<List<LoteDto>>> ObterPorArtigoAsync(int artigoId);

        // ── ESCRITA ───────────────────────────────────────────────────────────

        /// <summary>
        /// Cria um novo lote durante a receção de stock.
        /// Valida RN06 (duplicado) e RN17 (data validade).
        /// </summary>
        Task<Result<LoteDto>> CriarAsync(CriarLoteDto dto);

        /// <summary>Atualiza um lote existente.</summary>
        Task<Result<LoteDto>> AtualizarAsync(int id, AtualizarLoteDto dto);

        /// <summary>Desativa um lote.</summary>
        Task<Result> DesativarAsync(int id);

        // ── FEFO - RESERVAS (RN03, RN04, RN05) ────────────────────────────────

        /// <summary>
        /// Reserva stock usando algoritmo FEFO (RN03, RN04).
        /// Aloca de múltiplos lotes se necessário, ordenados por validade.
        /// </summary>
        Task<Result<ResultadoReservaDto>> ReservarStockFEFOAsync(ReservaStockDto dto);

        /// <summary>
        /// Liberta reservas de um lote específico (RN05 - timeout/cancelamento).
        /// </summary>
        Task<Result> LibertarReservaAsync(LibertarReservaDto dto);

        /// <summary>
        /// Liberta todas as reservas expiradas (job automático - RN05).
        /// </summary>
        Task<Result<int>> LibertarReservasExpiradasAsync();

        /// <summary>
        /// Confirma a saída de stock (converte reservas em saídas definitivas).
        /// </summary>
        Task<Result> ConfirmarSaidaAsync(int loteId, int quantidade);

        // ── STOCK ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Calcula o stock total disponível de um artigo (soma de todos os lotes).
        /// </summary>
        Task<Result<int>> CalcularStockDisponivelAsync(int artigoId);
    }
}