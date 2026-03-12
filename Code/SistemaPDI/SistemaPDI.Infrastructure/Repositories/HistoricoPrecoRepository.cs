using Microsoft.EntityFrameworkCore;
using SistemaPDI.Application.Interfaces.IRepositories;
using SistemaPDI.Domain.Entities;
using SistemaPDI.Infrastructure.Data;

namespace SistemaPDI.Infrastructure.Repositories
{
    public class HistoricoPrecoRepository : IHistoricoPrecoRepository
    {
        private readonly PdiDbContext _context;

        public HistoricoPrecoRepository(PdiDbContext context)
        {
            _context = context;
        }

        // ══════════════════════════════════════════════════════════════════════
        // CONSULTAS
        // ══════════════════════════════════════════════════════════════════════

        public async Task<List<HistoricoPreco>> ObterTodosAsync()
        {
            return await _context.HistoricosPrecos
                .Include(hp => hp.Artigo)
                .Include(hp => hp.Fornecedor)
                .Include(hp => hp.Encomenda)
                .OrderByDescending(hp => hp.DataCompra)
                .ToListAsync();
        }

        public async Task<HistoricoPreco?> ObterPorIdAsync(int id)
        {
            return await _context.HistoricosPrecos
                .Include(hp => hp.Artigo)
                .Include(hp => hp.Fornecedor)
                .Include(hp => hp.Encomenda)
                .FirstOrDefaultAsync(hp => hp.Id == id);
        }

        public async Task<List<HistoricoPreco>> ObterPorArtigoAsync(int artigoId)
        {
            return await _context.HistoricosPrecos
                .Include(hp => hp.Fornecedor)
                .Include(hp => hp.Encomenda)
                .Where(hp => hp.ArtigoId == artigoId)
                .OrderByDescending(hp => hp.DataCompra)
                .ToListAsync();
        }

        public async Task<List<HistoricoPreco>> ObterPorFornecedorAsync(int fornecedorId)
        {
            return await _context.HistoricosPrecos
                .Include(hp => hp.Artigo)
                .Include(hp => hp.Encomenda)
                .Where(hp => hp.FornecedorId == fornecedorId)
                .OrderByDescending(hp => hp.DataCompra)
                .ToListAsync();
        }

        public async Task<List<HistoricoPreco>> ObterPorEncomendaAsync(int encomendaId)
        {
            return await _context.HistoricosPrecos
                .Include(hp => hp.Artigo)
                .Include(hp => hp.Fornecedor)
                .Where(hp => hp.EncomendaId == encomendaId)
                .OrderBy(hp => hp.Artigo.Nome)
                .ToListAsync();
        }

        public async Task<List<HistoricoPreco>> ObterPorArtigoEFornecedorAsync(int artigoId, int fornecedorId)
        {
            return await _context.HistoricosPrecos
                .Include(hp => hp.Encomenda)
                .Where(hp => hp.ArtigoId == artigoId && hp.FornecedorId == fornecedorId)
                .OrderByDescending(hp => hp.DataCompra)
                .ToListAsync();
        }

        public async Task<List<HistoricoPreco>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            return await _context.HistoricosPrecos
                .Include(hp => hp.Artigo)
                .Include(hp => hp.Fornecedor)
                .Include(hp => hp.Encomenda)
                .Where(hp => hp.DataCompra >= dataInicio && hp.DataCompra <= dataFim)
                .OrderByDescending(hp => hp.DataCompra)
                .ToListAsync();
        }

        public async Task<HistoricoPreco?> ObterUltimoPrecoAsync(int artigoId, int? fornecedorId = null)
        {
            var query = _context.HistoricosPrecos
                .Include(hp => hp.Fornecedor)
                .Where(hp => hp.ArtigoId == artigoId);

            if (fornecedorId.HasValue)
            {
                query = query.Where(hp => hp.FornecedorId == fornecedorId.Value);
            }

            return await query
                .OrderByDescending(hp => hp.DataCompra)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal?> ObterPrecoMedioAsync(int artigoId, int? fornecedorId = null)
        {
            var query = _context.HistoricosPrecos
                .Where(hp => hp.ArtigoId == artigoId);

            if (fornecedorId.HasValue)
            {
                query = query.Where(hp => hp.FornecedorId == fornecedorId.Value);
            }

            if (!await query.AnyAsync())
                return null;

            return await query.AverageAsync(hp => hp.PrecoUnitario);
        }

        // ══════════════════════════════════════════════════════════════════════
        // OPERAÇÕES
        // ══════════════════════════════════════════════════════════════════════

        public async Task AdicionarAsync(HistoricoPreco historicoPreco)
        {
            await _context.HistoricosPrecos.AddAsync(historicoPreco);
        }

        public async Task AdicionarVariosAsync(List<HistoricoPreco> historicos)
        {
            await _context.HistoricosPrecos.AddRangeAsync(historicos);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}