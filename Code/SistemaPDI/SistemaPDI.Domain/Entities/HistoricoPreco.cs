namespace SistemaPDI.Domain.Entities
{
    /// <summary>
    /// Regista o histórico de preços pagos por artigo em cada encomenda
    /// Permite análise de evolução de preços e comparação entre fornecedores
    /// </summary>
    public class HistoricoPreco
    {
        // ══════════════════════════════════════════════════════════════════════
        // IDENTIFICAÇÃO
        // ══════════════════════════════════════════════════════════════════════
        public int Id { get; set; }

        // ══════════════════════════════════════════════════════════════════════
        // RELACIONAMENTOS
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>Artigo ao qual se refere este preço</summary>
        public int ArtigoId { get; set; }
        public Artigo Artigo { get; set; } = null!;

        /// <summary>Fornecedor que vendeu a este preço</summary>
        public int FornecedorId { get; set; }
        public Fornecedor Fornecedor { get; set; } = null!;

        /// <summary>Encomenda onde este preço foi praticado (opcional)</summary>
        public int? EncomendaId { get; set; }
        public Encomenda? Encomenda { get; set; }

        // ══════════════════════════════════════════════════════════════════════
        // DADOS DO PREÇO
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>Preço unitário pago (com IVA incluído)</summary>
        public decimal PrecoUnitario { get; set; }

        /// <summary>Quantidade comprada nesta transação</summary>
        public int Quantidade { get; set; }

        /// <summary>Valor total desta linha (PrecoUnitario × Quantidade)</summary>
        public decimal ValorTotal { get; set; }

        /// <summary>Data da compra/receção</summary>
        public DateTime DataCompra { get; set; }

        // ══════════════════════════════════════════════════════════════════════
        // OBSERVAÇÕES
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>Observações sobre esta compra (ex: promoção, desconto negociado)</summary>
        public string? Observacoes { get; set; }

        // ══════════════════════════════════════════════════════════════════════
        // AUDITORIA
        // ══════════════════════════════════════════════════════════════════════
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public string? CriadoPor { get; set; }
    }
}