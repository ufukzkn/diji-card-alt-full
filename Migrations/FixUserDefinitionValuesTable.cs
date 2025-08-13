using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace diji_card_alt_full.Migrations
{
    public partial class FixUserDefinitionValuesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CustomDefinitionName sütununu kaldır
            migrationBuilder.DropColumn(
                name: "CustomDefinitionName",
                table: "UserDefinitionValues");

            // Composite primary key ayarla
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDefinitionValues",
                table: "UserDefinitionValues");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDefinitionValues",
                table: "UserDefinitionValues",
                columns: new[] { "UserId", "DefinitionId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Composite primary key'i kaldır
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDefinitionValues",
                table: "UserDefinitionValues");

            migrationBuilder.AddColumn<string>(
                name: "CustomDefinitionName",
                table: "UserDefinitionValues",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDefinitionValues",
                table: "UserDefinitionValues",
                column: "UserId");
        }
    }
}
