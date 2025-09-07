using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace digitalbusinesscard.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSpecialAccessToProfileVisits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSpecialAccess",
                table: "ProfileVisits",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSpecialAccess",
                table: "ProfileVisits");
        }
    }
}
