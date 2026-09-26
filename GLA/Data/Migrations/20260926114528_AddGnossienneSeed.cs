using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GLA.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGnossienneSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Songs",
                columns: new[] { "Id", "Author", "CreatedAt", "FilePath", "Title", "UpdatedAt" },
                values: new object[] { 5, "Erik Satie", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "satie_erik-gnossienne_no_1.gp4", "Gnossienne No. 1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
