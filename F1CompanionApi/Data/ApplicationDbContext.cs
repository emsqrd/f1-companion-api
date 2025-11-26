using F1CompanionApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace F1CompanionApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // Add your DbSets here
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Constructor> Constructors => Set<Constructor>();
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

        modelBuilder.Entity<League>()
            .HasOne(e => e.Owner)
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

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

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasIndex(e => e.UserId).IsUnique();
            entity
                .HasOne(e => e.Owner)
                .WithOne(u => u.Team)
                .HasForeignKey<Team>(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure audit trail FK for user-owned entities only
        ConfigureAuditTrailForeignKeys<League>(modelBuilder);
        ConfigureAuditTrailForeignKeys<Team>(modelBuilder);
    }

    private void ConfigureAuditTrailForeignKeys<T>(ModelBuilder modelBuilder)
        where T : UserOwnedEntity
    {
        // Configure foreign key relationships for user-owned entities
        modelBuilder
            .Entity<T>()
            .HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<T>()
            .HasOne(e => e.UpdatedByUser)
            .WithMany()
            .HasForeignKey(e => e.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        modelBuilder
            .Entity<T>()
            .HasOne(e => e.DeletedByUser)
            .WithMany()
            .HasForeignKey(e => e.DeletedBy)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
