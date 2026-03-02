using Microsoft.EntityFrameworkCore;
using SistemaPDI.Application.Interfaces.IRepositories;
using SistemaPDI.Domain.Entities;
using SistemaPDI.Domain.Enums;
using SistemaPDI.Infrastructure.Data;

namespace SistemaPDI.Infrastructure.Repositories
{
    /// <summary>
    /// Repositório de acesso a dados para Localizacao.
    /// </summary>
    public class LocalizacaoRepository : ILocalizacaoRepository
    {
        private readonly PdiDbContext _context;

        public LocalizacaoRepository(PdiDbContext context)
        {
            _context = context;
        }

        // ══════════════════════════════════════════════════════════════════════
        // CONSULTAS
        // ══════════════════════════════════════════════════════════════════════

        public async Task<List<Localizacao>> ObterTodosAsync(bool incluirInativos = false)
        {
            var query = _context.Localizacoes.AsQueryable();

            if (!incluirInativos)
                query = query.Where(l => l.Ativo);

            return await query
                .OrderBy(l => l.Zona)
                .ThenBy(l => l.Tipo)
                .ThenBy(l => l.Prateleira)
                .ThenBy(l => l.Nivel)
                .ToListAsync();
        }

        public async Task<List<Localizacao>> ObterAtivasAsync()
        {
            return await _context.Localizacoes
                .Where(l => l.Ativo)
                .OrderBy(l => l.Zona)
                .ThenBy(l => l.Prateleira)
                .ThenBy(l => l.Nivel)
                .ToListAsync();
        }

        public async Task<Localizacao?> ObterPorIdAsync(int id)
        {
            return await _context.Localizacoes
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Localizacao?> ObterPorIdComLotesAsync(int id)
        {
            return await _context.Localizacoes
                .Include(l => l.Lotes)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Localizacao?> ObterPorCodigoAsync(string codigo)
        {
            return await _context.Localizacoes
                .FirstOrDefaultAsync(l => l.Codigo.ToLower() == codigo.ToLower());
        }

        public async Task<List<Localizacao>> ObterPorTipoAsync(TipoLocalizacao tipo)
        {
            return await _context.Localizacoes
                .Where(l => l.Tipo == tipo && l.Ativo)
                .OrderBy(l => l.Zona)
                .ThenBy(l => l.Prateleira)
                .ToListAsync();
        }

        public async Task<List<Localizacao>> ObterPorZonaAsync(string zona)
        {
            return await _context.Localizacoes
                .Where(l => l.Zona.ToLower() == zona.ToLower() && l.Ativo)
                .OrderBy(l => l.Prateleira)
                .ThenBy(l => l.Nivel)
                .ToListAsync();
        }

        // ══════════════════════════════════════════════════════════════════════
        // VALIDAÇÕES
        // ══════════════════════════════════════════════════════════════════════

        public async Task<bool> CodigoJaExisteAsync(string codigo, int? ignorarId = null)
        {
            var query = _context.Localizacoes
                .Where(l => l.Codigo.ToLower() == codigo.ToLower());

            if (ignorarId.HasValue)
                query = query.Where(l => l.Id != ignorarId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> LabelJaExisteAsync(TipoLocalizacao tipo, string zona, string? prateleira, string? nivel, int? ignorarId = null)
        {
            var query = _context.Localizacoes
                .Where(l => l.Tipo == tipo && l.Zona.ToLower() == zona.ToLower());

            // Comparar prateleira (pode ser null)
            if (string.IsNullOrWhiteSpace(prateleira))
                query = query.Where(l => l.Prateleira == null || l.Prateleira == "");
            else
                query = query.Where(l => l.Prateleira != null && l.Prateleira.ToLower() == prateleira.ToLower());

            // Comparar nivel (pode ser null)
            if (string.IsNullOrWhiteSpace(nivel))
                query = query.Where(l => l.Nivel == null || l.Nivel == "");
            else
                query = query.Where(l => l.Nivel != null && l.Nivel.ToLower() == nivel.ToLower());

            if (ignorarId.HasValue)
                query = query.Where(l => l.Id != ignorarId.Value);

            return await query.AnyAsync();
        }

        // ══════════════════════════════════════════════════════════════════════
        // COMANDOS
        // ══════════════════════════════════════════════════════════════════════

        public async Task AdicionarAsync(Localizacao localizacao)
        {
            await _context.Localizacoes.AddAsync(localizacao);
        }

        public Task AtualizarAsync(Localizacao localizacao)
        {
            _context.Localizacoes.Update(localizacao);
            return Task.CompletedTask;
        }

        // ══════════════════════════════════════════════════════════════════════
        // PERSISTÊNCIA
        // ══════════════════════════════════════════════════════════════════════

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // ══════════════════════════════════════════════════════════════════════
        // RELACIONAMENTOS
        // ══════════════════════════════════════════════════════════════════════

        public async Task<int> ContarLotesAsync(int localizacaoId)
        {
            return await _context.Lotes
                .Where(l => l.LocalizacaoId == localizacaoId && l.Ativo)
                .CountAsync();
        }
    }
}
