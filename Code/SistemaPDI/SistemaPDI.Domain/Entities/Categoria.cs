using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaPDI.Domain.Entities
{
    /// <summary>
    /// Representa uma categoria de artigos para organização do inventário.
    /// </summary>
    public class Categoria
    {
        /// <summary>Identificador único da categoria.</summary>
        public int Id { get; set; }

        /// <summary>Nome da categoria.</summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>Descrição da categoria.</summary>
        public string? Descricao { get; set; }

        /// <summary>Indica se a categoria está ativa.</summary>
        public bool Ativo { get; set; } = true;

        /// <summary>Data de criação do registo.</summary>
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        /// <summary>Lista de artigos desta categoria.</summary>
        public ICollection<Artigo> Artigos { get; set; } = new List<Artigo>();
    }
}
