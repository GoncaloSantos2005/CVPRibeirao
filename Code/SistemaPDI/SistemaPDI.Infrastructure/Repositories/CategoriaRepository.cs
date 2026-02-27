using Microsoft.EntityFrameworkCore;
using SistemaPDI.Application.Interfaces.IRepositories;
using SistemaPDI.Domain.Entities;
using SistemaPDI.Infrastructure.Data;

namespace SistemaPDI.Infrastructure.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly PdiDbContext _context;

        public CategoriaRepository(PdiDbContext context)
        {
            _context = context;
        }

        public async Task<Categoria?> ObterPorIdAsync(int id)
        {
            return await _context.Categorias
                .Include(c => c.Artigos)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Categoria?> ObterPorNomeAsync(string nome)
        {
            return await _context.Categorias
                .FirstOrDefaultAsync(c => c.Nome.ToLower() == nome.ToLower());
        }

        public async Task<List<Categoria>> ObterTodosAsync(bool incluirInativos = false)
        {
            var query = _context.Categorias.AsQueryable();

            if (!incluirInativos)
                query = query.Where(c => c.Ativo);

            return await query
                .OrderBy(c => c.Nome)
                .ToListAsync();
        }

        public async Task<List<Categoria>> ObterAtivasAsync()
        {
            return await _context.Categorias
                .Where(c => c.Ativo)
                .OrderBy(c => c.Nome)
                .ToListAsync();
        }

        public async Task AdicionarAsync(Categoria categoria)
        {
            await _context.Categorias.AddAsync(categoria);
        }

        public Task AtualizarAsync(Categoria categoria)
        {
            _context.Categorias.Update(categoria);
            return Task.CompletedTask;
        }

        public async Task<bool> NomeJaExisteAsync(string nome, int? ignorarId = null)
        {
            var query = _context.Categorias.Where(c => c.Nome.ToLower() == nome.ToLower());

            if (ignorarId.HasValue)
                query = query.Where(c => c.Id != ignorarId.Value);

            return await query.AnyAsync();
        }

        public async Task<int> ContarArtigosPorCategoriaAsync(int categoriaId)
        {
            return await _context.Artigos
                .Where(a => a.CategoriaId == categoriaId)
                .CountAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}