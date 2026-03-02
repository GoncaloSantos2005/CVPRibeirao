using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Domain.Entities;
using SistemaPDI.Domain.Enums;

namespace SistemaPDI.Application.Interfaces.IServices
{
    public interface ILocalizacaoService
    {
        // ── LEITURA ───────────────────────────────────────────────────────────
        Task<Result<List<LocalizacaoDto>>> ObterTodosAsync(bool incluirInativos = false);
        Task<Result<List<LocalizacaoDto>>> ObterAtivasAsync();
        Task<Result<LocalizacaoDto>> ObterPorIdAsync(int id);
        Task<Result<List<LocalizacaoDto>>> ObterPorTipoAsync(TipoLocalizacao tipo);
        Task<Result<List<LocalizacaoDto>>> ObterPorZonaAsync(string zona);
        Task<Result<List<LocalizacaoDropdownDto>>> ObterParaDropdownAsync();

        // ── ESCRITA ───────────────────────────────────────────────────────────
        Task<Result<LocalizacaoDto>> CriarAsync(CriarLocalizacaoDto dto);
        Task<Result<LocalizacaoDto>> AtualizarAsync(int id, AtualizarLocalizacaoDto dto);
        Task<Result> AlternarEstadoAtivoAsync(int id);
        Task<Result> ApagarAsync(int id);
    }
}