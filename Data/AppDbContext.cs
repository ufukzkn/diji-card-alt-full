    using Microsoft.EntityFrameworkCore;
    using diji_card_alt.Models;
    using System.Collections.Generic;


namespace diji_card_alt.Data
    {
        public class AppDbContext : DbContext
        {
            public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserDefinitionValue>()
                .HasKey(udv => new { udv.UserId, udv.DefinitionId });
        }

        public DbSet<User> Users { get; set; }

            public DbSet<Definition> Definitions { get; set; }  

            public DbSet<UserDefinitionValue> UserDefinitionValues { get; set; }
    }
    }
