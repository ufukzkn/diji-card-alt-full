using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace diji_card_alt_full.Migrations
{
    public partial class AddIsSpecialAccessToProfileVisits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSpecialAccess",
                table: "ProfileVisits",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSpecialAccess",
                table: "ProfileVisits");
        }
    }
}
