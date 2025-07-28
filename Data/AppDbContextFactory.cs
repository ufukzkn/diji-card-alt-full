using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using diji_card_alt.Data;

//appearently needed for migrations to work properly with postgresql

namespace diji_card_alt.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=dijicarddb;Username=postgres;Password=sifre");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
