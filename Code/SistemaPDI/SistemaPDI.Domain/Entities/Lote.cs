using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaPDI.Domain.Entities
{
    /// <summary>
    /// Representa um lote de um artigo com data de validade.
    /// </summary>
    public class Lote
    {
        /// <summary>Identificador único do lote.</summary>
        public int Id { get; set; }

        /// <summary>Número/código do lote.</summary>
        public string NumeroLote { get; set; } = string.Empty;

        /// <summary>Data de validade do lote.</summary>
        public DateTime DataValidade { get; set; }

        /// <summary>Quantidade disponível neste lote.</summary>
        public int QtdDisponivel { get; set; }

        /// <summary>ID do artigo associado.</summary>
        public int ArtigoId { get; set; }

        /// <summary>Artigo associado (navegação).</summary>
        public Artigo Artigo { get; set; } = null!;
    }
}
