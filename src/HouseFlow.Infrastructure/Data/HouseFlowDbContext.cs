using HouseFlow.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HouseFlow.Infrastructure.Data;

public class HouseFlowDbContext : DbContext
{
    public HouseFlowDbContext(DbContextOptions<HouseFlowDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<House> Houses => Set<House>();
    public DbSet<HouseMember> HouseMembers => Set<HouseMember>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<MaintenanceType> MaintenanceTypes => Set<MaintenanceType>();
    public DbSet<MaintenanceInstance> MaintenanceInstances => Set<MaintenanceInstance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
        });

        // Organization configuration
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.HasOne(e => e.Owner)
                .WithOne(u => u.DefaultOrganization)
                .HasForeignKey<Organization>(e => e.OwnerId);
        });

        // House configuration
        modelBuilder.Entity<House>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Address).IsRequired();
            entity.HasOne(e => e.Organization)
                .WithMany(o => o.Houses)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // HouseMember configuration
        modelBuilder.Entity<HouseMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.House)
                .WithMany(h => h.Members)
                .HasForeignKey(e => e.HouseId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                .WithMany(u => u.HouseMemberships)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.HouseId, e.UserId }).IsUnique();
        });

        // Device configuration
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.House)
                .WithMany(h => h.Devices)
                .HasForeignKey(e => e.HouseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MaintenanceType configuration
        modelBuilder.Entity<MaintenanceType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.HasOne(e => e.Device)
                .WithMany(d => d.MaintenanceTypes)
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MaintenanceInstance configuration
        modelBuilder.Entity<MaintenanceInstance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Cost).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.MaintenanceType)
                .WithMany(mt => mt.MaintenanceInstances)
                .HasForeignKey(e => e.MaintenanceTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
