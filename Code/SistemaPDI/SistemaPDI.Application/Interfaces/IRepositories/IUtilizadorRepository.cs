using SistemaPDI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaPDI.Application.Interfaces.IRepositories
{
    public interface IUtilizadorRepository
    {
        Task<Utilizador?> ObterPorEmailAsync(string email);
        Task<Utilizador?> ObterPorIdAsync(int id);
        Task<List<Utilizador>> ObterTodosAsync();
        Task AdicionarAsync(Utilizador utilizador);
        Task AtualizarAsync(Utilizador utilizador);
        Task<bool> EmailJaExisteAsync(string email);
        Task<int> SaveChangesAsync();
        Task<bool> DesativarAsync(int id);
    }
}
