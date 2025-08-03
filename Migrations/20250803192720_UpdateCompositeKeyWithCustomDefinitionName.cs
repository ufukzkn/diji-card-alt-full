using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace diji_card_alt_full.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCompositeKeyWithCustomDefinitionName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Veritabanı zaten istediğimiz yapıda, hiçbir şey yapmaya gerek yok
            // CustomDefinitionName sütunu zaten var ve composite key zaten ayarlanmış
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDefinitionValues",
                table: "UserDefinitionValues");

            migrationBuilder.AlterColumn<string>(
                name: "CustomDefinitionName",
                table: "UserDefinitionValues",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDefinitionValues",
                table: "UserDefinitionValues",
                columns: new[] { "UserId", "DefinitionId" });
        }
    }
}
