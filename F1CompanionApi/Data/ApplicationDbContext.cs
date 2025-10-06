using F1CompanionApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace F1CompanionApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // Add your DbSets here
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<League> Leagues => Set<League>();
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

        modelBuilder.Entity<League>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity
                .HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AccountId).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity
                .HasOne(e => e.Account)
                .WithOne(e => e.Profile)
                .HasForeignKey<UserProfile>(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
