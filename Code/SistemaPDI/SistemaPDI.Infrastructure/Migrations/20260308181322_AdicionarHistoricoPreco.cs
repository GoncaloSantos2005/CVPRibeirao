using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaPDI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarHistoricoPreco : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataFabrico",
                table: "LinhasEncomenda");

            migrationBuilder.CreateTable(
                name: "HistoricosPrecos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArtigoId = table.Column<int>(type: "int", nullable: false),
                    FornecedorId = table.Column<int>(type: "int", nullable: false),
                    EncomendaId = table.Column<int>(type: "int", nullable: true),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DataCompra = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observacoes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadoPor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricosPrecos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricosPrecos_Artigos_ArtigoId",
                        column: x => x.ArtigoId,
                        principalTable: "Artigos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistoricosPrecos_Encomendas_EncomendaId",
                        column: x => x.EncomendaId,
                        principalTable: "Encomendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HistoricosPrecos_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricosPrecos_ArtigoId",
                table: "HistoricosPrecos",
                column: "ArtigoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricosPrecos_ArtigoId_FornecedorId",
                table: "HistoricosPrecos",
                columns: new[] { "ArtigoId", "FornecedorId" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricosPrecos_DataCompra",
                table: "HistoricosPrecos",
                column: "DataCompra");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricosPrecos_EncomendaId",
                table: "HistoricosPrecos",
                column: "EncomendaId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricosPrecos_FornecedorId",
                table: "HistoricosPrecos",
                column: "FornecedorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricosPrecos");
        }
    }
}
