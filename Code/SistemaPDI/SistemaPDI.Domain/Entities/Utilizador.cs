using SistemaPDI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaPDI.Domain.Entities
{
    public class Utilizador
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string NomeCompleto { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public Perfil Perfil { get; set; }
        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? UltimoLogin { get; set; }
    }
}
