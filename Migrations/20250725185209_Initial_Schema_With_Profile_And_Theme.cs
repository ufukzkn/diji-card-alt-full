using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace diji_card_alt_full.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Schema_With_Profile_And_Theme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Iban",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Linkedin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LocationLink",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WhatsappLink",
                table: "Users");

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
                name: "ProfilePhotoUrl",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThemeColor",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortId",
                table: "UserDefinitionValues",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardLayout",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FontFamily",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ThemeColor",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SortId",
                table: "UserDefinitionValues");

            migrationBuilder.AddColumn<string>(
                name: "Iban",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Linkedin",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LocationLink",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WhatsappLink",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
