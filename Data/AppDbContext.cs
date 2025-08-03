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

            // Auto-increment primary key
            modelBuilder.Entity<UserDefinitionValue>()
                .HasKey(udv => udv.Id);

            // Foreign key to Users table
            modelBuilder.Entity<UserDefinitionValue>()
                .HasOne(udv => udv.User)
                .WithMany()
                .HasForeignKey(udv => udv.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Foreign key to Definitions table
            modelBuilder.Entity<UserDefinitionValue>()
                .HasOne(udv => udv.Definition)
                .WithMany()
                .HasForeignKey(udv => udv.DefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Regular definitions için unique constraint (DefinitionId != 11)
            // Bir kullanıcı aynı regular definition'dan sadece 1 tane ekleyebilir
            // Custom definitions (DefinitionId = 11) için serbest - birden fazla eklenebilir
            modelBuilder.Entity<UserDefinitionValue>()
                .HasIndex(udv => new { udv.UserId, udv.DefinitionId })
                .IsUnique()
                .HasFilter("\"DefinitionId\" != 11");
        }

        public DbSet<User> Users { get; set; }

            public DbSet<Definition> Definitions { get; set; }  

            public DbSet<UserDefinitionValue> UserDefinitionValues { get; set; }
    }
    }
