using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaPDI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCategoriaEAtualizarArtigo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Utilizadores",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "SKU",
                table: "Artigos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Artigos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Artigos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "AtualizadoEm",
                table: "Artigos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CategoriaId",
                table: "Artigos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DesativadoEm",
                table: "Artigos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DesativadoPor",
                table: "Artigos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlImagem",
                table: "Artigos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Artigos_CategoriaId",
                table: "Artigos",
                column: "CategoriaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Artigos_Categorias_CategoriaId",
                table: "Artigos",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Artigos_Categorias_CategoriaId",
                table: "Artigos");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropIndex(
                name: "IX_Artigos_CategoriaId",
                table: "Artigos");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Artigos");

            migrationBuilder.DropColumn(
                name: "AtualizadoEm",
                table: "Artigos");

            migrationBuilder.DropColumn(
                name: "CategoriaId",
                table: "Artigos");

            migrationBuilder.DropColumn(
                name: "DesativadoEm",
                table: "Artigos");

            migrationBuilder.DropColumn(
                name: "DesativadoPor",
                table: "Artigos");

            migrationBuilder.DropColumn(
                name: "UrlImagem",
                table: "Artigos");

            migrationBuilder.AlterColumn<string>(
                name: "SKU",
                table: "Artigos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Artigos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.InsertData(
                table: "Utilizadores",
                columns: new[] { "Id", "Ativo", "CriadoEm", "Email", "NomeCompleto", "PasswordHash", "Perfil", "UltimoLogin" },
                values: new object[] { 1, true, new DateTime(2026, 2, 23, 10, 46, 46, 833, DateTimeKind.Utc).AddTicks(7589), "admin@cvp.pt", "Administrador Sistema", "$2a$11$RJV/NE21xIAO9WLRecar1Ovn6IroYjysWJyxyeTA6cKxqm7uzhX0u", "ADMINISTRADOR", null });
        }
    }
}
