using SistemaPDI.Domain.Enums;

namespace SistemaPDI.Contracts.DTOs
{
    /// <summary>
    /// DTO para registo de um novo utilizador no sistema.
    /// </summary>
    /// <param name="Email">Email do utilizador (será normalizado para minúsculas).</param>
    /// <param name="Password">Password em texto simples (será encriptada com BCrypt).</param>
    /// <param name="NomeCompleto">Nome completo do utilizador.</param>
    /// <param name="Perfil">Perfil de permissões (SOCORRISTA, GESTOR, ADMINISTRADOR).</param>
    public record RegistarUtilizadorDto(
        string Email,
        string Password,
        string NomeCompleto,
        Perfil Perfil
    );

    /// <summary>
    /// DTO para autenticação de utilizador.
    /// </summary>
    /// <param name="Email">Email do utilizador.</param>
    /// <param name="Password">Password em texto simples.</param>
    public record LoginDto(
        string Email,
        string Password
    );

    /// <summary>
    /// DTO de resposta após login bem-sucedido.
    /// </summary>
    /// <param name="Token">Token JWT (se aplicável).</param>
    /// <param name="Utilizador">Dados do utilizador autenticado.</param>
    public record LoginResponseDto(
        string Token,
        UtilizadorDto Utilizador
    );

    /// <summary>
    /// DTO com dados públicos do utilizador (sem informações sensíveis).
    /// </summary>
    /// <param name="Id">Identificador único.</param>
    /// <param name="Email">Email do utilizador.</param>
    /// <param name="NomeCompleto">Nome completo.</param>
    /// <param name="Perfil">Perfil de permissões.</param>
    /// <param name="Ativo">Estado da conta.</param>
    /// <param name="CriadoEm">Data de criação.</param>
    /// <param name="UltimoLogin">Data do último acesso.</param>
    public record UtilizadorDto(
        int Id,
        string Email,
        string NomeCompleto,
        string Perfil,
        bool Ativo,
        DateTime CriadoEm,
        DateTime? UltimoLogin
    );

    /// <summary>
    /// DTO para atualização de dados de um utilizador existente.
    /// </summary>
    /// <param name="NomeCompleto">Novo nome completo.</param>
    /// <param name="Perfil">Novo perfil de permissões.</param>
    /// <param name="Ativo">Novo estado da conta.</param>
    public record AtualizarUtilizadorDto(
        string NomeCompleto,
        Perfil Perfil,
        bool Ativo
    );

    /// <summary>
    /// DTO para redefinição de password por administrador.
    /// </summary>
    /// <param name="NovaPassword">Nova password (mínimo 6 caracteres).</param>
    public record ResetPasswordDto(
        string NovaPassword
    );
}
