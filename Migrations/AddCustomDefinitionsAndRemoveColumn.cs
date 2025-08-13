using Microsoft.EntityFrameworkCore.Migrations;

public partial class AddCustomDefinitionsAndRemoveColumn : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // CustomDefinitions tablosunu oluştur
        migrationBuilder.CreateTable(
            name: "CustomDefinitions",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<string>(nullable: false),
                DefinitionName = table.Column<string>(nullable: false),
                Value = table.Column<string>(nullable: true),
                SortId = table.Column<int>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CustomDefinitions", x => x.Id);
                table.ForeignKey(
                    name: "FK_CustomDefinitions_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // UserDefinitionValues tablosundan CustomDefinitionName kolonunu sil
        migrationBuilder.DropColumn(
            name: "CustomDefinitionName",
            table: "UserDefinitionValues");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // CustomDefinitions tablosunu geri al
        migrationBuilder.DropTable(
            name: "CustomDefinitions");

        // UserDefinitionValues tablosuna CustomDefinitionName kolonunu geri ekle
        migrationBuilder.AddColumn<string>(
            name: "CustomDefinitionName",
            table: "UserDefinitionValues",
            type: "nvarchar(max)",
            nullable: true);
    }
}
