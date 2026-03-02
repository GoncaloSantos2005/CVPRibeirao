using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaPDI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NomeDescritivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Descricao",
                table: "Localizacoes",
                newName: "Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_Localizacao_Descricao",
                table: "Localizacoes",
                newName: "IX_Localizacao_Codigo");

            migrationBuilder.AlterColumn<string>(
                name: "Zona",
                table: "Localizacoes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nivel",
                table: "Localizacoes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observacoes",
                table: "Localizacoes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nivel",
                table: "Localizacoes");

            migrationBuilder.DropColumn(
                name: "Observacoes",
                table: "Localizacoes");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "Localizacoes",
                newName: "Descricao");

            migrationBuilder.RenameIndex(
                name: "IX_Localizacao_Codigo",
                table: "Localizacoes",
                newName: "IX_Localizacao_Descricao");

            migrationBuilder.AlterColumn<string>(
                name: "Zona",
                table: "Localizacoes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
