using Microsoft.EntityFrameworkCore;
using SistemaPDI.Domain.Entities;
using SistemaPDI.Domain.Enums;

namespace SistemaPDI.Infrastructure.Data
{
    public class PdiDbContext : DbContext
    {
        public PdiDbContext(DbContextOptions<PdiDbContext> options) : base(options)
        {
        }

        public DbSet<Artigo> Artigos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Utilizador> Utilizadores { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<Lote> Lotes { get; set; }
        public DbSet<Localizacao> Localizacoes { get; set; }
        public DbSet<Encomenda> Encomendas { get; set; }
        public DbSet<LinhaEncomenda> LinhasEncomenda { get; set; }
        public DbSet<HistoricoPreco> HistoricosPrecos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // ══════════════════════════════════════════════════════════════════════
            // HISTÓRICO DE PREÇOS
            // ══════════════════════════════════════════════════════════════════════
            modelBuilder.Entity<HistoricoPreco>(entity =>
            {
                entity.HasKey(hp => hp.Id);

                // Preços
                entity.Property(hp => hp.PrecoUnitario)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(hp => hp.ValorTotal)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(hp => hp.Quantidade)
                    .IsRequired();

                // Observações
                entity.Property(hp => hp.Observacoes)
                    .HasMaxLength(500);

                // Auditoria
                entity.Property(hp => hp.CriadoPor)
                    .HasMaxLength(200);

                // Relacionamento com Artigo
                entity.HasOne(hp => hp.Artigo)
                    .WithMany()
                    .HasForeignKey(hp => hp.ArtigoId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relacionamento com Fornecedor
                entity.HasOne(hp => hp.Fornecedor)
                    .WithMany()
                    .HasForeignKey(hp => hp.FornecedorId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relacionamento com Encomenda (opcional)
                entity.HasOne(hp => hp.Encomenda)
                    .WithMany()
                    .HasForeignKey(hp => hp.EncomendaId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);

                // Índices para performance
                entity.HasIndex(hp => hp.ArtigoId);
                entity.HasIndex(hp => hp.FornecedorId);
                entity.HasIndex(hp => hp.DataCompra);
                entity.HasIndex(hp => new { hp.ArtigoId, hp.FornecedorId }); 
            });

            // ══════════════════════════════════════════════════════════════════════
            // ENCOMENDA
            // ══════════════════════════════════════════════════════════════════════
            modelBuilder.Entity<Encomenda>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Número único
                entity.Property(e => e.NumeroEncomenda)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(e => e.NumeroEncomenda).IsUnique();

                // Estado
                entity.Property(e => e.Estado)
                    .IsRequired()
                    .HasConversion<string>();

                // Valores
                entity.Property(e => e.ValorOrcamento)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.ValorTotal)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(0);

                // Textos
                entity.Property(e => e.Observacoes).HasMaxLength(2000);
                entity.Property(e => e.ObservacoesInternas).HasMaxLength(2000);
                entity.Property(e => e.MotivoRejeicao).HasMaxLength(1000);
                entity.Property(e => e.CaminhoOrcamentoPdf).HasMaxLength(500);

                // Auditoria
                entity.Property(e => e.CriadoPor).HasMaxLength(200);
                entity.Property(e => e.SubmetidoPor).HasMaxLength(200);
                entity.Property(e => e.AprovadoPor).HasMaxLength(200);
                entity.Property(e => e.RejeitadoPor).HasMaxLength(200);
                entity.Property(e => e.GeradoPdfPor).HasMaxLength(200);
                entity.Property(e => e.ConfirmadaPor).HasMaxLength(200);

                // Relacionamento com Fornecedor (opcional até aprovação)
                entity.HasOne(e => e.Fornecedor)
                    .WithMany(f => f.Encomendas)
                    .HasForeignKey(e => e.FornecedorId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            });

            // ══════════════════════════════════════════════════════════════════════
            // LINHA ENCOMENDA
            // ══════════════════════════════════════════════════════════════════════
            modelBuilder.Entity<LinhaEncomenda>(entity =>
            {
                entity.HasKey(le => le.Id);

                // Quantidades
                entity.Property(le => le.QuantidadeEncomendada).IsRequired();
                entity.Property(le => le.QuantidadeAprovada).HasDefaultValue(0);
                entity.Property(le => le.QuantidadeRecebida).HasDefaultValue(0);

                // Preços (opcionais até confirmação)
                entity.Property(le => le.PrecoUnitario)
                    .HasColumnType("decimal(18,2)");

                entity.Property(le => le.Subtotal)
                    .HasColumnType("decimal(18,2)");

                // Lote
                entity.Property(le => le.NumeroLote).HasMaxLength(100);

                // Observações
                entity.Property(le => le.Observacoes).HasMaxLength(1000);

                // Relacionamento com Encomenda
                entity.HasOne(le => le.Encomenda)
                    .WithMany(e => e.Linhas)
                    .HasForeignKey(le => le.EncomendaId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relacionamento com Artigo
                entity.HasOne(le => le.Artigo)
                    .WithMany()
                    .HasForeignKey(le => le.ArtigoId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relacionamento com Lote (opcional)
                entity.HasOne(le => le.Lote)
                    .WithMany()
                    .HasForeignKey(le => le.LoteId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);

                // Relacionamento com Localização (opcional)
                entity.HasOne(le => le.Localizacao)
                    .WithMany()
                    .HasForeignKey(le => le.LocalizacaoId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);
            });

            // ── Configuração de Localizacao ──────────────────────────────────
            modelBuilder.Entity<Localizacao>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Corrigido: Usar 'Codigo' ao invés de 'Descricao'
                entity.Property(e => e.Codigo)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Tipo)
                    .IsRequired()
                    .HasConversion<string>();

                entity.Property(e => e.Zona)
                    .HasMaxLength(100);

                entity.Property(e => e.Prateleira)
                    .HasMaxLength(50);

                // Corrigido: Índice único na propriedade 'Codigo' (evitar duplicados)
                entity.HasIndex(e => e.Codigo)
                    .IsUnique()
                    .HasDatabaseName("IX_Localizacao_Codigo");
            });

            // ── Configuração de Lote ─────────────────────────────────────────
            modelBuilder.Entity<Lote>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.EmTrafico)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(e => e.NumeroLote)
                    .HasMaxLength(50)
                    .IsRequired(false);

                entity.Property(e => e.DataValidade)
                    .IsRequired(false);

                entity.Property(e => e.PrecoUnitario)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired(false);

                // Quantidades (sempre obrigatórias)
                entity.Property(e => e.QtdDisponivel)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(e => e.QtdReservada)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(e => e.Ativo)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(e => e.CriadoEm)
                    .IsRequired();

                entity.HasIndex(e => new { e.ArtigoId, e.NumeroLote })
                    .IsUnique()
                    .HasFilter("[NumeroLote] IS NOT NULL")
                    .HasDatabaseName("IX_Lote_Artigo_NumeroLote");

                // Índices adicionais
                entity.HasIndex(e => e.EmTrafico)
                    .HasDatabaseName("IX_Lote_EmTrafico");

                entity.HasIndex(e => e.DataValidade)
                    .HasDatabaseName("IX_Lote_DataValidade");

                entity.HasIndex(e => e.ArtigoId)
                    .HasDatabaseName("IX_Lote_ArtigoId");

                entity.HasIndex(e => e.LocalizacaoId)
                    .HasDatabaseName("IX_Lote_LocalizacaoId");

                // Relação: Lote → Artigo
                entity.HasOne(l => l.Artigo)
                    .WithMany(a => a.Lotes)
                    .HasForeignKey(l => l.ArtigoId);

                // Relação: Lote → Localizacao
                entity.HasOne(l => l.Localizacao)
                    .WithMany(loc => loc.Lotes)
                    .HasForeignKey(l => l.LocalizacaoId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ── Configuração de Fornecedor ───────────────────────────────────
            modelBuilder.Entity<Fornecedor>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Nome)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.HasIndex(e => e.Nome).IsUnique();

                entity.Property(e => e.NIF)
                    .IsRequired()
                    .HasMaxLength(20);
                entity.HasIndex(e => e.NIF).IsUnique();

                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.Telefone).HasMaxLength(20);
                entity.Property(e => e.PessoaContacto).HasMaxLength(200);
                entity.Property(e => e.Morada).HasMaxLength(500);
                entity.Property(e => e.CodigoPostal).HasMaxLength(10);
                entity.Property(e => e.Localidade).HasMaxLength(100);
                entity.Property(e => e.Observacoes).HasMaxLength(1000);
                entity.Property(e => e.TempoEntrega).HasDefaultValue(15);
            });

            // ── Configuração de Categoria ────────────────────────────────────
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.HasIndex(e => e.Nome).IsUnique();
            });

            // ── Configuração de Artigo ───────────────────────────────────────
            modelBuilder.Entity<Artigo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.Property(e => e.SKU).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UrlImagem).HasMaxLength(500);
                entity.Property(e => e.PrecoMedio).HasColumnType("decimal(18,2)");
                entity.Property(e => e.UltimoPreco).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DesativadoPor).HasMaxLength(200);

                entity.HasOne(a => a.Categoria)
                      .WithMany(c => c.Artigos)
                      .HasForeignKey(a => a.CategoriaId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Configuração de Utilizador ───────────────────────────────────
            modelBuilder.Entity<Utilizador>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.HasIndex(e => e.Email).IsUnique();

                entity.Property(e => e.NomeCompleto)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.PasswordHash)
                    .IsRequired();

                entity.Property(e => e.Perfil)
                    .IsRequired()
                    .HasConversion<string>();
            });
        }
    }
}