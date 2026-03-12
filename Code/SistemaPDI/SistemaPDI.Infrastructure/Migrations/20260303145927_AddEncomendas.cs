using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaPDI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEncomendas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AprovadoEm",
                table: "Encomendas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AprovadoPor",
                table: "Encomendas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoRejeicao",
                table: "Encomendas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejeitadoEm",
                table: "Encomendas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejeitadoPor",
                table: "Encomendas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmetidoEm",
                table: "Encomendas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmetidoPor",
                table: "Encomendas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AprovadoEm",
                table: "Encomendas");

            migrationBuilder.DropColumn(
                name: "AprovadoPor",
                table: "Encomendas");

            migrationBuilder.DropColumn(
                name: "MotivoRejeicao",
                table: "Encomendas");

            migrationBuilder.DropColumn(
                name: "RejeitadoEm",
                table: "Encomendas");

            migrationBuilder.DropColumn(
                name: "RejeitadoPor",
                table: "Encomendas");

            migrationBuilder.DropColumn(
                name: "SubmetidoEm",
                table: "Encomendas");

            migrationBuilder.DropColumn(
                name: "SubmetidoPor",
                table: "Encomendas");
        }
    }
}
