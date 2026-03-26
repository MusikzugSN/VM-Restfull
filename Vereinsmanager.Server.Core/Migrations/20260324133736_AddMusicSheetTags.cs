using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vereinsmanager.Migrations
{
    /// <inheritdoc />
    public partial class AddMusicSheetTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MusicSheetTag",
                columns: table => new
                {
                    MusicSheetsMusicSheetId = table.Column<int>(type: "int", nullable: false),
                    TagsTagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicSheetTag", x => new { x.MusicSheetsMusicSheetId, x.TagsTagId });
                    table.ForeignKey(
                        name: "FK_MusicSheetTag_MusicSheets_MusicSheetsMusicSheetId",
                        column: x => x.MusicSheetsMusicSheetId,
                        principalTable: "MusicSheets",
                        principalColumn: "MusicSheetId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MusicSheetTag_Tags_TagsTagId",
                        column: x => x.TagsTagId,
                        principalTable: "Tags",
                        principalColumn: "TagId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MusicSheetTag_TagsTagId",
                table: "MusicSheetTag",
                column: "TagsTagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MusicSheetTag");
        }
    }
}
