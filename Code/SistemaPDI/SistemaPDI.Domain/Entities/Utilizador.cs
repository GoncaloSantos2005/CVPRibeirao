using SistemaPDI.Domain.Enums;
using System;

namespace SistemaPDI.Domain.Entities
{
    /// <summary>
    /// Representa um utilizador do sistema com credenciais de acesso.
    /// </summary>
    public class Utilizador
    {
        /// <summary>Identificador único do utilizador.</summary>
        public int Id { get; set; }

        /// <summary>Email do utilizador (usado para login).</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Nome completo do utilizador.</summary>
        public string NomeCompleto { get; set; } = string.Empty;

        /// <summary>Hash da password (BCrypt).</summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>Perfil/role do utilizador (Socorrista, Gestor, Administrador).</summary>
        public Perfil Perfil { get; set; }

        /// <summary>Indica se o utilizador está ativo.</summary>
        public bool Ativo { get; set; } = true;

        /// <summary>Data de criação do registo.</summary>
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        /// <summary>Data do último login.</summary>
        public DateTime? UltimoLogin { get; set; }
    }
}
