using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace diji_card_alt_full.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomDefinitionSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CustomDefinitionName sütunu ekle (nullable)
            migrationBuilder.AddColumn<string>(
                name: "CustomDefinitionName",
                table: "UserDefinitionValues",
                type: "text",
                nullable: true);

            // Custom Definition'lar için başlangıç ID'lerini ayarla
            // Identity başlangıç değerini 20'ye ayarlıyoruz (PostgreSQL için)
            migrationBuilder.Sql("SELECT setval('\"Definitions_DefinitionId_seq\"', 19, false);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // CustomDefinitionName sütununu kaldır
            migrationBuilder.DropColumn(
                name: "CustomDefinitionName",
                table: "UserDefinitionValues");
        }
    }
}
