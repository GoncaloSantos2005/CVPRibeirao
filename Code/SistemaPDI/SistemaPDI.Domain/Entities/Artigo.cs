namespace SistemaPDI.Domain.Entities
{
    /// <summary>
    /// Representa um artigo/material no inventário da organização.
    /// </summary>
    public class Artigo
    {
        /// <summary>Identificador único do artigo.</summary>
        public int Id { get; set; }

        /// <summary>Nome do artigo.</summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>Descrição detalhada do artigo.</summary>
        public string Descricao { get; set; } = string.Empty;

        /// <summary>Código SKU (Stock Keeping Unit) único do artigo.</summary>
        public string SKU { get; set; } = string.Empty;

        /// <summary>URL da imagem do artigo.</summary>
        public string? UrlImagem { get; set; }

        // ── Categoria ────────────────────────────────────────────────────────

        /// <summary>ID da categoria à qual o artigo pertence.</summary>
        public int CategoriaId { get; set; }

        /// <summary>Categoria do artigo (navegação).</summary>
        public Categoria Categoria { get; set; } = null!;

        // ── Stock ─────────────────────────────────────────────────────────────

        /// <summary>Quantidade física em armazém.</summary>
        public int StockFisico { get; set; } = 0;

        /// <summary>Stock disponível (físico + pendente).</summary>
        public int StockVirtual { get; set; } = 0;

        /// <summary>Quantidade reservada/pendente de entrega.</summary>
        public int StockPendente { get; set; } = 0;

        /// <summary>Stock total = StockFisico + StockVirtual.</summary>
        public int StockTotal => StockFisico + StockVirtual;

        /// <summary>Limite mínimo de stock (alerta amarelo).</summary>
        public int StockMinimo { get; set; } = 10;

        /// <summary>Limite crítico de stock (alerta vermelho).</summary>
        public int StockCritico { get; set; } = 5;

        // ── Financeiro ────────────────────────────────────────────────────────

        /// <summary>Preço médio ponderado do artigo.</summary>
        public decimal PrecoMedio { get; set; } = 0;

        /// <summary>Preço da última entrada de stock.</summary>
        public decimal UltimoPreco { get; set; } = 0;

        // ── Auditoria ─────────────────────────────────────────────────────────

        /// <summary>Indica se o artigo está ativo no sistema.</summary>
        public bool Ativo { get; set; } = true;

        /// <summary>Data de criação do registo.</summary>
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        /// <summary>Data da última atualização.</summary>
        public DateTime? AtualizadoEm { get; set; }

        /// <summary>Data de desativação do artigo.</summary>
        public DateTime? DesativadoEm { get; set; }

        /// <summary>Nome do utilizador que desativou o artigo.</summary>
        public string? DesativadoPor { get; set; }

        // ── Navegacao ──────────────────────────────────────────────────
        public ICollection<Lote> Lotes { get; set; } = new List<Lote>();

        // ── Metodos de Dominio ────────────────────────────────────────────────

        /// <summary>
        /// Verifica se necessita reposição (zona amarela).
        /// </summary>
        public bool NecessitaReposicao() => StockVirtual + StockFisico <= StockMinimo;

        /// <summary>
        /// Verifica se está em zona crítica (zona vermelha).
        /// </summary>
        public bool EstaCritico() => StockFisico <= StockCritico;

        /// <summary>
        /// Sugere quantidade a encomendar com margem de 5%.
        /// </summary>
        public int SugerirQuantidade()
        {
            var sugestao = (int)Math.Ceiling(StockMinimo * 1.05m) - StockVirtual;
            return sugestao > 0 ? sugestao : 0;
        }

        /// <summary>
        /// Atualiza o preço médio ponderado após nova entrada.
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