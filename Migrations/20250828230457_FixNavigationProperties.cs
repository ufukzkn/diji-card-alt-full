using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace digitalbusinesscard.Migrations
{
    /// <inheritdoc />
    public partial class FixNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Navigation property düzeltmeleri - manuel olarak düzeltildi
            // Bu migration'da database değişikliği gerekmiyor
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferencesUserId",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_PreferencesUserId",
                table: "Users",
                column: "PreferencesUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserPreferences_PreferencesUserId",
                table: "Users",
                column: "PreferencesUserId",
                principalTable: "UserPreferences",
                principalColumn: "UserId");
        }
    }
}
