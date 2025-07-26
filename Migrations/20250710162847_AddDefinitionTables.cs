using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace diji_card_alt_full.Migrations
{
    /// <inheritdoc />
    public partial class AddDefinitionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Definitions",
                columns: table => new
                {
                    DefinitionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Definitions", x => x.DefinitionId);
                });

            migrationBuilder.CreateTable(
                name: "UserDefinitionValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    DefinitionId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDefinitionValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDefinitionValues_Definitions_DefinitionId",
                        column: x => x.DefinitionId,
                        principalTable: "Definitions",
                        principalColumn: "DefinitionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDefinitionValues_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDefinitionValues_DefinitionId",
                table: "UserDefinitionValues",
                column: "DefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDefinitionValues_UserId",
                table: "UserDefinitionValues",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDefinitionValues");

            migrationBuilder.DropTable(
                name: "Definitions");
        }
    }
}
