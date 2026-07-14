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
    public DbSet<MediaFolder> MediaFolders { get; set; }

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

        modelBuilder.Entity<MediaFolder>(entity =>
        {
            entity.ToTable("media_folders");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(e => e.CreatedByUserId)
                .HasColumnName("created_by_user_id")
                .IsRequired();

            entity.HasIndex(e => e.Name)
                .IsUnique();

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .IsRequired();
        });
    }
}