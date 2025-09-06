using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace diji_card_alt.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivateProfileFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Users tablosuna PrivateAccessPassword ekle
            migrationBuilder.AddColumn<string>(
                name: "PrivateAccessPassword",
                table: "Users",
                type: "text",
                nullable: true);

            // 2. PrivateProfileAccesses tablosunu oluştur
            migrationBuilder.CreateTable(
                name: "PrivateProfileAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccessToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateProfileAccesses", x => x.Id);
                });

            // 3. PrivateProfileAccesses indexleri
            migrationBuilder.CreateIndex(
                name: "IX_PrivateProfileAccess_AccessToken",
                table: "PrivateProfileAccesses",
                column: "AccessToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrivateProfileAccess_UserId",
                table: "PrivateProfileAccesses",
                column: "UserId");

            // 4. UserPreferences'ten Users'a IsPublic kopyala (eğer mevcut değilse)
            migrationBuilder.Sql(@"
                UPDATE ""Users"" 
                SET ""IsPublic"" = COALESCE((
                    SELECT ""IsPublic"" 
                    FROM ""UserPreferences"" 
                    WHERE ""UserPreferences"".""UserId"" = ""Users"".""UserId""
                ), true)
                WHERE ""Users"".""IsPublic"" IS NULL OR ""Users"".""IsPublic"" = false
            ");

            // 5. UserPreferences'ten IsPublic'i sil
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "UserPreferences");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback işlemleri
            
            // 1. UserPreferences'e IsPublic'i geri ekle
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "UserPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            // 2. Users'tan UserPreferences'e IsPublic kopyala
            migrationBuilder.Sql(@"
                UPDATE ""UserPreferences"" 
                SET ""IsPublic"" = ""Users"".""IsPublic""
                FROM ""Users""
                WHERE ""UserPreferences"".""UserId"" = ""Users"".""UserId""
            ");

            // 3. PrivateProfileAccesses tablosunu sil
            migrationBuilder.DropTable(
                name: "PrivateProfileAccesses");

            // 4. Users'tan PrivateAccessPassword'ı sil
            migrationBuilder.DropColumn(
                name: "PrivateAccessPassword",
                table: "Users");
        }
    }
}
