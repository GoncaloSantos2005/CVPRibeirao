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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

                entity.Property(e => e.NumeroLote)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(e => new { e.ArtigoId, e.NumeroLote })
                    .IsUnique()
                    .HasDatabaseName("IX_Lote_Artigo_NumeroLote");

                entity.Property(e => e.PrecoUnitario)
                    .HasColumnType("decimal(18,2)");

                // Relação: Lote → Artigo
                entity.HasOne(l => l.Artigo)
                    .WithMany()
                    .HasForeignKey(l => l.ArtigoId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relação: Lote → Localizacao
                entity.HasOne(l => l.Localizacao)
                    .WithMany(loc => loc.Lotes)
                    .HasForeignKey(l => l.LocalizacaoId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.DataValidade)
                    .HasDatabaseName("IX_Lote_DataValidade");

                entity.HasIndex(e => e.ArtigoId)
                    .HasDatabaseName("IX_Lote_ArtigoId");

                entity.HasIndex(e => e.LocalizacaoId)
                    .HasDatabaseName("IX_Lote_LocalizacaoId");
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