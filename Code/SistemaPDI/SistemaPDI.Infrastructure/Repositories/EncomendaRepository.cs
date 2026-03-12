using Microsoft.EntityFrameworkCore;
using SistemaPDI.Application.Interfaces.IRepositories;
using SistemaPDI.Domain.Entities;
using SistemaPDI.Domain.Enums;
using SistemaPDI.Infrastructure.Data;

namespace SistemaPDI.Infrastructure.Repositories
{
    public class EncomendaRepository : IEncomendaRepository
    {
        private readonly PdiDbContext _context;

        public EncomendaRepository(PdiDbContext context)
        {
            _context = context;
        }

        public async Task<Encomenda?> ObterPorIdAsync(int id)
        {
            return await _context.Encomendas
                .Include(e => e.Fornecedor)
                .Include(e => e.Linhas)
                    .ThenInclude(l => l.Artigo)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Encomenda?> ObterPorNumeroAsync(string numeroEncomenda)
        {
            return await _context.Encomendas
                .Include(e => e.Fornecedor)
                .Include(e => e.Linhas)
                    .ThenInclude(l => l.Artigo)
                .FirstOrDefaultAsync(e => e.NumeroEncomenda == numeroEncomenda);
        }

        public async Task<List<Encomenda>> ObterTodosAsync(bool incluirInativos = false)
        {
            var query = _context.Encomendas
                .Include(e => e.Fornecedor)
                .Include(e => e.Linhas)
                .AsQueryable();

            if (!incluirInativos)
                query = query.Where(e => e.Ativo);

            return await query
                .OrderByDescending(e => e.DataCriacao)
                .ToListAsync();
        }

        public async Task<List<Encomenda>> ObterPorFornecedorAsync(int fornecedorId)
        {
            return await _context.Encomendas
                .Include(e => e.Fornecedor)
                .Include(e => e.Linhas)
                .Where(e => e.FornecedorId == fornecedorId && e.Ativo)
                .OrderByDescending(e => e.DataCriacao)
                .ToListAsync();
        }

        public async Task<List<Encomenda>> ObterPorEstadoAsync(EstadoEncomenda estado)
        {
            return await _context.Encomendas
                .Include(e => e.Fornecedor)
                .Include(e => e.Linhas)
                .Where(e => e.Estado == estado && e.Ativo)
                .OrderByDescending(e => e.DataCriacao)
                .ToListAsync();
        }

        public async Task<List<Encomenda>> ObterPendentesAsync()
        {
            return await _context.Encomendas
                .Include(e => e.Fornecedor)
                .Include(e => e.Linhas)
                .Where(e => (e.Estado == EstadoEncomenda.PENDENTE || e.Estado == EstadoEncomenda.PARCIAL) && e.Ativo)
                .OrderBy(e => e.DataEntregaPrevista)
                .ToListAsync();
        }

        public async Task AdicionarAsync(Encomenda encomenda)
        {
            await _context.Encomendas.AddAsync(encomenda);
        }

        public async Task AtualizarAsync(Encomenda encomenda)
        {
            _context.Encomendas.Update(encomenda);
            await Task.CompletedTask;
        }

        public async Task<bool> NumeroJaExisteAsync(string numeroEncomenda)
        {
            return await _context.Encomendas
                .AnyAsync(e => e.NumeroEncomenda == numeroEncomenda);
        }

        public async Task<string> GerarProximoNumeroAsync()
        {
            var ano = DateTime.UtcNow.Year;
            var prefixo = $"ENC-{ano}-";

            // Procurar última encomenda do ano
            var ultimaEncomenda = await _context.Encomendas
                .Where(e => e.NumeroEncomenda.StartsWith(prefixo))
                .OrderByDescending(e => e.NumeroEncomenda)
                .FirstOrDefaultAsync();

            if (ultimaEncomenda == null)
            {
                return $"{prefixo}001"; // Primeira do ano
            }

            // Extrair número e incrementar
            var ultimoNumero = ultimaEncomenda.NumeroEncomenda.Substring(prefixo.Length);
            if (int.TryParse(ultimoNumero, out int numero))
            {
                return $"{prefixo}{(numero + 1):D3}"; // D3 = 3 dígitos (001, 002...)
            }

            return $"{prefixo}001";
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}