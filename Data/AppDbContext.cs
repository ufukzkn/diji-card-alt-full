    using Microsoft.EntityFrameworkCore;
    using diji_card_alt.Models;
    using DigitalBusinessCard.Models;
    using System.Collections.Generic;


namespace diji_card_alt.Data
    {
        public class AppDbContext : DbContext
        {
            public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // UserDefinitionValue yapılandırması
            modelBuilder.Entity<UserDefinitionValue>(entity =>
            {
                // Primary key
                entity.HasKey(e => e.Id);
                
                // Properties
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DefinitionId).IsRequired();
                entity.Property(e => e.Value).IsRequired();
                entity.Property(e => e.SortId);
                entity.Property(e => e.CustomDefinitionName).HasMaxLength(100);

                // Foreign key constraints - explicit configuration without navigation properties
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_UserDefinitionValues_UserId");
                entity.HasIndex(e => e.DefinitionId).HasDatabaseName("IX_UserDefinitionValues_DefinitionId");
                
                // Unique constraint for regular definitions (not custom)
                entity.HasIndex(e => new { e.UserId, e.DefinitionId })
                      .IsUnique()
                      .HasFilter("\"DefinitionId\" != 11")
                      .HasDatabaseName("IX_UserDefinitionValues_UserId_DefinitionId_Unique");
            });

            // ProfileVisit yapılandırması
            modelBuilder.Entity<ProfileVisit>(entity =>
            {
                entity.HasIndex(e => new { e.ProfileUserId, e.VisitedAtUtc })
                      .HasDatabaseName("IX_ProfileVisits_ProfileUserId_VisitedAt");
                entity.HasIndex(e => e.VisitorUserId)
                      .HasDatabaseName("IX_ProfileVisits_VisitorUserId");
            });

            // UserPreferences yapılandırması
            modelBuilder.Entity<UserPreferences>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.GridColumns).HasDefaultValue(3);
                entity.Property(e => e.ViewMode).HasMaxLength(10).HasDefaultValue("list");
                entity.Property(e => e.ThemeColor).HasMaxLength(20).HasDefaultValue("orange");
                entity.Property(e => e.FontFamily).HasMaxLength(30).HasDefaultValue("Inter");
            });

            // PrivateProfileAccess yapılandırması
            modelBuilder.Entity<PrivateProfileAccess>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.AccessToken).IsRequired().HasMaxLength(500);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.Description).HasMaxLength(200);
                
                entity.HasIndex(e => e.AccessToken).IsUnique().HasDatabaseName("IX_PrivateProfileAccess_AccessToken");
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_PrivateProfileAccess_UserId");
            });
        }

        public DbSet<User> Users { get; set; }

            public DbSet<Definition> Definitions { get; set; }  

            public DbSet<UserDefinitionValue> UserDefinitionValues { get; set; }
            public DbSet<ProfileVisit> ProfileVisits { get; set; }
            public DbSet<UserPreferences> UserPreferences { get; set; }
            public DbSet<PrivateProfileAccess> PrivateProfileAccesses { get; set; }

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
