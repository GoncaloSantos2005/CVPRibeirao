using Microsoft.EntityFrameworkCore;
using SistemaPDI.Application.Interfaces.IRepositories;
using SistemaPDI.Domain.Entities;
using SistemaPDI.Infrastructure.Data;

namespace SistemaPDI.Infrastructure.Repositories
{
    /// <summary>
    /// Repositório de acesso a dados para a entidade Lote.
    /// </summary>
    public class LoteRepository : ILoteRepository
    {
        private readonly PdiDbContext _context;

        public LoteRepository(PdiDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém todos os lotes ativos com dados do artigo.
        /// </summary>
        public async Task<List<Lote>> ObterTodosAsync()
        {
            return await _context.Lotes
                .Include(l => l.Artigo)
                .Include(l => l.Localizacao)
                .Where(l => l.Ativo)
                .OrderBy(l => l.DataValidade)
                .ToListAsync();
        }

        /// <summary>
        /// Obtém um lote por ID.
        /// </summary>
        public async Task<Lote?> ObterPorIdAsync(int id)
        {
            return await _context.Lotes.FindAsync(id);
        }

        /// <summary>
        /// Obtém um lote por ID com dados do Artigo.
        /// </summary>
        public async Task<Lote?> ObterPorIdComArtigoAsync(int id)
        {
            return await _context.Lotes
                .Include(l => l.Artigo)
                .Include(l => l.Localizacao)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        /// <summary>
        /// Obtém um lote pelo número do lote.
        /// </summary>
        public async Task<Lote?> ObterPorNumeroAsync(string numeroLote)
        {
            if (string.IsNullOrEmpty(numeroLote)) return null;

            return await _context.Lotes
                .Include(l => l.Artigo)
                .Include(l => l.Localizacao)
                .FirstOrDefaultAsync(l => l.NumeroLote != null && l.NumeroLote.ToLower() == numeroLote.ToLower());
        }

        /// <summary>
        /// Obtém todos os lotes de um artigo.
        /// </summary>
        public async Task<List<Lote>> ObterPorArtigoIdAsync(int artigoId)
        {
            return await _context.Lotes
                .Include(l => l.Artigo)
                .Include(l => l.Localizacao)
                .Where(l => l.ArtigoId == artigoId && l.Ativo)
                .OrderBy(l => l.DataValidade)
                .ToListAsync();
        }

        /// <summary>
        /// Obtém todos os lotes de um artigo que estão em tráfico.
        /// </summary>
        public async Task<List<Lote>> ObterPendentesAsync()
        {
            return await _context.Lotes
                .Include(l => l.Artigo)         
                .Include(l => l.Localizacao)    
                .Where(l => l.EmTrafico == true) 
                .ToListAsync();
        }

        /// <summary>
        /// Obtém lotes disponíveis para FEFO (RN03).
        /// Ordenados por DataValidade ASC, excluindo expirados e sem stock.
        /// </summary>
        public async Task<List<Lote>> ObterLotesParaFEFOAsync(int artigoId)
        {
            var dataAtual = DateTime.UtcNow.Date;

            return await _context.Lotes
                .Include(l => l.Artigo)
                .Include(l => l.Localizacao)
                .Where(l => l.ArtigoId == artigoId
                         && l.Ativo
                         && l.DataValidade >= dataAtual
                         && (l.QtdDisponivel - l.QtdReservada) > 0)
                .OrderBy(l => l.DataValidade)
                .ToListAsync();
        }

        /// <summary>
        /// Verifica se já existe um lote com o mesmo número para o artigo (RN06).
        /// </summary>
        public async Task<bool> NumeroLoteJaExisteAsync(int artigoId, string numeroLote)
        {
            if (string.IsNullOrWhiteSpace(numeroLote))
                return false;

            return await _context.Lotes
                .AnyAsync(l => l.ArtigoId == artigoId && l.NumeroLote == numeroLote);
        }

        /// <summary>
        /// Obtém lotes com validade próxima ou expirados (RN13).
        /// </summary>
        public async Task<List<Lote>> ObterLotesComValidadeProximaAsync(int diasAlerta = 15)
        {
            var dataLimite = DateTime.UtcNow.Date.AddDays(diasAlerta);

            return await _context.Lotes
                .Include(l => l.Artigo)
                .Include(l => l.Localizacao)
                .Where(l => l.Ativo
                         && l.QtdDisponivel > 0
                         && l.DataValidade <= dataLimite)
                .OrderBy(l => l.DataValidade)
                .ToListAsync();
        }

        /// <summary>
        /// Obtém lotes com reservas (para verificação de timeout).
        /// </summary>
        public async Task<List<Lote>> ObterLotesComReservasExpiradasAsync()
        {
            return await _context.Lotes
                .Where(l => l.Ativo && l.QtdReservada > 0)
                .ToListAsync();
        }

        /// <summary>
        /// Adiciona um novo lote.
        /// </summary>
        public async Task AdicionarAsync(Lote lote)
        {
            await _context.Lotes.AddAsync(lote);
        }

        /// <summary>
        /// Atualiza um lote existente.
        /// </summary>
        public async Task AtualizarAsync(Lote lote)
        {
            _context.Lotes.Update(lote);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Atualiza múltiplos lotes (usado em FEFO).
        /// </summary>
        public async Task AtualizarVariosAsync(IEnumerable<Lote> lotes)
        {
            _context.Lotes.UpdateRange(lotes);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Persiste alterações na BD.
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}