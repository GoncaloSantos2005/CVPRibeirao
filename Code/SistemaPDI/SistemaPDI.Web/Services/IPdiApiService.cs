using SistemaPDI.Contracts.DTOs; // Certifique-se que AtualizarEncomendaDto está neste namespace
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

        // ── Encomendas ─────────────────────────────────────────────────────

        // Consultas
        Task<ApiResult<List<EncomendaDto>>> ObterEncomendasAsync(bool incluirInativos = false);
        Task<ApiResult<EncomendaDto>> ObterEncomendaPorIdAsync(int id);
        Task<ApiResult<List<EncomendaDto>>> ObterEncomendaPorEstadoAsync(string estado);
        Task<ApiResult<List<EncomendaDto>>> ObterMinhasEncomendasAsync();
        Task<ApiResult<List<EncomendaDto>>> ObterPendentesAprovacaoAsync();
        Task<ApiResult<List<EncomendaDto>>> ObterEncomendasEnviadasAsync();

        // ETAPA 1: Lista
        Task<ApiResult<EncomendaDto>> CriarListaAsync(CriarListaDto dto);
        Task<ApiResult<EncomendaDto>> AtualizarListaAsync(int id, CriarListaDto dto);

        // ETAPA 2: Rascunho
        Task<ApiResult<byte[]>> GerarPdfAsync(int id);
        Task<ApiResult<EncomendaDto>> MarcarComoRascunhoAsync(int id);

        // ETAPA 3: Submeter Orçamento
        Task<ApiResult<EncomendaDto>> SubmeterOrcamentoAsync(int id, SubmeterOrcamentoDto dto);

        // ETAPA 4: Aprovação
        Task<ApiResult<EncomendaDto>> RejeitarEncomendaAsync(int id, string motivo);
        Task<ApiResult<EncomendaDto>> AprovarEPreencherAsync(int id, AprovarEPreencherDto dto);

        // ETAPA 5: Enviar
        Task<ApiResult<EncomendaDto>> ConfirmarEEnviarAsync(int id);

        // ETAPA 6: Receção
        Task<ApiResult<EncomendaDto>> RegistarRecepcaoAsync(RegistarRecepcaoDto dto);

        // Outras
        Task<ApiResult> CancelarEncomendaAsync(int id, string motivo);
        Task<ApiResult> ToggleAtivoEncomendaAsync(int id);


        // ══════════════════════════════════════════════════════════════════════
        // HISTÓRICO DE PREÇOS
        // ══════════════════════════════════════════════════════════════════════

        Task<ApiResult<List<HistoricoPrecoDto>>> ObterHistoricoPrecosAsync();
        Task<ApiResult<HistoricoPrecoDto>> ObterHistoricoPrecoPorIdAsync(int id);
        Task<ApiResult<List<HistoricoPrecoDto>>> ObterHistoricoPorArtigoAsync(int artigoId);
        Task<ApiResult<List<HistoricoPrecoDto>>> ObterHistoricoPorFornecedorAsync(int fornecedorId);
        Task<ApiResult<List<HistoricoPrecoDto>>> ObterHistoricoPorEncomendaAsync(int encomendaId);
        Task<ApiResult<List<HistoricoPrecoDto>>> ObterHistoricoPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);

        // Análises
        Task<ApiResult<List<EvolucaoPrecoDto>>> ObterEvolucaoPrecosAsync(int artigoId);
        Task<ApiResult<ComparacaoFornecedorDto>> CompararFornecedoresAsync(int artigoId);
        Task<ApiResult<List<SugestaoPrecoDto>>> ObterSugestoesPrecosAsync(List<int> artigosIds, int? fornecedorId = null);
       
    }
}