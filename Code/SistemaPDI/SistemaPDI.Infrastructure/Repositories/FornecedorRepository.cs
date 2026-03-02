using Microsoft.EntityFrameworkCore;
using SistemaPDI.Application.Interfaces.IRepositories;
using SistemaPDI.Domain.Entities;
using SistemaPDI.Infrastructure.Data;

namespace SistemaPDI.Infrastructure.Repositories
{
    public class FornecedorRepository : IFornecedorRepository
    {
        private readonly PdiDbContext _context;

        public FornecedorRepository(PdiDbContext context)
        {
            _context = context;
        }

        public async Task<Fornecedor?> ObterPorIdAsync(int id)
        {
            return await _context.Fornecedores
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Fornecedor?> ObterPorNIFAsync(string nif)
        {
            return await _context.Fornecedores
                .FirstOrDefaultAsync(f => f.NIF == nif);
        }

        public async Task<Fornecedor?> ObterPorNomeAsync(string nome)
        {
            return await _context.Fornecedores
                .FirstOrDefaultAsync(f => f.Nome.ToLower() == nome.ToLower());
        }

        public async Task<List<Fornecedor>> ObterTodosAsync(bool incluirInativos = false)
        {
            var query = _context.Fornecedores.AsQueryable();

            if (!incluirInativos)
                query = query.Where(f => f.Ativo);

            return await query
                .OrderByDescending(f => f.Preferencial) // Preferenciais primeiro
                .ThenBy(f => f.Nome)
                .ToListAsync();
        }

        public async Task<List<Fornecedor>> ObterAtivosAsync()
        {
            return await _context.Fornecedores
                .Where(f => f.Ativo)
                .OrderByDescending(f => f.Preferencial)
                .ThenBy(f => f.Nome)
                .ToListAsync();
        }

        public async Task<List<Fornecedor>> ObterPreferenciaisAsync()
        {
            return await _context.Fornecedores
                .Where(f => f.Ativo && f.Preferencial)
                .OrderBy(f => f.Nome)
                .ToListAsync();
        }

        public async Task AdicionarAsync(Fornecedor fornecedor)
        {
            await _context.Fornecedores.AddAsync(fornecedor);
        }

        public Task AtualizarAsync(Fornecedor fornecedor)
        {
            _context.Fornecedores.Update(fornecedor);
            return Task.CompletedTask;
        }

        public async Task<bool> NIFJaExisteAsync(string nif, int? ignorarId = null)
        {
            var query = _context.Fornecedores.Where(f => f.NIF == nif);

            if (ignorarId.HasValue)
                query = query.Where(f => f.Id != ignorarId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> NomeJaExisteAsync(string nome, int? ignorarId = null)
        {
            var query = _context.Fornecedores.Where(f => f.Nome.ToLower() == nome.ToLower());

            if (ignorarId.HasValue)
                query = query.Where(f => f.Id != ignorarId.Value);

            return await query.AnyAsync();
        }

        public Task<int> ContarEncomendasAsync(int fornecedorId)
        {
            // Por agora retorna 0 (tabela Encomenda ainda não existe)
            // Quando criares Encomenda, atualiza para:
            // return await _context.Encomendas.Where(e => e.FornecedorId == fornecedorId).CountAsync();
            return Task.FromResult(0);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}