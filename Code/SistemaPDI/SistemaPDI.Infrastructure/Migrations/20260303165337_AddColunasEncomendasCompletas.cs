using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaPDI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddColunasEncomendasCompletas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DataEncomenda",
                table: "Encomendas",
                newName: "DataCriacao");

            migrationBuilder.AlterColumn<decimal>(
                name: "Subtotal",
                table: "LinhasEncomenda",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecoUnitario",
                table: "LinhasEncomenda",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataFabrico",
                table: "LinhasEncomenda",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataValidade",
                table: "LinhasEncomenda",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LocalizacaoId",
                table: "LinhasEncomenda",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LoteId",
                table: "LinhasEncomenda",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroLote",
                table: "LinhasEncomenda",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observacoes",
                table: "LinhasEncomenda",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuantidadeAprovada",
                table: "LinhasEncomenda",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "SubmetidoPor",
                table: "Encomendas",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RejeitadoPor",
                table: "Encomendas",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Observacoes",
                table: "Encomendas",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MotivoRejeicao",
                table: "Encomendas",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FornecedorId",
                table: "Encomendas",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "AprovadoPor",
                table: "Encomendas",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CaminhoOrcamentoPdf",
                table: "Encomendas",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmadaEm",
                table: "Encomendas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfirmadaPor",
                table: "Encomendas",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataEnvioFornecedor",
                table: "Encomendas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GeradoPdfEm",
                table: "Encomendas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeradoPdfPor",
                table: "Encomendas",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacoesInternas",
                table: "Encomendas",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorOrcamento",
                table: "Encomendas",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LinhasEncomenda_LocalizacaoId",
                table: "LinhasEncomenda",
                column: "LocalizacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_LinhasEncomenda_LoteId",
                table: "LinhasEncomenda",
                column: "LoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_LinhasEncomenda_Localizacoes_LocalizacaoId",
                table: "LinhasEncomenda",
                column: "LocalizacaoId",
                principalTable: "Localizacoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LinhasEncomenda_Lotes_LoteId",
                table: "LinhasEncomenda",
                column: "LoteId",
                principalTable: "Lotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LinhasEncomenda_Localizacoes_LocalizacaoId",
                table: "LinhasEncomenda");

            migrationBuilder.DropForeignKey(
                name: "FK_LinhasEncomenda_Lotes_LoteId",
                table: "LinhasEncomenda");

            migrationBuilder.DropIndex(
                name: "IX_LinhasEncomenda_LocalizacaoId",
                table: "LinhasEncomenda");

            migrationBuilder.DropIndex(
                name: "IX_LinhasEncomenda_LoteId",
                table: "LinhasEncomenda");

            migrationBuilder.DropColumn(
                name: "DataFabrico",
                table: "LinhasEncomenda");

            migrationBuilder.DropColumn(
                name: "DataValidade",
                table: "LinhasEncomenda");

            migrationBuilder.DropColumn(
                name: "LocalizacaoId",
                table: "LinhasEncomenda");

            migrationBuilder.DropColumn(
                name: "LoteId",
                table: "LinhasEncomenda");

            migrationBuilder.DropColumn(
                name: "NumeroLote",
                table: "LinhasEncomenda");

            migrationBuilder.DropColumn(
                name: "Observacoes",
                table: "LinhasEncomenda");

            migrationBuilder.DropColumn(
                name: "QuantidadeAprovada",
                table: "LinhasEncomenda");

            migrationBuilder.DropColumn(
                name: "CaminhoOrcamentoPdf",
                table: "Encomendas");

            migrationBuilder.DropColumn(
                name: "ConfirmadaEm",
                table: "Encomendas");

            migrationBuilder.DropColumn(
                name: "ConfirmadaPor",
                table: "Encomendas");

            migrationBuilder.DropColumn(
                name: "DataEnvioFornecedor",
                table: "Encomendas");

            migrationBuilder.DropColumn(
                name: "GeradoPdfEm",
                table: "Encomendas");

            migrationBuilder.DropColumn(
                name: "GeradoPdfPor",
                table: "Encomendas");

            migrationBuilder.DropColumn(
                name: "ObservacoesInternas",
                table: "Encomendas");

            migrationBuilder.DropColumn(
                name: "ValorOrcamento",
                table: "Encomendas");

            migrationBuilder.RenameColumn(
                name: "DataCriacao",
                table: "Encomendas",
                newName: "DataEncomenda");

            migrationBuilder.AlterColumn<decimal>(
                name: "Subtotal",
                table: "LinhasEncomenda",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecoUnitario",
                table: "LinhasEncomenda",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SubmetidoPor",
                table: "Encomendas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RejeitadoPor",
                table: "Encomendas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Observacoes",
                table: "Encomendas",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MotivoRejeicao",
                table: "Encomendas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FornecedorId",
                table: "Encomendas",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AprovadoPor",
                table: "Encomendas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
