using SistemaPDI.Domain.Enums;

namespace SistemaPDI.Domain.Entities
{
    public class Encomenda
    {
        // ══════════════════════════════════════════════════════════════════════
        // IDENTIFICAÇÃO
        // ══════════════════════════════════════════════════════════════════════
        public int Id { get; set; }
        public string NumeroEncomenda { get; set; } = string.Empty; // ENC-2026-001

        // ══════════════════════════════════════════════════════════════════════
        // DATAS
        // ══════════════════════════════════════════════════════════════════════
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataEnvioFornecedor { get; set; }
        public DateTime? DataEntregaPrevista { get; set; }
        public DateTime? DataEntregaReal { get; set; }

        // ══════════════════════════════════════════════════════════════════════
        // ESTADO
        // ══════════════════════════════════════════════════════════════════════
        public EstadoEncomenda Estado { get; set; } = EstadoEncomenda.LISTA;

        // ══════════════════════════════════════════════════════════════════════
        // FORNECEDOR (preenchido só após aprovação)
        // ══════════════════════════════════════════════════════════════════════
        public int? FornecedorId { get; set; }
        public Fornecedor? Fornecedor { get; set; }

        // ══════════════════════════════════════════════════════════════════════
        // ORÇAMENTO
        // ══════════════════════════════════════════════════════════════════════
        public string? CaminhoOrcamentoPdf { get; set; } // URL do Azure Blob ou local
        public decimal? ValorOrcamento { get; set; }

        // ══════════════════════════════════════════════════════════════════════
        // VALORES FINAIS (calculado após confirmação)
        // ══════════════════════════════════════════════════════════════════════
        public decimal ValorTotal { get; set; } = 0;

        // ══════════════════════════════════════════════════════════════════════
        // OBSERVAÇÕES
        // ══════════════════════════════════════════════════════════════════════
        public string? Observacoes { get; set; } // Públicas
        public string? ObservacoesInternas { get; set; } // Só para equipa

        // ══════════════════════════════════════════════════════════════════════
        // WORKFLOW: LISTA → RASCUNHO
        // ══════════════════════════════════════════════════════════════════════
        public DateTime? GeradoPdfEm { get; set; }
        public string? GeradoPdfPor { get; set; }

        // ══════════════════════════════════════════════════════════════════════
        // WORKFLOW: RASCUNHO → PENDENTE
        // ══════════════════════════════════════════════════════════════════════
        public DateTime? SubmetidoEm { get; set; }
        public string? SubmetidoPor { get; set; }

        // ══════════════════════════════════════════════════════════════════════
        // WORKFLOW: PENDENTE → CONFIRMADA (aprovação)
        // ══════════════════════════════════════════════════════════════════════
        public DateTime? AprovadoEm { get; set; }
        public string? AprovadoPor { get; set; }

        // ══════════════════════════════════════════════════════════════════════
        // WORKFLOW: Rejeição
        // ══════════════════════════════════════════════════════════════════════
        public DateTime? RejeitadoEm { get; set; }
        public string? RejeitadoPor { get; set; }
        public string? MotivoRejeicao { get; set; }

        // ══════════════════════════════════════════════════════════════════════
        // WORKFLOW: CONFIRMADA → ENVIADA
        // ══════════════════════════════════════════════════════════════════════
        public DateTime? ConfirmadaEm { get; set; }
        public string? ConfirmadaPor { get; set; }

        // ══════════════════════════════════════════════════════════════════════
        // AUDITORIA
        // ══════════════════════════════════════════════════════════════════════
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? AtualizadoEm { get; set; }
        public string? CriadoPor { get; set; }
        public bool Ativo { get; set; } = true;

        // ══════════════════════════════════════════════════════════════════════
        // RELACIONAMENTOS
        // ══════════════════════════════════════════════════════════════════════
        public ICollection<LinhaEncomenda> Linhas { get; set; } = new List<LinhaEncomenda>();
    }
}