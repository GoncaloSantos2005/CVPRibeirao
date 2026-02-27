namespace SistemaPDI.Contracts.DTOs
{
    // Para listar/visualizar
    public record CategoriaDto(
        int Id,
        string Nome,
        string? Descricao,
        bool Ativo,
        int TotalArtigos
    );

    public record CriarCategoriaDto(
        string Nome,
        string? Descricao
    );

    public record AtualizarCategoriaDto(
        string Nome,
        string? Descricao
    );
}