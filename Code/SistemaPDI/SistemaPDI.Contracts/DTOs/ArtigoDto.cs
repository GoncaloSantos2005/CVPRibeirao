namespace SistemaPDI.Contracts.DTOs
{   
    public record ArtigoDto(
        int Id,
        string Nome,
        string Descricao,
        string SKU,
        string? UrlImagem,
        string NomeCategoria, 
        int StockFisico,
        int StockVirtual,
        int StockPendente,
        int StockMinimo,
        int StockCritico,
        decimal PrecoMedio,
        decimal UltimoPreco,
        DateTime CriadoEm,
        bool Ativo,
        bool NecessitaReposicao, 
        bool EstaCritico,
        int? CategoriaId,        
        string? CategoriaNome    
    );

    public record CriarArtigoDto(
        string Nome,
        string Descricao,
        string SKU,
        string? UrlImagem,
        int CategoriaId,
        string? CategoriaNome,
        int StockMinimo,
        int StockCritico
    );

    public record AtualizarArtigoDto(
        string Nome,
        string Descricao,
        string SKU,
        string? UrlImagem,
        int CategoriaId,
        string? CategoriaNome,
        int StockMinimo,
        int StockCritico        
    );
}