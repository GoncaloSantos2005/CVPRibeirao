using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaPDI.Domain.Entities
{
    public class Lote
    {
        public int Id { get; set; }
        public string NumeroLote { get; set; }
        public DateTime DataValidade { get; set; }
        public int QtdDisponivel { get; set; }

        // Relacionamento
        public int ArtigoId { get; set; }
        public Artigo Artigo { get; set; }
    }
}
