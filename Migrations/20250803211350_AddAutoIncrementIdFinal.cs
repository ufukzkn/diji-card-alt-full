using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace diji_card_alt_full.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoIncrementIdFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDefinitionValues",
                table: "UserDefinitionValues");

            //migrationBuilder.DropColumn(
            //  name: "CardLayout",
            //    table: "Users");

            //migrationBuilder.DropColumn(
            //    name: "FontFamily",
            //    table: "Users");

            //migrationBuilder.DropColumn(
            //  name: "ThemeColor",
            //    table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "CustomDefinitionName",
                table: "UserDefinitionValues",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserDefinitionValues",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDefinitionValues",
                table: "UserDefinitionValues",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserDefinitionValues_UserId_DefinitionId",
                table: "UserDefinitionValues",
                columns: new[] { "UserId", "DefinitionId" },
                unique: true,
                filter: "\"DefinitionId\" != 11");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDefinitionValues",
                table: "UserDefinitionValues");

            migrationBuilder.DropIndex(
                name: "IX_UserDefinitionValues_UserId_DefinitionId",
                table: "UserDefinitionValues");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserDefinitionValues");

            // Bu sütunlar zaten yok, geri eklemeye gerek yok!
            // migrationBuilder.AddColumn<string>(name: "CardLayout", table: "Users", type: "text", nullable: true);
            // migrationBuilder.AddColumn<string>(name: "FontFamily", table: "Users", type: "text", nullable: true);
            // migrationBuilder.AddColumn<string>(name: "ThemeColor", table: "Users", type: "text", nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomDefinitionName",
                table: "UserDefinitionValues",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDefinitionValues",
                table: "UserDefinitionValues",
                columns: new[] { "UserId", "DefinitionId", "CustomDefinitionName" });
        }
    }
}
