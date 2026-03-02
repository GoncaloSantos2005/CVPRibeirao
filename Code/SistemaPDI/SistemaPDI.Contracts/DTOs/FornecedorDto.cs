namespace SistemaPDI.Contracts.DTOs
{
    // Para listar/visualizar
    public record FornecedorDto(
        int Id,
        string Nome,
        string NIF,
        string? Email,
        string? Telefone,
        string? PessoaContacto,
        string? Morada,
        string? CodigoPostal,
        string? Localidade,
        int TempoEntrega,
        string? Observacoes,
        bool Ativo,
        bool Preferencial,
        DateTime CriadoEm,
        int TotalEncomendas
    );

    // Para criar
    public record CriarFornecedorDto(
        string Nome,
        string NIF,
        string? Email,
        string? Telefone,
        string? PessoaContacto,
        string? Morada,
        string? CodigoPostal,
        string? Localidade,
        int TempoEntrega,
        string? Observacoes
    );

    // Para editar
    public record AtualizarFornecedorDto(
        string Nome,
        string NIF,
        string? Email,
        string? Telefone,
        string? PessoaContacto,
        string? Morada,
        string? CodigoPostal,
        string? Localidade,
        int TempoEntrega,
        string? Observacoes
    );

    // Para dropdown (simplificado)
    public record FornecedorDropdownDto(
        int Id,
        string Nome,
        bool Preferencial,
        int TempoEntrega
    );
}