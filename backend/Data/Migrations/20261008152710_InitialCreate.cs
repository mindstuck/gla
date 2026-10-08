using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GLA.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Songs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Author = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Songs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Songs",
                columns: new[] { "Id", "Author", "CreatedAt", "FilePath", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Led Zeppelin", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "led-zeppelin-stairway_to_heaven.gp4", "Stairway to Heaven", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "Radiohead", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ParanoidAndroid.gp5", "Paranoid Android", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "Soundgarden", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BlackHoleSun.gp4", "Black Hole Sun", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "Metallica", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "NothingElseMatters.gp5", "Nothing Else Matters", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, "Erik Satie", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "satie_erik-gnossienne_no_1.gp4", "Gnossienne No. 1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Songs");
        }
    }
}
