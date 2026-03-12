namespace SistemaPDI.Domain.Entities
{
    public class LinhaEncomenda
    {
        // ══════════════════════════════════════════════════════════════════════
        // IDENTIFICAÇÃO
        // ══════════════════════════════════════════════════════════════════════
        public int Id { get; set; }

        // ══════════════════════════════════════════════════════════════════════
        // ARTIGO
        // ══════════════════════════════════════════════════════════════════════
        public int ArtigoId { get; set; }
        public Artigo Artigo { get; set; } = null!;

        // ══════════════════════════════════════════════════════════════════════
        // QUANTIDADES
        // ══════════════════════════════════════════════════════════════════════
        public int QuantidadeEncomendada { get; set; }  
        public int QuantidadeAprovada { get; set; }     
        public int QuantidadeRecebida { get; set; } = 0; 

        // ══════════════════════════════════════════════════════════════════════
        // PREÇOS (preenchido na CONFIRMADA pelo Gestor Logística)
        // ══════════════════════════════════════════════════════════════════════
        public decimal? PrecoUnitario { get; set; }
        public decimal? Subtotal { get; set; }

        // ══════════════════════════════════════════════════════════════════════
        // LOTE (preenchido na CONFIRMADA ou RECEÇÃO)
        // ══════════════════════════════════════════════════════════════════════
        public string? NumeroLote { get; set; }
        public DateTime? DataValidade { get; set; }

        // ══════════════════════════════════════════════════════════════════════
        // LOTE CRIADO (se já foi criado o lote)
        // ══════════════════════════════════════════════════════════════════════
        public int? LoteId { get; set; }
        public Lote? Lote { get; set; }

        // ══════════════════════════════════════════════════════════════════════
        // LOCALIZAÇÃO (onde guardar - preenchido na receção)
        // ══════════════════════════════════════════════════════════════════════
        public int? LocalizacaoId { get; set; }
        public Localizacao? Localizacao { get; set; }

        // ══════════════════════════════════════════════════════════════════════
        // OBSERVAÇÕES (correções, diferenças, etc.)
        // ══════════════════════════════════════════════════════════════════════
        public string? Observacoes { get; set; }

        // ══════════════════════════════════════════════════════════════════════
        // RELACIONAMENTO
        // ══════════════════════════════════════════════════════════════════════
        public int EncomendaId { get; set; }
        public Encomenda Encomenda { get; set; } = null!;
    }
}