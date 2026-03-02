using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaPDI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarLocalizacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocalizacaoFisica",
                table: "Lotes");

            migrationBuilder.AddColumn<int>(
                name: "LocalizacaoId",
                table: "Lotes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Localizacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descricao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Zona = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Prateleira = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localizacoes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lote_LocalizacaoId",
                table: "Lotes",
                column: "LocalizacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Localizacao_Descricao",
                table: "Localizacoes",
                column: "Descricao",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Lotes_Localizacoes_LocalizacaoId",
                table: "Lotes",
                column: "LocalizacaoId",
                principalTable: "Localizacoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lotes_Localizacoes_LocalizacaoId",
                table: "Lotes");

            migrationBuilder.DropTable(
                name: "Localizacoes");

            migrationBuilder.DropIndex(
                name: "IX_Lote_LocalizacaoId",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "LocalizacaoId",
                table: "Lotes");

            migrationBuilder.AddColumn<string>(
                name: "LocalizacaoFisica",
                table: "Lotes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
