using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaPDI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarEncomendas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Encomendas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroEncomenda = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DataEncomenda = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataEntregaPrevista = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataEntregaReal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    Observacoes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CriadoPor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    FornecedorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Encomendas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Encomendas_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LinhasEncomenda",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuantidadeEncomendada = table.Column<int>(type: "int", nullable: false),
                    QuantidadeRecebida = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EncomendaId = table.Column<int>(type: "int", nullable: false),
                    ArtigoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinhasEncomenda", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LinhasEncomenda_Artigos_ArtigoId",
                        column: x => x.ArtigoId,
                        principalTable: "Artigos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LinhasEncomenda_Encomendas_EncomendaId",
                        column: x => x.EncomendaId,
                        principalTable: "Encomendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Encomendas_FornecedorId",
                table: "Encomendas",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Encomendas_NumeroEncomenda",
                table: "Encomendas",
                column: "NumeroEncomenda",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LinhasEncomenda_ArtigoId",
                table: "LinhasEncomenda",
                column: "ArtigoId");

            migrationBuilder.CreateIndex(
                name: "IX_LinhasEncomenda_EncomendaId",
                table: "LinhasEncomenda",
                column: "EncomendaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LinhasEncomenda");

            migrationBuilder.DropTable(
                name: "Encomendas");
        }
    }
}
