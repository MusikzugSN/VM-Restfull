using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vereinsmanager.Migrations
{
    /// <inheritdoc />
    public partial class AlternateVoiceFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlternateVoices_Voices_AlternativeVoiceId",
                table: "AlternateVoices");

            migrationBuilder.RenameColumn(
                name: "AlternativeVoiceId",
                table: "AlternateVoices",
                newName: "Alternative");

            migrationBuilder.RenameIndex(
                name: "IX_AlternateVoices_VoiceId_AlternativeVoiceId",
                table: "AlternateVoices",
                newName: "IX_AlternateVoices_VoiceId_Alternative");

            migrationBuilder.RenameIndex(
                name: "IX_AlternateVoices_AlternativeVoiceId",
                table: "AlternateVoices",
                newName: "IX_AlternateVoices_Alternative");

            migrationBuilder.AddColumn<int>(
                name: "Alternative",
                table: "Voices",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Voices_Alternative",
                table: "Voices",
                column: "Alternative");

            migrationBuilder.AddForeignKey(
                name: "FK_AlternateVoices_Voices_Alternative",
                table: "AlternateVoices",
                column: "Alternative",
                principalTable: "Voices",
                principalColumn: "VoiceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Voices_Voices_Alternative",
                table: "Voices",
                column: "Alternative",
                principalTable: "Voices",
                principalColumn: "VoiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlternateVoices_Voices_Alternative",
                table: "AlternateVoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Voices_Voices_Alternative",
                table: "Voices");

            migrationBuilder.DropIndex(
                name: "IX_Voices_Alternative",
                table: "Voices");

            migrationBuilder.DropColumn(
                name: "Alternative",
                table: "Voices");

            migrationBuilder.RenameColumn(
                name: "Alternative",
                table: "AlternateVoices",
                newName: "AlternativeVoiceId");

            migrationBuilder.RenameIndex(
                name: "IX_AlternateVoices_VoiceId_Alternative",
                table: "AlternateVoices",
                newName: "IX_AlternateVoices_VoiceId_AlternativeVoiceId");

            migrationBuilder.RenameIndex(
                name: "IX_AlternateVoices_Alternative",
                table: "AlternateVoices",
                newName: "IX_AlternateVoices_AlternativeVoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlternateVoices_Voices_AlternativeVoiceId",
                table: "AlternateVoices",
                column: "AlternativeVoiceId",
                principalTable: "Voices",
                principalColumn: "VoiceId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
