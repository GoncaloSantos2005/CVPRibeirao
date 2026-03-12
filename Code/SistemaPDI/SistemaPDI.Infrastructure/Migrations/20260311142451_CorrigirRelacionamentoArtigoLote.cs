using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaPDI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CorrigirRelacionamentoArtigoLote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lotes_Artigos_ArtigoId",
                table: "Lotes");

            migrationBuilder.AddForeignKey(
                name: "FK_Lotes_Artigos_ArtigoId",
                table: "Lotes",
                column: "ArtigoId",
                principalTable: "Artigos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lotes_Artigos_ArtigoId",
                table: "Lotes");

            migrationBuilder.AddForeignKey(
                name: "FK_Lotes_Artigos_ArtigoId",
                table: "Lotes",
                column: "ArtigoId",
                principalTable: "Artigos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
