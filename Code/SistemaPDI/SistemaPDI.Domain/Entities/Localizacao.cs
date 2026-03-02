using SistemaPDI.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaPDI.Domain.Entities
{
    /// <summary>
    /// Representa uma localização física no armazém.
    /// </summary>
    public class Localizacao
    {
        /// <summary>Identificador único da localização.</summary>
        public int Id { get; set; }

        /// <summary>Código único da localização (ex: "LOC001").</summary>
        public string Codigo { get; set; } = string.Empty;

        /// <summary>Tipo de localização (Prateleira, Armário, Frigorífico, Zona).</summary>
        public TipoLocalizacao Tipo { get; set; }

        /// <summary>Zona onde se encontra (ex: "A", "B", "C").</summary>
        public string Zona { get; set; } = string.Empty;

        /// <summary>Identificação da prateleira (ex: "1", "2", "P1").</summary>
        public string? Prateleira { get; set; }

        /// <summary>Nível/altura (ex: "1", "2", "3").</summary>
        public string? Nivel { get; set; }

        /// <summary>Observações opcionais.</summary>
        public string? Observacoes { get; set; }

        /// <summary>Data de criação do registo.</summary>
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        /// <summary>Estado da localização.</summary>
        public bool Ativo { get; set; } = true;

        /// <summary>
        /// Label gerado dinamicamente (não guardado na BD).
        /// Ex: "PRATELEIRA_A_3_2"
        /// </summary>
        [NotMapped]
        public string Label
        {
            get
            {
                var partes = new List<string> { Tipo.ToString(), Zona };

                if (!string.IsNullOrWhiteSpace(Prateleira))
                    partes.Add(Prateleira);

                if (!string.IsNullOrWhiteSpace(Nivel))
                    partes.Add(Nivel);

                return string.Join("_", partes);
            }
        }

        public virtual ICollection<Lote> Lotes { get; set; } = new List<Lote>();
    }
}
