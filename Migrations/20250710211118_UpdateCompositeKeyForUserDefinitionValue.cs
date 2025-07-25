using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace diji_card_alt_full.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCompositeKeyForUserDefinitionValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDefinitionValues",
                table: "UserDefinitionValues");

            migrationBuilder.DropIndex(
                name: "IX_UserDefinitionValues_UserId",
                table: "UserDefinitionValues");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserDefinitionValues");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Definitions",
                newName: "DefinitionName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDefinitionValues",
                table: "UserDefinitionValues",
                columns: new[] { "UserId", "DefinitionId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDefinitionValues",
                table: "UserDefinitionValues");

            migrationBuilder.RenameColumn(
                name: "DefinitionName",
                table: "Definitions",
                newName: "Name");

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
                name: "IX_UserDefinitionValues_UserId",
                table: "UserDefinitionValues",
                column: "UserId");
        }
    }
}
