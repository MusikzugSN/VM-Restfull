using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vereinsmanager.Migrations
{
    /// <inheritdoc />
    public partial class Added_OAuth_and_UniqeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Not needed
            // migrationBuilder.DropIndex(
            //     name: "IX_UserRoles_UserId",
            //     table: "UserRoles");
            //
            // migrationBuilder.DropIndex(
            //     name: "IX_Permissions_RoleId",
            //     table: "Permissions");

            migrationBuilder.AddColumn<string>(
                name: "OAuthSubject",
                table: "Users",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "Users",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Provider_OAuthSubject",
                table: "Users",
                columns: new[] { "Provider", "OAuthSubject" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId_GroupId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId", "GroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_RoleId_PermissionType",
                table: "Permissions",
                columns: new[] { "RoleId", "PermissionType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_Name",
                table: "Groups",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Provider_OAuthSubject",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId_RoleId_GroupId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_Name",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_RoleId_PermissionType",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Groups_Name",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "OAuthSubject",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "Users");

            // Not needed
            // migrationBuilder.CreateIndex(
            //     name: "IX_UserRoles_UserId",
            //     table: "UserRoles",
            //     column: "UserId");
            //
            // migrationBuilder.CreateIndex(
            //     name: "IX_Permissions_RoleId",
            //     table: "Permissions",
            //     column: "RoleId");
        }
    }
}
