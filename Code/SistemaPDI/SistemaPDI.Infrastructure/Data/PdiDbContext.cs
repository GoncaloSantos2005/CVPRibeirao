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
        public DbSet<Lote> Lotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Configuração de CategoriaDtos ────────────────────────────────────
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descricao).HasMaxLength(500);

                // Índice único no nome
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

                // Relação: 1 CategoriaDtos → N Artigos
                entity.HasOne(a => a.Categoria)
                      .WithMany(c => c.Artigos)
                      .HasForeignKey(a => a.CategoriaId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Configuração de Lote ─────────────────────────────────────────
            modelBuilder.Entity<Lote>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumeroLote).HasMaxLength(50);

                entity.HasOne(l => l.Artigo)
                      .WithMany()
                      .HasForeignKey(l => l.ArtigoId);
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