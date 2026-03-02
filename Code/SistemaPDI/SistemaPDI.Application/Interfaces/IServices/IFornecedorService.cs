using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Domain.Entities;

namespace SistemaPDI.Application.Interfaces.IServices
{
    public interface IFornecedorService
    {
        Task<Result<List<FornecedorDto>>> ObterTodosAsync(bool incluirInativos = false);
        Task<Result<List<FornecedorDropdownDto>>> ObterAtivosParaDropdownAsync();
        Task<Result<List<FornecedorDropdownDto>>> ObterPreferenciaisParaDropdownAsync();
        Task<Result<FornecedorDto>> ObterPorIdAsync(int id);
        Task<Result<FornecedorDto>> CriarAsync(CriarFornecedorDto dto);
        Task<Result<FornecedorDto>> AtualizarAsync(int id, AtualizarFornecedorDto dto);
        Task<Result<bool>> AlterarEstadoAsync(int id, bool ativo);
        Task<Result<bool>> TogglePreferencialAsync(int id);
        Task<Result<bool>> ApagarAsync(int id);
    }
}