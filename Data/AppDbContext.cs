using Microsoft.EntityFrameworkCore;
using diji_card_alt.Models;
using System.Collections.Generic;

namespace diji_card_alt.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
