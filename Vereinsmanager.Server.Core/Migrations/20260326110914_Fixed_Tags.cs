using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vereinsmanager.Migrations
{
    /// <inheritdoc />
    public partial class Fixed_Tags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Tags_TagId1",
                table: "Tags");

            migrationBuilder.DropTable(
                name: "MusicSheetTag");

            migrationBuilder.DropIndex(
                name: "IX_Tags_TagId1",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "TagId1",
                table: "Tags");

            migrationBuilder.CreateTable(
                name: "TagUser",
                columns: table => new
                {
                    TagUserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TagId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MusicSheetId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagUser", x => x.TagUserId);
                    table.ForeignKey(
                        name: "FK_TagUser_MusicSheets_MusicSheetId",
                        column: x => x.MusicSheetId,
                        principalTable: "MusicSheets",
                        principalColumn: "MusicSheetId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TagUser_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "TagId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TagUser_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TagUser_MusicSheetId",
                table: "TagUser",
                column: "MusicSheetId");

            migrationBuilder.CreateIndex(
                name: "IX_TagUser_TagId",
                table: "TagUser",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_TagUser_UserId",
                table: "TagUser",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TagUser");

            migrationBuilder.AddColumn<int>(
                name: "TagId1",
                table: "Tags",
                type: "int",
                nullable: true);

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
                name: "IX_Tags_TagId1",
                table: "Tags",
                column: "TagId1");

            migrationBuilder.CreateIndex(
                name: "IX_MusicSheetTag_TagsTagId",
                table: "MusicSheetTag",
                column: "TagsTagId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Tags_TagId1",
                table: "Tags",
                column: "TagId1",
                principalTable: "Tags",
                principalColumn: "TagId");
        }
    }
}
