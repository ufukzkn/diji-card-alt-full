using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace digitalbusinesscard.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivateProfileSystemFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "UserPreferences");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "PrivateAccessPassword",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PrivateProfileAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccessToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateProfileAccesses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDefinitionValues_UserId",
                table: "UserDefinitionValues",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateProfileAccess_AccessToken",
                table: "PrivateProfileAccesses",
                column: "AccessToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrivateProfileAccess_UserId",
                table: "PrivateProfileAccesses",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrivateProfileAccesses");

            migrationBuilder.DropIndex(
                name: "IX_UserDefinitionValues_UserId",
                table: "UserDefinitionValues");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PrivateAccessPassword",
                table: "Users");

            migrationBuilder.RenameIndex(
                name: "IX_UserDefinitionValues_UserId_DefinitionId_Unique",
                table: "UserDefinitionValues",
                newName: "IX_UserDefinitionValues_UserId_DefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_ProfileVisits_ProfileUserId_VisitedAt",
                table: "ProfileVisits",
                newName: "IX_ProfileVisits_ProfileUserId_VisitedAtUtc");

            migrationBuilder.AlterColumn<string>(
                name: "ViewMode",
                table: "UserPreferences",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValue: "list");

            migrationBuilder.AlterColumn<string>(
                name: "ThemeColor",
                table: "UserPreferences",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldDefaultValue: "orange");

            migrationBuilder.AlterColumn<int>(
                name: "GridColumns",
                table: "UserPreferences",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 3);

            migrationBuilder.AlterColumn<string>(
                name: "FontFamily",
                table: "UserPreferences",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true,
                oldDefaultValue: "Inter");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserPreferences",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "UserPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserDefinitionValues",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "CustomDefinitionName",
                table: "UserDefinitionValues",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefinitionId1",
                table: "UserDefinitionValues",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDefinitionValues_DefinitionId1",
                table: "UserDefinitionValues",
                column: "DefinitionId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserDefinitionValues_Definitions_DefinitionId1",
                table: "UserDefinitionValues",
                column: "DefinitionId1",
                principalTable: "Definitions",
                principalColumn: "DefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserDefinitionValues_Users_UserId",
                table: "UserDefinitionValues",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferences_Users_UserId",
                table: "UserPreferences",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
