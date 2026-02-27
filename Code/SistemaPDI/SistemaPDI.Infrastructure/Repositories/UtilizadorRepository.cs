using Microsoft.EntityFrameworkCore;
using SistemaPDI.Application.Interfaces.IRepositories;
using SistemaPDI.Domain.Entities;
using SistemaPDI.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaPDI.Infrastructure.Repositories
{
    public class UtilizadorRepository : IUtilizadorRepository
    {
        private readonly PdiDbContext _context;

        public UtilizadorRepository(PdiDbContext context)
        {
            _context = context;
        }

        public async Task<Utilizador?> ObterPorEmailAsync(string email)
        {
            return await _context.Utilizadores
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<Utilizador?> ObterPorIdAsync(int id)
        {
            return await _context.Utilizadores.FindAsync(id);
        }

        public async Task<List<Utilizador>> ObterTodosAsync()
        {
            return await _context.Utilizadores
                .Where(u => u.Ativo)
                .OrderBy(u => u.NomeCompleto)
                .ToListAsync();
        }

        public async Task AdicionarAsync(Utilizador utilizador)
        {
            await _context.Utilizadores.AddAsync(utilizador);
        }

        public async Task AtualizarAsync(Utilizador utilizador)
        {
            _context.Utilizadores.Update(utilizador);
            await Task.CompletedTask;
        }

        public async Task<bool> EmailJaExisteAsync(string email)
        {
            return await _context.Utilizadores
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }
        public async Task<bool> DesativarAsync(int id)
        {
            var utilizador = await _context.Utilizadores.FindAsync(id);
            if (utilizador == null) return false;

            utilizador.Ativo = false;
            _context.Utilizadores.Update(utilizador);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
