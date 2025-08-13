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

            // ProfileVisit indexleri - en sık sorgular: belirli profilin ziyaretleri (zaman sıralı)
            modelBuilder.Entity<ProfileVisit>()
                .HasIndex(pv => new { pv.ProfileUserId, pv.VisitedAtUtc });

            modelBuilder.Entity<ProfileVisit>()
                .HasIndex(pv => pv.VisitorUserId);
        }

        public DbSet<User> Users { get; set; }

            public DbSet<Definition> Definitions { get; set; }  

            public DbSet<UserDefinitionValue> UserDefinitionValues { get; set; }
            public DbSet<ProfileVisit> ProfileVisits { get; set; }

        // UserDefinitionValue kayıtları için veri bütünlüğü koruması:
        //  - DefinitionId != 11 ise CustomDefinitionName daima NULL olmalı
        //  - DefinitionId == 11 ise CustomDefinitionName boş / whitespace olamaz
        //  - "[default]" gibi placeholder değerler NULL'a çekilir
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<UserDefinitionValue>())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    if (entry.Entity.DefinitionId != 11)
                    {
                        // Regular definition: asla custom name tutulmaz
                        entry.Entity.CustomDefinitionName = null;
                    }
                    else
                    {
                        // Custom definition: isim zorunlu
                        if (string.IsNullOrWhiteSpace(entry.Entity.CustomDefinitionName))
                        {
                            throw new InvalidOperationException("Custom definition için 'CustomDefinitionName' boş olamaz.");
                        }
                        entry.Entity.CustomDefinitionName = entry.Entity.CustomDefinitionName!.Trim();
                    }

                    // Global placeholder temizliği
                    if (string.Equals(entry.Entity.CustomDefinitionName, "[default]", StringComparison.OrdinalIgnoreCase))
                    {
                        // Eğer custom değilse null'a çek; custom ise kullanıcıya ait gerçek isim olsun diye hata fırlat
                        if (entry.Entity.DefinitionId != 11)
                        {
                            entry.Entity.CustomDefinitionName = null;
                        }
                        else
                        {
                            throw new InvalidOperationException("'[default]' geçersiz bir custom definition ismidir.");
                        }
                    }
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
    }
