using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Web.Models;

namespace SistemaPDI.Web.Services
{
    public interface IPdiApiService
    {
        Task<LoginResponseDto?> LoginAsync(LoginDto dto);
        void ConfigurarToken(string token);

        // ── Utilizadores ─────────────────────────────────────────────────────
        Task<ApiResult<List<UtilizadorDto>>> ObterUtilizadoresAsync();
        Task<ApiResult<UtilizadorDto>> ObterUtilizadorPorIdAsync(int id);
        Task<ApiResult<UtilizadorDto>> CriarUtilizadorAsync(RegistarUtilizadorDto dto);
        Task<ApiResult<UtilizadorDto>> AtualizarUtilizadorAsync(int id, AtualizarUtilizadorDto dto);
        Task<ApiResult> DesativarUtilizadorAsync(int id);
        Task<ApiResult> AtivarUtilizadorAsync(int id);
        Task<ApiResult> ResetPasswordAsync(int id, string novaPassword);

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

        // ── Fornecedores ─────────────────────────────────────────────────────
        Task<ApiResult<List<FornecedorDto>>> ObterFornecedoresAsync(bool incluirInativos = false);
        Task<ApiResult<List<FornecedorDropdownDto>>> ObterFornecedoresDropdownAsync();
        Task<ApiResult<List<FornecedorDropdownDto>>> ObterFornecedoresPreferenciaisAsync();
        Task<ApiResult<FornecedorDto>> ObterFornecedorPorIdAsync(int id);
        Task<ApiResult<FornecedorDto>> CriarFornecedorAsync(CriarFornecedorDto dto);
        Task<ApiResult<FornecedorDto>> AtualizarFornecedorAsync(int id, AtualizarFornecedorDto dto);
        Task<ApiResult> ToggleAtivoFornecedorAsync(int id);
        Task<ApiResult> TogglePreferencialFornecedorAsync(int id);
        Task<ApiResult> ApagarFornecedorAsync(int id);

        // ── Lotes ─────────────────────────────────────────────────────
        Task<ApiResult<List<LoteDto>>> ObterLotesAsync();
        Task<ApiResult<LoteDto>> ObterLotePorIdAsync(int id);
        Task<ApiResult<List<LoteDto>>> ObterLotesPorArtigoAsync(int artigoId);
        Task<ApiResult<List<AlertaValidadeDto>>> ObterAlertasValidadeAsync(int dias = 15);
        Task<ApiResult<LoteDto>> CriarLoteAsync(CriarLoteDto dto);
        Task<ApiResult<LoteDto>> AtualizarLoteAsync(int id, AtualizarLoteDto dto);
        Task<ApiResult> DesativarLoteAsync(int id);
        Task<ApiResult<ResultadoReservaDto>> ReservarStockAsync(ReservaStockDto dto);
        Task<ApiResult> LibertarReservaAsync(LibertarReservaDto dto);
        Task<ApiResult> ConfirmarSaidaLoteAsync(int loteId, int quantidade);
        Task<ApiResult<int>> ObterStockDisponivelAsync(int artigoId);

        // ── Localizações ─────────────────────────────────────────────────────
        Task<ApiResult<List<LocalizacaoDto>>> ObterLocalizacoesAsync(bool incluirInativos = false);
        Task<ApiResult<List<LocalizacaoDto>>> ObterLocalizacoesAtivasAsync();
        Task<ApiResult<LocalizacaoDto>> ObterLocalizacaoPorIdAsync(int id);
        Task<ApiResult<List<LocalizacaoDropdownDto>>> ObterLocalizacoesDropdownAsync();
        Task<ApiResult<LocalizacaoDto>> CriarLocalizacaoAsync(CriarLocalizacaoDto dto);
        Task<ApiResult<LocalizacaoDto>> AtualizarLocalizacaoAsync(int id, AtualizarLocalizacaoDto dto);
        Task<ApiResult> ToggleAtivoLocalizacaoAsync(int id);
        Task<ApiResult> ApagarLocalizacaoAsync(int id);
    }
}