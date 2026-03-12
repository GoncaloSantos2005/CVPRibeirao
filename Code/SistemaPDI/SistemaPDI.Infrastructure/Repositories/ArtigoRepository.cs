using Microsoft.EntityFrameworkCore;
using SistemaPDI.Application.Interfaces.IRepositories;
using SistemaPDI.Domain.Entities;
using SistemaPDI.Infrastructure.Data;

namespace SistemaPDI.Infrastructure.Repositories
{
    public class ArtigoRepository : IArtigoRepository
    {
        private readonly PdiDbContext _context;

        public ArtigoRepository(PdiDbContext context)
        {
            _context = context;
        }

        public async Task<Artigo?> ObterPorIdAsync(int id)
        {
            return await _context.Artigos
                .Include(a => a.Categoria)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Artigo>> ObterTodosAsync()
        {
            return await _context.Artigos
                .Include(a => a.Categoria)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Artigo>> ObterComStockBaixoAsync()
        {
            return await _context.Artigos
                .Include(a => a.Categoria)
                .Where(a => a.Ativo
                    && a.StockFisico > a.StockCritico 
                    && a.StockFisico <= a.StockMinimo
                )
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Artigo>> ObterComStockCriticoAsync()
        {
            return await _context.Artigos
                .Include(a => a.Categoria)
                .Where(a => a.Ativo && a.StockFisico <= a.StockCritico)
                .AsNoTracking()
                .ToListAsync();
        }

        public void ToggleAtivo(Artigo artigo, string utilizador)
        {
            if (artigo.Ativo)
            {
                artigo.Ativo = false;
                artigo.DesativadoEm = DateTime.UtcNow;
                artigo.DesativadoPor = utilizador;
            }
            else
            {
                artigo.Ativo = true;
                artigo.DesativadoEm = null;
                artigo.DesativadoPor = null;
            }
        }

        public async Task AdicionarAsync(Artigo artigo)
        {
            await _context.Artigos.AddAsync(artigo);
        }

        public Task AtualizarAsync(Artigo artigo)
        {
            _context.Artigos.Update(artigo);
            return Task.CompletedTask;
        }

        public void Remover(Artigo artigo)
        {
            _context.Artigos.Remove(artigo);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<List<Artigo>> ObterAtivosAsync()
        {
            return await _context.Artigos
                .Include(a => a.Categoria)
                .Where(a => a.Ativo)
                .ToListAsync();
        }

        public async Task<List<Artigo>> ObterDesativadosAsync()
        {
            return await _context.Artigos
                .Include(a => a.Categoria)
                .Where(a => !a.Ativo)
                .ToListAsync();
        }
    }
}