namespace SistemaPDI.Domain.Entities
{
    public class Artigo
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string? UrlImagem { get; set; }

        // ── Categoria ────────────────────────────────────────────────────────
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; } = null!;

        // ── Stock ─────────────────────────────────────────────────────────────
        public int StockFisico { get; set; } = 0;
        public int StockVirtual { get; set; } = 0;
        public int StockPendente { get; set; } = 0;
        public int StockMinimo { get; set; } = 10;
        public int StockCritico { get; set; } = 5;

        // ── Financeiro ────────────────────────────────────────────────────────
        public decimal PrecoMedio { get; set; } = 0;
        public decimal UltimoPreco { get; set; } = 0;

        // ── Auditoria ─────────────────────────────────────────────────────────
        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? AtualizadoEm { get; set; }
        public DateTime? DesativadoEm { get; set; }
        public string? DesativadoPor { get; set; }

        // ── Navegacao futura ──────────────────────────────────────────────────
        // public ICollection<Lote> Lotes { get; set; } = new List<Lote>();

        // ── Metodos de Dominio ────────────────────────────────────────────────

        /// <summary>
        /// Recalcula o StockVirtual.
        /// Formula: StockVirtual = StockFisico + StockPendente
        /// </summary>
        public void RecalcularStockVirtual()
        {
            StockVirtual = StockFisico + StockPendente;
        }

        /// <summary>
        /// Verifica se o artigo necessita de reposicao.
        /// Zona Amarela: StockVirtual menor ou igual a StockMinimo
        /// </summary>
        public bool NecessitaReposicao() => StockVirtual <= StockMinimo;

        /// <summary>
        /// Verifica se o artigo esta em zona critica (rutura iminente).
        /// Zona Vermelha: StockFisico menor ou igual a StockCritico
        /// </summary>
        public bool EstaCritico() => StockFisico <= StockCritico;

        /// <summary>
        /// Calcula a quantidade a encomendar com margem de seguranca de 5%.
        /// Formula: (StockMinimo x 1.05) - StockVirtual
        /// </summary>
        public int SugerirQuantidade()
        {
            var sugestao = (int)Math.Ceiling(StockMinimo * 1.05m) - StockVirtual;
            return sugestao > 0 ? sugestao : 0;
        }

        /// <summary>
        /// Atualiza o preco medio ponderado apos uma nova entrada de stock.
        /// Formula: ((StockFisico x PrecoMedio) + (qtdEntrada x precoEntrada)) / (StockFisico + qtdEntrada)
        /// </summary>
        public void AtualizarPrecoMedio(int qtdEntrada, decimal precoEntrada)
        {
            if (qtdEntrada <= 0) return;

            var totalAtual = StockFisico * PrecoMedio;
            var totalEntrada = qtdEntrada * precoEntrada;
            var novoTotal = StockFisico + qtdEntrada;

            PrecoMedio = novoTotal > 0
                ? (totalAtual + totalEntrada) / novoTotal
                : precoEntrada;

            UltimoPreco = precoEntrada;
        }
    }
}