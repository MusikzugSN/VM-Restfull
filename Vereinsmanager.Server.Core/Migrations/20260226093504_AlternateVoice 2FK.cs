using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vereinsmanager.Migrations
{
    /// <inheritdoc />
    public partial class AlternateVoice2FK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Voices_InstrumentId_Name",
                table: "Voices");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AlternateVoices",
                type: "datetime(6)",
                nullable: false)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AlternateVoices",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AlternateVoices",
                type: "datetime(6)",
                nullable: false)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "AlternateVoices",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Voices_InstrumentId",
                table: "Voices",
                column: "InstrumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Voices_InstrumentId",
                table: "Voices");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AlternateVoices");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AlternateVoices");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AlternateVoices");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AlternateVoices");

            migrationBuilder.CreateIndex(
                name: "IX_Voices_InstrumentId_Name",
                table: "Voices",
                columns: new[] { "InstrumentId", "Name" },
                unique: true);
        }
    }
}
