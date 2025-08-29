using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace digitalbusinesscard.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPreferencesOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. İlk önce UserPreferences tablosunu oluştur
            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    GridColumns = table.Column<int>(type: "integer", nullable: false),
                    ViewMode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ThemeColor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Layout = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            // 2. Mevcut Users'dan IsPublic verilerini UserPreferences'a kopyala
            migrationBuilder.Sql(@"
                INSERT INTO ""UserPreferences"" (""UserId"", ""IsPublic"", ""GridColumns"", ""ViewMode"", ""ThemeColor"", ""Layout"", ""CreatedAt"", ""UpdatedAt"")
                SELECT ""UserId"", ""IsPublic"", 3, 'list', 'blue', 'default', NOW(), NOW()
                FROM ""Users""
            ");

            // 3. Son olarak Users tablosundan IsPublic kolonunu sil
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Users tablosuna IsPublic kolonunu geri ekle
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // 2. UserPreferences'dan Users'a IsPublic verilerini geri kopyala
            migrationBuilder.Sql(@"
                UPDATE ""Users"" 
                SET ""IsPublic"" = ""UserPreferences"".""IsPublic""
                FROM ""UserPreferences""
                WHERE ""Users"".""UserId"" = ""UserPreferences"".""UserId""
            ");

            // 3. UserPreferences tablosunu sil
            migrationBuilder.DropTable(
                name: "UserPreferences");
        }
    }
}
