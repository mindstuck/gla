using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GLA.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefreshSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: 2,
                column: "FilePath",
                value: "radiohead-paranoid_android.gp4");

            migrationBuilder.UpdateData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: 3,
                column: "FilePath",
                value: "soundgarden-black_hole_sun.gp3");

            migrationBuilder.UpdateData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: 4,
                column: "FilePath",
                value: "metallica-nothing_else_matters.gp4");

            migrationBuilder.UpdateData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "FilePath", "Title" },
                values: new object[] { "satie-erik-gnossienne_no_2.gp4", "Gnossienne No. 2" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: 2,
                column: "FilePath",
                value: "ParanoidAndroid.gp5");

            migrationBuilder.UpdateData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: 3,
                column: "FilePath",
                value: "BlackHoleSun.gp4");

            migrationBuilder.UpdateData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: 4,
                column: "FilePath",
                value: "NothingElseMatters.gp5");

            migrationBuilder.UpdateData(
                table: "Songs",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "FilePath", "Title" },
                values: new object[] { "satie_erik-gnossienne_no_1.gp4", "Gnossienne No. 1" });
        }
    }
}
