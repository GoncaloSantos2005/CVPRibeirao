namespace SistemaPDI.Contracts.DTOs
{
    /// <summary>
    /// DTO para criar um novo lote durante a receção de stock.
    /// </summary>
    public record CriarLoteDto(
        int ArtigoId,
        string NumeroLote,
        DateTime DataValidade,
        decimal PrecoUnitario,
        int Quantidade,
        int? LocalizacaoId
    );

    /// <summary>
    /// DTO para atualizar um lote existente.
    /// </summary>
    public record AtualizarLoteDto(
        DateTime DataValidade,
        decimal PrecoUnitario,
        int? LocalizacaoId,
        int QtdDisponivel
    );

    /// <summary>
    /// DTO de leitura de um lote.
    /// </summary>
    public record LoteDto(
        int Id,
        int ArtigoId,
        string NomeArtigo,
        string SKU,
        string NumeroLote,
        DateTime DataValidade,
        decimal PrecoUnitario,
        int QtdDisponivel,
        int QtdReservada,
        int QtdRealmenteDisponivel,
        int? LocalizacaoId,
        string? LocalizacaoLabel,
        bool Ativo,
        bool EstaExpirado,
        bool ValidadeProxima,
        DateTime CriadoEm
    );

    /// <summary>
    /// DTO para reservar stock de um artigo (entrada para FEFO).
    /// </summary>
    public record ReservaStockDto(
        int ArtigoId,
        int QuantidadeTotal,
        int GuiaPickingId
    );

    /// <summary>
    /// DTO de resultado da alocação FEFO (um item por lote alocado).
    /// </summary>
    public record AlocacaoLoteDto(
        int LoteId,
        string NumeroLote,
        DateTime DataValidade,
        int QuantidadeAlocada,
        string? LocalizacaoLabel  // Para o socorrista saber onde ir buscar
    );

    /// <summary>
    /// DTO de resultado completo da reserva FEFO.
    /// </summary>
    public record ResultadoReservaDto(
        int ArtigoId,
        int QuantidadeSolicitada,
        int QuantidadeAlocada,
        bool AlocacaoCompleta,
        List<AlocacaoLoteDto> Alocacoes
    );

    /// <summary>
    /// DTO para libertar reservas (timeout ou cancelamento).
    /// </summary>
    public record LibertarReservaDto(
        int LoteId,
        int Quantidade
    );

    /// <summary>
    /// DTO para alerta de validade próxima (RN13).
    /// </summary>
    public record AlertaValidadeDto(
        int LoteId,
        int ArtigoId,
        string NomeArtigo,
        string NumeroLote,
        DateTime DataValidade,
        int DiasParaExpirar,
        int QtdDisponivel,
        string? LocalizacaoLabel,
        bool JaExpirado
    );
}