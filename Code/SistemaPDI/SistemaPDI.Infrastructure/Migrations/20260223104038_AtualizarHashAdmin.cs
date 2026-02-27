using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaPDI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AtualizarHashAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Utilizadores",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CriadoEm", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 23, 10, 40, 37, 741, DateTimeKind.Utc).AddTicks(8860), "$2a$11$KZx.vBGhH0FvM5R8f0Z9xOZ0Q3N8h7Y8B8J0Z9xOZ0Q3N8h7Y8B8J" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Utilizadores",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CriadoEm", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 23, 10, 28, 52, 292, DateTimeKind.Utc).AddTicks(9144), "" });
        }
    }
}
