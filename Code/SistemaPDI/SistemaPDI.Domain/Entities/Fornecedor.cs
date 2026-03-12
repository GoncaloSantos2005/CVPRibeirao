namespace SistemaPDI.Domain.Entities
{
    /// <summary>
    /// Representa um fornecedor de materiais.
    /// </summary>
    public class Fornecedor
    {
        // ── Identificação ─────────────────────────────────────────────────────

        /// <summary>Identificador único do fornecedor.</summary>
        public int Id { get; set; }

        /// <summary>Nome comercial do fornecedor.</summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>Número de Identificação Fiscal.</summary>
        public string NIF { get; set; } = string.Empty;

        // ── Contactos ─────────────────────────────────────────────────────────

        /// <summary>Email de contacto.</summary>
        public string? Email { get; set; }

        /// <summary>Telefone de contacto.</summary>
        public string? Telefone { get; set; }

        /// <summary>Nome da pessoa de contacto.</summary>
        public string? PessoaContacto { get; set; }

        // ── Morada ────────────────────────────────────────────────────────────

        /// <summary>Morada completa.</summary>
        public string? Morada { get; set; }

        /// <summary>Código postal (XXXX-XXX).</summary>
        public string? CodigoPostal { get; set; }

        /// <summary>Localidade/cidade.</summary>
        public string? Localidade { get; set; }

        // ── Operacional ───────────────────────────────────────────────────────

        /// <summary>Tempo médio de entrega em dias úteis.</summary>
        public int TempoEntrega { get; set; } = 3;

        /// <summary>Observações adicionais.</summary>
        public string? Observacoes { get; set; }

        // ── Estado ────────────────────────────────────────────────────────────

        /// <summary>Indica se o fornecedor está ativo.</summary>
        public bool Ativo { get; set; } = true;

        /// <summary>Indica se é fornecedor preferencial.</summary>
        public bool Preferencial { get; set; } = false;

        // ── Auditoria ─────────────────────────────────────────────────────────

        /// <summary>Data de criação do registo.</summary>
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        /// <summary>Data da última atualização.</summary>
        public DateTime? AtualizadoEm { get; set; }

        // Relacionamentos
        public ICollection<Encomenda> Encomendas { get; set; } = new List<Encomenda>();
        //public ICollection<HistoricoPreco> HistoricosPreco { get; set; } = new List<HistoricoPreco>();
    }
}