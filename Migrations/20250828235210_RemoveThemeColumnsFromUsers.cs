using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace digitalbusinesscard.Migrations
{
    /// <inheritdoc />
    public partial class RemoveThemeColumnsFromUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardLayout",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FontFamily",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ThemeColor",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardLayout",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FontFamily",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThemeColor",
                table: "Users",
                type: "text",
                nullable: true);
        }
    }
}
