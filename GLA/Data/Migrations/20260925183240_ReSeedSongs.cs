using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GLA.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReSeedSongs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Author", "FilePath", "Title" },
                values: new object[] { "Led Zeppelin", "led-zeppelin-stairway_to_heaven.gp4", "Stairway to Heaven" });

            migrationBuilder.UpdateData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Author", "FilePath", "Title" },
                values: new object[] { "Radiohead", "ParanoidAndroid.gp5", "Paranoid Android" });

            migrationBuilder.UpdateData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Author", "FilePath", "Title" },
                values: new object[] { "Soundgarden", "BlackHoleSun.gp4", "Black Hole Sun" });

            migrationBuilder.InsertData(
                table: "Songs",
                columns: new[] { "Id", "Author", "CreatedAt", "FilePath", "Title", "UpdatedAt" },
                values: new object[] { 4, "Metallica", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "NothingElseMatters.gp5", "Nothing Else Matters", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Author", "FilePath", "Title" },
                values: new object[] { "Radiohead", "/ParanoidAndroid.gp5", "Paranoid Android" });

            migrationBuilder.UpdateData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Author", "FilePath", "Title" },
                values: new object[] { "Soundgarden", "/BlackHoleSun.gp4", "Black Hole Sun" });

            migrationBuilder.UpdateData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Author", "FilePath", "Title" },
                values: new object[] { "Metallica", "/NothingElseMatters.gp5", "Nothing Else Matters" });
        }
    }
}
