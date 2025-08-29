using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace digitalbusinesscard.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserPreferencesAddFontFamily : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "Layout",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UserPreferences");

            migrationBuilder.AddColumn<string>(
                name: "FontFamily",
                table: "UserPreferences",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FontFamily",
                table: "UserPreferences");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UserPreferences",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Layout",
                table: "UserPreferences",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserPreferences",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
