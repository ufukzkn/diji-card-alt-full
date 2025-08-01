using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace diji_card_alt_full.Migrations
{
    /// <inheritdoc />
    public partial class AddTestUserPasswords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tüm kullanıcıların şifrelerini UserId+123. formatında güncelle
            migrationBuilder.Sql(@"
                UPDATE ""Users"" 
                SET ""Password"" = ""UserId"" || '123.' 
                WHERE ""UserId"" IS NOT NULL AND ""UserId"" != '';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Şifreleri temizle
            migrationBuilder.Sql(@"
                UPDATE ""Users"" SET ""Password"" = '';
            ");
        }
    }
}
