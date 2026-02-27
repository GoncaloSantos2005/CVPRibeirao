using SistemaPDI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaPDI.Contracts.DTOs
{
    public record RegistarUtilizadorDto(
        string Email,
        string Password,
        string NomeCompleto,
        Perfil Perfil
    );
    public record LoginDto(
        string Email,
        string Password
    );
    public record LoginResponseDto(
        string Token,
        UtilizadorDto Utilizador
    );
    public record UtilizadorDto(
        int Id,
        string Email,
        string NomeCompleto,
        string Perfil,
        bool Ativo,
        DateTime CriadoEm
    );
    public record AtualizarUtilizadorDto(
        string NomeCompleto,
        Perfil Perfil,
        bool Ativo
    );
}
