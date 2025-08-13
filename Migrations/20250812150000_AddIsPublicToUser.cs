using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace diji_card_alt_full.Migrations
{
    [DbContext(typeof(diji_card_alt.Data.AppDbContext))]
    [Migration("20250812150000_AddIsPublicToUser")]
    public partial class AddIsPublicToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Users");
        }
    }
}
