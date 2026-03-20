using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vereinsmanager.Migrations
{
    /// <inheritdoc />
    public partial class Printjob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MusicSheets_FilePath",
                table: "MusicSheets");

            migrationBuilder.DropIndex(
                name: "IX_MusicSheets_SecondFilePath",
                table: "MusicSheets");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "MusicSheets");

            migrationBuilder.DropColumn(
                name: "SecondFilePath",
                table: "MusicSheets");

            migrationBuilder.CreateTable(
                name: "MusicSheetFiles",
                columns: table => new
                {
                    MusicSheetFileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MusicSheetId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Filesize = table.Column<int>(type: "int", nullable: false),
                    PageCount = table.Column<int>(type: "int", nullable: false),
                    FileHash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicSheetFiles", x => x.MusicSheetFileId);
                    table.ForeignKey(
                        name: "FK_MusicSheetFiles_MusicSheets_MusicSheetId",
                        column: x => x.MusicSheetId,
                        principalTable: "MusicSheets",
                        principalColumn: "MusicSheetId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MusicSheetFiles_FilePath",
                table: "MusicSheetFiles",
                column: "FilePath",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MusicSheetFiles_MusicSheetId_SortOrder",
                table: "MusicSheetFiles",
                columns: new[] { "MusicSheetId", "SortOrder" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MusicSheetFiles");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "MusicSheets",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SecondFilePath",
                table: "MusicSheets",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MusicSheets_FilePath",
                table: "MusicSheets",
                column: "FilePath",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MusicSheets_SecondFilePath",
                table: "MusicSheets",
                column: "SecondFilePath",
                unique: true);
        }
    }
}
