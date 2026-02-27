using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Web.Models;

namespace SistemaPDI.Web.Services
{
    public interface IPdiApiService
    {
        Task<LoginResponseDto?> LoginAsync(LoginDto dto);
        void ConfigurarToken(string token);

        // Utilizadores
        Task<ApiResult<List<UtilizadorDto>>> ObterUtilizadoresAsync();
        Task<ApiResult<UtilizadorDto>> ObterUtilizadorPorIdAsync(int id);
        Task<ApiResult<UtilizadorDto>> CriarUtilizadorAsync(RegistarUtilizadorDto dto);
        Task<ApiResult<UtilizadorDto>> AtualizarUtilizadorAsync(int id, AtualizarUtilizadorDto dto);
        Task<ApiResult> DesativarUtilizadorAsync(int id);

        // ── Artigos ──────────────────────────────────────────────────────────
        Task<ApiResult<List<ArtigoDto>>> ObterArtigosAsync();
        Task<ApiResult<ArtigoDto>> ObterArtigoPorIdAsync(int id);
        Task<ApiResult<List<ArtigoDto>>> ObterArtigosComStockBaixoAsync();
        Task<ApiResult<List<ArtigoDto>>> ObterArtigosComStockCriticoAsync();
        Task<ApiResult<ArtigoDto>> CriarArtigoAsync(CriarArtigoDto dto, string? urlImagem);
        Task<ApiResult<ArtigoDto>> AtualizarArtigoAsync(int id, AtualizarArtigoDto dto);
        Task<ApiResult> AlternarEstadoArtigoAsync(int id);
        Task<ApiResult> RemoverArtigoAsync(int id);
        Task<ApiResult<List<ArtigoDto>>> ObterArtigosDesativadosAsync();

        // ── Categorias ───────────────────────────────────────────────────────
        Task<ApiResult<List<CategoriaDto>>> ObterCategoriasAsync(bool incluirInativos = false);
        Task<ApiResult<List<CategoriaDto>>> ObterCategoriasAtivasAsync();
        Task<ApiResult<CategoriaDto>> ObterCategoriaPorIdAsync(int id);
        Task<ApiResult<CategoriaDto>> CriarCategoriaAsync(CriarCategoriaDto dto);
        Task<ApiResult<CategoriaDto>> AtualizarCategoriaAsync(int id, AtualizarCategoriaDto dto);
        Task<ApiResult> AlternarEstadoCategoriaAsync(int id);
        Task<ApiResult> RemoverCategoriaAsync(int id);
    }
}