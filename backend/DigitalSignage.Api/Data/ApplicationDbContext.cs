using DigitalSignage.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Device> Devices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(u => u.Id);

            entity.Property(u => u.Id)
                .HasColumnName("id");

            entity.Property(u => u.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(u => u.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(u => u.Email)
                .HasColumnName("email")
                .HasMaxLength(150)
                .IsRequired();

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.Property(u => u.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(u => u.Role)
                .HasColumnName("role")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(u => u.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            entity.Property(u => u.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(u => u.LastLoginAt)
                .HasColumnName("last_login_at");
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.ToTable("devices");

            entity.HasKey(d => d.Id);

            entity.Property(d => d.Id)
                .HasColumnName("id");

            entity.Property(d => d.DeviceUuid)
                .HasColumnName("device_uuid")
                .IsRequired();

            entity.Property(d => d.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(d => d.Location)
                .HasColumnName("location")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(d => d.AccessTokenHash)
                .HasColumnName("access_token_hash")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(d => d.Status)
                .HasColumnName("status")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(d => d.IpAddress)
                .HasColumnName("ip_address")
                .HasMaxLength(45);

            entity.Property(d => d.CurrentPlaylistVersion)
                .HasColumnName("current_playlist_version")
                .IsRequired();

            entity.Property(d => d.AgentVersion)
                .HasColumnName("agent_version")
                .HasMaxLength(50);

            entity.Property(d => d.LastConnectionAt)
                .HasColumnName("last_connection_at");

            entity.Property(d => d.LastSyncAt)
                .HasColumnName("last_sync_at");

            entity.Property(d => d.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            entity.Property(d => d.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.HasIndex(d => d.DeviceUuid)
                .IsUnique();

            entity.HasIndex(d => d.Name)
                .IsUnique();
        });
    }
}