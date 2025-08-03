using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace diji_card_alt_full.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDefinitionId1Shadow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sadece sütun varsa drop et - constraint'ler otomatik kalkacak
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF EXISTS (SELECT 1 FROM information_schema.columns 
                              WHERE table_name = 'UserDefinitionValues' 
                              AND column_name = 'DefinitionId1') THEN
                        ALTER TABLE ""UserDefinitionValues"" DROP COLUMN ""DefinitionId1"";
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefinitionId1",
                table: "UserDefinitionValues",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDefinitionValues_DefinitionId1",
                table: "UserDefinitionValues",
                column: "DefinitionId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserDefinitionValues_Definitions_DefinitionId1",
                table: "UserDefinitionValues",
                column: "DefinitionId1",
                principalTable: "Definitions",
                principalColumn: "DefinitionId");
        }
    }
}
