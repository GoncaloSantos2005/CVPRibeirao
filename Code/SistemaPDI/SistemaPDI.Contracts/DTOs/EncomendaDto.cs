using Microsoft.AspNetCore.Http;

namespace SistemaPDI.Contracts.DTOs
{
    // ══════════════════════════════════════════════════════════════════════
    // ETAPA 1: CRIAR LISTA
    // ══════════════════════════════════════════════════════════════════════

    public record CriarListaDto(
        List<LinhaListaDto> Linhas,
        string? Observacoes
    );

    public record LinhaListaDto(
        int ArtigoId,
        int QuantidadeEncomendada
    );

    // ══════════════════════════════════════════════════════════════════════
    // ETAPA 2: SUBMETER ORÇAMENTO (RASCUNHO → PENDENTE)
    // ══════════════════════════════════════════════════════════════════════

    public record SubmeterOrcamentoDto(
        decimal ValorOrcamento,
        string? Observacoes,
        string? CaminhoOrcamentoPdf = null
    );

    // ══════════════════════════════════════════════════════════════════════
    // ETAPA 3: APROVAR E PREENCHER (PENDENTE → CONFIRMADA)
    // ══════════════════════════════════════════════════════════════════════

    public record AprovarEPreencherDto(
        int FornecedorId,
        DateTime? DataEntregaPrevista,
        List<PreencherLinhaDto> Linhas,
        string? ObservacoesInternas
    );

    public record PreencherLinhaDto(
        int LinhaId,
        int QuantidadeAprovada,
        decimal PrecoUnitario,
        string? NumeroLote,
        DateTime? DataValidade
    );

    // ══════════════════════════════════════════════════════════════════════
    // ETAPA 4: REGISTAR RECEÇÃO (ENVIADA → PARCIAL/CONCLUIDA)
    // ══════════════════════════════════════════════════════════════════════

    public record RegistarRecepcaoDto(
        int EncomendaId,
        List<RecepcaoLinhaDto> Linhas,
        string? Observacoes
    );

    public record RecepcaoLinhaDto(
        int LinhaId,
        int QuantidadeRecebida,
        string NumeroLote,
        DateTime DataValidade,
        int LocalizacaoId,
        string? Observacoes
    );

    // ══════════════════════════════════════════════════════════════════════
    // DTOs DE CRIAÇÃO E ATUALIZAÇÃO
    // ══════════════════════════════════════════════════════════════════════

    public record CriarEncomendaDto(
        int? FornecedorId,
        DateTime? DataEntregaPrevista,
        string? Observacoes,
        string? ObservacoesInternas,
        List<CriarLinhaEncomendaDto> Linhas
    );

    public record CriarLinhaEncomendaDto(
        int ArtigoId,
        int QuantidadeEncomendada,
        decimal? PrecoUnitario,
        string? Observacoes
    );

    public record AtualizarEncomendaDto(
        int? FornecedorId,
        DateTime? DataEntregaPrevista,
        string? Observacoes,
        string? ObservacoesInternas,
        List<AtualizarLinhaEncomendaDto>? Linhas
    );

    public record AtualizarLinhaEncomendaDto(
        int? Id,
        int ArtigoId,
        int QuantidadeEncomendada,
        int QuantidadeAprovada,
        decimal? PrecoUnitario,
        string? NumeroLote,
        DateTime? DataValidade,
        int? LocalizacaoId,
        string? Observacoes
    );

    // ══════════════════════════════════════════════════════════════════════
    // DTOs DE LEITURA
    // ══════════════════════════════════════════════════════════════════════

    public record EncomendaDto(
        int Id,
        string NumeroEncomenda,
        DateTime DataCriacao,
        DateTime? DataEnvioFornecedor,
        DateTime? DataEntregaPrevista,
        DateTime? DataEntregaReal,
        string Estado,
        int? FornecedorId,
        string? FornecedorNome,
        string? CaminhoOrcamentoPdf,
        decimal? ValorOrcamento,
        decimal ValorTotal,
        string? Observacoes,
        string? ObservacoesInternas,
        string? MotivoRejeicao,
        string? RejeitadoPor,
        DateTime? RejeitadoEm,
        bool Ativo,
        DateTime CriadoEm,
        string? CriadoPor,
        int TotalLinhas,
        int TotalArtigosRequisitados,
        int TotalArtigosAprovados,
        int TotalArtigosRecebidos,
        decimal PercentualRecebido,
        List<LinhaEncomendaDto> Linhas
    );

    public record LinhaEncomendaDto(
        int Id,
        int ArtigoId,
        string ArtigoNome,
        string ArtigoSKU,
        int QuantidadeEncomendada,
        int QuantidadeAprovada,
        int QuantidadeRecebida,
        int QuantidadePendente,
        decimal? PrecoUnitario,
        decimal? Subtotal,
        string? NumeroLote,
        DateTime? DataValidade,
        int? LocalizacaoId,
        string? LocalizacaoNome,
        string? Observacoes
    );

    // ══════════════════════════════════════════════════════════════════════
    // DROPDOWN
    // ══════════════════════════════════════════════════════════════════════

    public record EncomendaDropdownDto(
        int Id,
        string NumeroEncomenda,
        string? FornecedorNome,
        string Estado
    );
}