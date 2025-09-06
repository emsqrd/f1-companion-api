using Microsoft.EntityFrameworkCore;
using F1CompanionApi.Data.Models;

namespace F1CompanionApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Add your DbSets here
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(36); // UUID length
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AccountId).IsUnique();
            entity.HasOne(e => e.Account)
                    .WithOne(e => e.Profile)
                    .HasForeignKey<UserProfile>(e => e.AccountId)
                    .OnDelete(DeleteBehavior.Cascade);
        });

    }
}