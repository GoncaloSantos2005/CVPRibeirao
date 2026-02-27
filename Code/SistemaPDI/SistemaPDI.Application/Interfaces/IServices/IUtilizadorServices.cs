using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Domain.Entities;

namespace SistemaPDI.Application.Interfaces.IUtilizadorServices
{
    public interface IUtilizadorService
    {
        Task<Result<List<UtilizadorDto>>> ObterTodosAsync();
        Task<Result<UtilizadorDto>> ObterPorIdAsync(int id);
        Task<Result<UtilizadorDto>> CriarAsync(RegistarUtilizadorDto dto);
        Task<Result<UtilizadorDto>> AtualizarAsync(int id, AtualizarUtilizadorDto dto);
        Task<Result> DesativarAsync(int id);
    }
}