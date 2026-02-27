using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Domain.Entities;

namespace SistemaPDI.Application.Interfaces.IServices
{
    public interface ICategoriaService
    {
        Task<Result<List<CategoriaDto>>> ObterTodosAsync(bool incluirInativos = false);
        Task<Result<List<CategoriaDto>>> ObterAtivasAsync();
        Task<Result<CategoriaDto>> ObterPorIdAsync(int id);
        Task<Result<CategoriaDto>> CriarAsync(CriarCategoriaDto dto);
        Task<Result<CategoriaDto>> AtualizarAsync(int id, AtualizarCategoriaDto dto);
        Task<Result<bool>> AlterarEstadoAsync(int id, bool ativo);
        Task<Result<bool>> ApagarAsync(int id);
    }
}   