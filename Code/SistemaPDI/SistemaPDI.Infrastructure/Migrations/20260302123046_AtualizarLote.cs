using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaPDI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AtualizarLote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lotes_Artigos_ArtigoId",
                table: "Lotes");

            migrationBuilder.RenameIndex(
                name: "IX_Lotes_ArtigoId",
                table: "Lotes",
                newName: "IX_Lote_ArtigoId");

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Lotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CriadoEm",
                table: "Lotes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LocalizacaoFisica",
                table: "Lotes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecoUnitario",
                table: "Lotes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "QtdReservada",
                table: "Lotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Lote_Artigo_NumeroLote",
                table: "Lotes",
                columns: new[] { "ArtigoId", "NumeroLote" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lote_DataValidade",
                table: "Lotes",
                column: "DataValidade");

            migrationBuilder.AddForeignKey(
                name: "FK_Lotes_Artigos_ArtigoId",
                table: "Lotes",
                column: "ArtigoId",
                principalTable: "Artigos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lotes_Artigos_ArtigoId",
                table: "Lotes");

            migrationBuilder.DropIndex(
                name: "IX_Lote_Artigo_NumeroLote",
                table: "Lotes");

            migrationBuilder.DropIndex(
                name: "IX_Lote_DataValidade",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "CriadoEm",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "LocalizacaoFisica",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "PrecoUnitario",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "QtdReservada",
                table: "Lotes");

            migrationBuilder.RenameIndex(
                name: "IX_Lote_ArtigoId",
                table: "Lotes",
                newName: "IX_Lotes_ArtigoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lotes_Artigos_ArtigoId",
                table: "Lotes",
                column: "ArtigoId",
                principalTable: "Artigos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
