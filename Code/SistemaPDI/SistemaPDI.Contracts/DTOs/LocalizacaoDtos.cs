using SistemaPDI.Domain.Enums;
using System;

namespace SistemaPDI.Contracts.DTOs
{
    /// <summary>
    /// DTO de leitura de uma localização.
    /// </summary>
    public record LocalizacaoDto(
        int Id,
        string Codigo,
        TipoLocalizacao Tipo,
        string TipoNome,
        string Zona,
        string? Prateleira,
        string? Nivel,
        string? Observacoes,
        string Label,              // Calculado: "PRATELEIRA_A_3_2"
        bool Ativo,
        DateTime CriadoEm
    );

    /// <summary>
    /// DTO para criar uma nova localização.
    /// </summary>
    public record CriarLocalizacaoDto(
        string Codigo,
        TipoLocalizacao Tipo,
        string Zona,
        string? Prateleira,
        string? Nivel,
        string? Observacoes
    );

    /// <summary>
    /// DTO para atualizar uma localização existente.
    /// </summary>
    public record AtualizarLocalizacaoDto(
        string Codigo,
        TipoLocalizacao Tipo,
        string Zona,
        string? Prateleira,
        string? Nivel,
        string? Observacoes
    );

    /// <summary>
    /// DTO para dropdowns/selects.
    /// </summary>
    public record LocalizacaoDropdownDto(
        int Id,
        string Label    // Ex: "PRATELEIRA_A_3_2"
    );
}
