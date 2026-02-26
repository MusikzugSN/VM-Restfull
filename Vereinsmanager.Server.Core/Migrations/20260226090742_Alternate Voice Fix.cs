using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vereinsmanager.Migrations
{
    /// <inheritdoc />
    public partial class AlternateVoiceFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Voices_InstrumentId",
                table: "Voices");

            migrationBuilder.DropIndex(
                name: "IX_MusicFolders_GroupId",
                table: "MusicFolders");

            migrationBuilder.DropIndex(
                name: "IX_AlternateVoices_Alternative_VoiceId_Priority",
                table: "AlternateVoices");

            migrationBuilder.DropIndex(
                name: "IX_AlternateVoices_VoiceId",
                table: "AlternateVoices");

            migrationBuilder.DropColumn(
                name: "Alternative",
                table: "AlternateVoices");

            migrationBuilder.AddColumn<int>(
                name: "AlternativeVoiceId",
                table: "AlternateVoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Voices_InstrumentId_Name",
                table: "Voices",
                columns: new[] { "InstrumentId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Scores_Title",
                table: "Scores",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MusicFolders_GroupId_Name",
                table: "MusicFolders",
                columns: new[] { "GroupId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_Name",
                table: "Instruments",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_Name_Date",
                table: "Events",
                columns: new[] { "Name", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlternateVoices_AlternativeVoiceId",
                table: "AlternateVoices",
                column: "AlternativeVoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_AlternateVoices_VoiceId_AlternativeVoiceId",
                table: "AlternateVoices",
                columns: new[] { "VoiceId", "AlternativeVoiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlternateVoices_VoiceId_Priority",
                table: "AlternateVoices",
                columns: new[] { "VoiceId", "Priority" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AlternateVoices_Voices_AlternativeVoiceId",
                table: "AlternateVoices",
                column: "AlternativeVoiceId",
                principalTable: "Voices",
                principalColumn: "VoiceId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlternateVoices_Voices_AlternativeVoiceId",
                table: "AlternateVoices");

            migrationBuilder.DropIndex(
                name: "IX_Voices_InstrumentId_Name",
                table: "Voices");

            migrationBuilder.DropIndex(
                name: "IX_Scores_Title",
                table: "Scores");

            migrationBuilder.DropIndex(
                name: "IX_MusicFolders_GroupId_Name",
                table: "MusicFolders");

            migrationBuilder.DropIndex(
                name: "IX_Instruments_Name",
                table: "Instruments");

            migrationBuilder.DropIndex(
                name: "IX_Events_Name_Date",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_AlternateVoices_AlternativeVoiceId",
                table: "AlternateVoices");

            migrationBuilder.DropIndex(
                name: "IX_AlternateVoices_VoiceId_AlternativeVoiceId",
                table: "AlternateVoices");

            migrationBuilder.DropIndex(
                name: "IX_AlternateVoices_VoiceId_Priority",
                table: "AlternateVoices");

            migrationBuilder.DropColumn(
                name: "AlternativeVoiceId",
                table: "AlternateVoices");

            migrationBuilder.AddColumn<string>(
                name: "Alternative",
                table: "AlternateVoices",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Voices_InstrumentId",
                table: "Voices",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicFolders_GroupId",
                table: "MusicFolders",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AlternateVoices_Alternative_VoiceId_Priority",
                table: "AlternateVoices",
                columns: new[] { "Alternative", "VoiceId", "Priority" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlternateVoices_VoiceId",
                table: "AlternateVoices",
                column: "VoiceId");
        }
    }
}
