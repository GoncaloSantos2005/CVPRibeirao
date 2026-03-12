using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaPDI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarEmTraficoLote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Lote_Artigo_NumeroLote",
                table: "Lotes");

            migrationBuilder.AlterColumn<int>(
                name: "QtdReservada",
                table: "Lotes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "QtdDisponivel",
                table: "Lotes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecoUnitario",
                table: "Lotes",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroLote",
                table: "Lotes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataValidade",
                table: "Lotes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "Ativo",
                table: "Lotes",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<bool>(
                name: "EmTrafico",
                table: "Lotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Lote_Artigo_NumeroLote",
                table: "Lotes",
                columns: new[] { "ArtigoId", "NumeroLote" },
                unique: true,
                filter: "[NumeroLote] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Lote_EmTrafico",
                table: "Lotes",
                column: "EmTrafico");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Lote_Artigo_NumeroLote",
                table: "Lotes");

            migrationBuilder.DropIndex(
                name: "IX_Lote_EmTrafico",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "EmTrafico",
                table: "Lotes");

            migrationBuilder.AlterColumn<int>(
                name: "QtdReservada",
                table: "Lotes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "QtdDisponivel",
                table: "Lotes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecoUnitario",
                table: "Lotes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroLote",
                table: "Lotes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataValidade",
                table: "Lotes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Ativo",
                table: "Lotes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lote_Artigo_NumeroLote",
                table: "Lotes",
                columns: new[] { "ArtigoId", "NumeroLote" },
                unique: true);
        }
    }
}
