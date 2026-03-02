using SistemaPDI.Domain.Entities;

namespace SistemaPDI.Application.Interfaces.IRepositories
{
    /// <summary>
    /// Interface do repositório de lotes.
    /// </summary>
    public interface ILoteRepository
    {
        /// <summary>Obtém todos os lotes ativos.</summary>
        Task<List<Lote>> ObterTodosAsync();

        /// <summary>Obtém um lote por ID.</summary>
        Task<Lote?> ObterPorIdAsync(int id);

        /// <summary>Obtém um lote por ID com dados do Artigo.</summary>
        Task<Lote?> ObterPorIdComArtigoAsync(int id);

        /// <summary>Obtém todos os lotes de um artigo.</summary>
        Task<List<Lote>> ObterPorArtigoIdAsync(int artigoId);

        /// <summary>
        /// Obtém lotes disponíveis para FEFO (RN03).
        /// Ordenados por DataValidade ASC, excluindo expirados.
        /// </summary>
        Task<List<Lote>> ObterLotesParaFEFOAsync(int artigoId);

        /// <summary>
        /// Verifica se já existe um lote com o mesmo número para o artigo (RN06).
        /// </summary>
        Task<bool> NumeroLoteJaExisteAsync(int artigoId, string numeroLote);

        /// <summary>
        /// Obtém lotes com validade próxima ou expirados (RN13).
        /// </summary>
        Task<List<Lote>> ObterLotesComValidadeProximaAsync(int diasAlerta = 15);

        /// <summary>
        /// Obtém lotes com reservas expiradas (timeout > 2 horas).
        /// </summary>
        Task<List<Lote>> ObterLotesComReservasExpiradasAsync();

        /// <summary>Adiciona um novo lote.</summary>
        Task AdicionarAsync(Lote lote);

        /// <summary>Atualiza um lote existente.</summary>
        Task AtualizarAsync(Lote lote);

        /// <summary>Atualiza múltiplos lotes (usado em FEFO).</summary>
        Task AtualizarVariosAsync(IEnumerable<Lote> lotes);

        /// <summary>Persiste alterações na BD.</summary>
        Task<int> SaveChangesAsync();
    }
}