using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaPDI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTabelaLotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Artigos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.CreateTable(
                name: "Lotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroLote = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DataValidade = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QtdDisponivel = table.Column<int>(type: "int", nullable: false),
                    ArtigoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lotes_Artigos_ArtigoId",
                        column: x => x.ArtigoId,
                        principalTable: "Artigos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_ArtigoId",
                table: "Lotes",
                column: "ArtigoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lotes");

            migrationBuilder.InsertData(
                table: "Artigos",
                columns: new[] { "Id", "CriadoEm", "Descricao", "Nome", "PrecoMedio", "SKU", "StockCritico", "StockFisico", "StockMinimo", "StockPendente", "StockVirtual", "UltimoPreco" },
                values: new object[] { 1, new DateTime(2026, 2, 20, 15, 12, 57, 402, DateTimeKind.Utc).AddTicks(7988), "", "Compressas Esterilizadas", 0m, "COMP-001", 20, 0, 50, 0, 0, 0m });
        }
    }
}
