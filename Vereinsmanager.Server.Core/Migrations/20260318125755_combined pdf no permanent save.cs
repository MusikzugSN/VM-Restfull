using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vereinsmanager.Migrations
{
    /// <inheritdoc />
    public partial class combinedpdfnopermanentsave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecondFilePath",
                table: "MusicSheets",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MusicSheets_SecondFilePath",
                table: "MusicSheets",
                column: "SecondFilePath",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MusicSheets_SecondFilePath",
                table: "MusicSheets");

            migrationBuilder.DropColumn(
                name: "SecondFilePath",
                table: "MusicSheets");
        }
    }
}
