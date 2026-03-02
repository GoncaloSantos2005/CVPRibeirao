using SistemaPDI.Domain.Entities;
using SistemaPDI.Domain.Enums;

namespace SistemaPDI.Application.Interfaces.IRepositories
{
    public interface ILocalizacaoRepository
    {
        // --- Consultas ---
        Task<List<Localizacao>> ObterTodosAsync(bool incluirInativos = false);
        Task<List<Localizacao>> ObterAtivasAsync();
        Task<Localizacao?> ObterPorIdAsync(int id);
        Task<Localizacao?> ObterPorIdComLotesAsync(int id);
        Task<Localizacao?> ObterPorCodigoAsync(string codigo);
        Task<List<Localizacao>> ObterPorTipoAsync(TipoLocalizacao tipo);
        Task<List<Localizacao>> ObterPorZonaAsync(string zona);

        // --- Validações ---
        Task<bool> CodigoJaExisteAsync(string codigo, int? ignorarId = null);
        Task<bool> LabelJaExisteAsync(TipoLocalizacao tipo, string zona, string? prateleira, string? nivel, int? ignorarId = null);

        // --- Comandos ---
        Task AdicionarAsync(Localizacao localizacao);
        Task AtualizarAsync(Localizacao localizacao);

        // --- Persistência ---
        Task<int> SaveChangesAsync();

        // --- Relacionamentos ---
        Task<int> ContarLotesAsync(int localizacaoId);
    }
}