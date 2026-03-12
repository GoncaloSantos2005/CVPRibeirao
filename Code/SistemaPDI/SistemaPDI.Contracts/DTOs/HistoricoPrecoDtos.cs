namespace SistemaPDI.Contracts.DTOs
{
    // ══════════════════════════════════════════════════════════════════════
    // DTO DE LEITURA
    // ══════════════════════════════════════════════════════════════════════

    public record HistoricoPrecoDto(
        int Id,
        int ArtigoId,
        string ArtigoNome,
        string ArtigoSKU,
        int FornecedorId,
        string FornecedorNome,
        int? EncomendaId,
        string? EncomendaNumero,
        decimal PrecoUnitario,
        int Quantidade,
        decimal ValorTotal,
        DateTime DataCompra,
        string? Observacoes,
        DateTime CriadoEm,
        string? CriadoPor
    );

    // ══════════════════════════════════════════════════════════════════════
    // DTO PARA ANÁLISE DE EVOLUÇÃO
    // ══════════════════════════════════════════════════════════════════════

    public record EvolucaoPrecoDto(
        int ArtigoId,
        string ArtigoNome,
        string ArtigoSKU,
        decimal PrecoAtual,
        decimal PrecoAnterior,
        decimal DiferencaAbsoluta,
        decimal DiferencaPercentual,
        DateTime DataUltimaCompra,
        string? FornecedorAtual
    );

    // ══════════════════════════════════════════════════════════════════════
    // DTO PARA COMPARAÇÃO DE FORNECEDORES
    // ══════════════════════════════════════════════════════════════════════

    public record ComparacaoFornecedorDto(
        int ArtigoId,
        string ArtigoNome,
        List<PrecoFornecedorDto> Fornecedores
    );

    public record PrecoFornecedorDto(
        int FornecedorId,
        string FornecedorNome,
        decimal PrecoMedio,
        decimal UltimoPreco,
        DateTime UltimaCompra,
        int TotalCompras
    );

    // ══════════════════════════════════════════════════════════════════════
    // DTO PARA SUGESTÃO DE PREÇO
    // ══════════════════════════════════════════════════════════════════════

    public record SugestaoPrecoDto(
        int ArtigoId,
        string ArtigoNome,
        int? FornecedorId,
        string? FornecedorNome,
        decimal? UltimoPreco,
        decimal? PrecoMedio,
        DateTime? UltimaCompra,
        bool TemHistorico
    );
}