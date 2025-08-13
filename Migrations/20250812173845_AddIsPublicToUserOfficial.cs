using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace diji_card_alt_full.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPublicToUserOfficial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfileVisits",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProfileUserId = table.Column<string>(type: "text", nullable: false),
                    VisitorUserId = table.Column<string>(type: "text", nullable: true),
                    VisitedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VisitorIpHash = table.Column<string>(type: "text", nullable: true),
                    UserAgentHash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileVisits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileVisits_Users_ProfileUserId",
                        column: x => x.ProfileUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfileVisits_Users_VisitorUserId",
                        column: x => x.VisitorUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileVisits_ProfileUserId_VisitedAtUtc",
                table: "ProfileVisits",
                columns: new[] { "ProfileUserId", "VisitedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileVisits_VisitorUserId",
                table: "ProfileVisits",
                column: "VisitorUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfileVisits");
        }
    }
}
