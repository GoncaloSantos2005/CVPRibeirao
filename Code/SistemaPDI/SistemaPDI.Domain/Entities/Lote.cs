using System;

namespace SistemaPDI.Domain.Entities
{
    /// <summary>
    /// Representa um lote de um artigo no inventário.
    /// </summary>
    public class Lote
    {
        /// <summary>Identificador único do lote.</summary>
        public int Id { get; set; }

        /// <summary>ID do artigo associado (FK).</summary>
        public int ArtigoId { get; set; }

        /// <summary>Artigo associado (navegação).</summary>
        public Artigo Artigo { get; set; } = null!;

        /// <summary>
        /// Indica se o lote está em trânsito (encomenda enviada mas não recebida).
        /// Quando true: NumeroLote, DataValidade, PrecoUnitario podem ser NULL.
        /// Quando false: Todos os campos devem estar preenchidos.
        /// </summary>
        public bool EmTrafico { get; set; } = false;

        /// <summary>Código do lote impresso na embalagem pelo fornecedor.</summary>
        public string? NumeroLote { get; set; } = string.Empty;

        /// <summary>Data de validade do material clínico.</summary>
        public DateTime? DataValidade { get; set; }

        /// <summary>Preço unitário registado durante o Blind Check.</summary>
        public decimal? PrecoUnitario { get; set; }

        /// <summary>
        /// Quantidade física disponível.
        /// - EmTrafico = true: sempre 0
        /// - EmTrafico = false: quantidade real em armazém
        /// </summary>
        public int QtdDisponivel { get; set; }

        /// <summary>
        /// Quantidade reservada/comprometida.
        /// - EmTrafico = true: quantidade esperada da encomenda
        /// - EmTrafico = false: quantidade reservada para saídas futuras
        /// </summary>
        public int QtdReservada { get; set; }

        /// <summary>Estado do lote (false se esgotado ou inutilizado).</summary>
        public bool Ativo { get; set; } = true;

        /// <summary>Data de criação do registo.</summary>
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        /// <summary>ID da localização física (FK - opcional).</summary>
        public int? LocalizacaoId { get; set; }

        /// <summary>Localização física associada (navegação).</summary>
        public Localizacao? Localizacao { get; set; }

        // ── Propriedades Calculadas ───────────────────────────────────────────

        /// <summary>
        /// Quantidade realmente disponível para alocação (RN03 - FEFO).
        /// Lotes em trânsito sempre retornam 0 (não podem ser alocados).
        /// </summary>
        public int QtdRealmenteDisponivel
        {
            get
            {
                // Se está em trânsito, não há nada disponível para alocação
                if (EmTrafico)
                    return 0;

                // Senão, calcular normalmente
                return QtdDisponivel - QtdReservada;
            }
        }

        /// <summary>Verifica se o lote está expirado.</summary>
        public bool EstaExpirado => DataValidade.HasValue && DataValidade.Value.Date < DateTime.UtcNow.Date;

        /// <summary>Verifica se o lote expira nos próximos 15 dias (RN13).</summary>
        public bool ValidadeProxima => DataValidade.HasValue && DataValidade.Value.Date <= DateTime.UtcNow.Date.AddDays(15);

        /// <summary>
        /// Lote só pode ser alocado se:
        /// - NÃO está em trânsito
        /// - NÃO expirou
        /// - Tem quantidade disponível
        /// - Está ativo
        /// </summary>
        public bool PodeSerAlocado => !EmTrafico &&
                                       !EstaExpirado &&
                                       QtdRealmenteDisponivel > 0 &&
                                       Ativo;

        // ── Métodos de Domínio ────────────────────────────────────────────────

        public bool Reservar(int quantidade)
        {
            if (EmTrafico)
                return false;

            if (quantidade <= 0 || quantidade > QtdRealmenteDisponivel)
                return false;
            QtdReservada += quantidade;
            return true;
        }

        public bool LibertarReserva(int quantidade)
        {
            if (quantidade <= 0 || quantidade > QtdReservada)
                return false;
            QtdReservada -= quantidade;
            return true;
        }

        public bool ConfirmarSaida(int quantidade)
        {
            if (EmTrafico)
                return false;

            if (quantidade <= 0 || quantidade > QtdReservada)
                return false;
            QtdReservada -= quantidade;
            QtdDisponivel -= quantidade;
            if (QtdDisponivel <= 0)
                Ativo = false;
            return true;
        }
    }
}