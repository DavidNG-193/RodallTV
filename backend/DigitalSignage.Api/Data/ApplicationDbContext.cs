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
    public DbSet<Media> MediaFiles { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<PlaylistItem> PlaylistItems { get; set; }
    public DbSet<PlaylistAssignment> PlaylistAssignments { get; set; }
    public DbSet<SyncLog> SyncLogs { get; set; }

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

        modelBuilder.Entity<Media>(entity =>
        {
            entity.ToTable("media");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.OriginalFileName)
                .HasColumnName("original_file_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.StoredFileName)
                .HasColumnName("stored_file_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.FileExtension)
                .HasColumnName("file_extension")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.MimeType)
                .HasColumnName("mime_type")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.MediaType)
                .HasColumnName("media_type")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.FileSizeBytes)
                .HasColumnName("file_size_bytes")
                .IsRequired();

            entity.Property(e => e.DurationSeconds)
                .HasColumnName("duration_seconds");

            entity.Property(e => e.FilePath)
                .HasColumnName("file_path")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.HashSha256)
                .HasColumnName("hash_sha256")
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(e => e.UploadedAt)
                .HasColumnName("uploaded_at")
                .IsRequired();

            entity.Property(e => e.UploadedByUserId)
                .HasColumnName("uploaded_by_user_id")
                .IsRequired();

            entity.Property(e => e.MediaFolderId)
                .HasColumnName("media_folder_id");

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            entity.HasIndex(e => e.StoredFileName).IsUnique();
            entity.HasIndex(e => e.HashSha256);
            entity.HasIndex(e => e.UploadedByUserId);
            entity.HasIndex(e => e.MediaFolderId);

            entity.HasOne(e => e.UploadedByUser)
                .WithMany()
                .HasForeignKey(e => e.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.MediaFolder)
                .WithMany()
                .HasForeignKey(e => e.MediaFolderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.ToTable("playlists");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            entity.Property(e => e.Version)
                .HasColumnName("version")
                .IsRequired();

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            entity.Property(e => e.CreatedByUserId)
                .HasColumnName("created_by_user_id")
                .IsRequired();

            entity.HasIndex(e => e.Name)
                .IsUnique();

            entity.HasIndex(e => e.CreatedByUserId);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PlaylistItem>(entity =>
        {
            entity.ToTable("playlist_items");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.PlaylistId)
                .HasColumnName("playlist_id")
                .IsRequired();

            entity.Property(e => e.MediaId)
                .HasColumnName("media_id")
                .IsRequired();

            entity.Property(e => e.Position)
                .HasColumnName("position")
                .IsRequired();

            entity.Property(e => e.CustomDurationSeconds)
                .HasColumnName("custom_duration_seconds");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.HasIndex(e => new { e.PlaylistId, e.Position })
                .IsUnique();

            entity.HasOne(e => e.Playlist)
                .WithMany(p => p.Items)
                .HasForeignKey(e => e.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Media)
                .WithMany()
                .HasForeignKey(e => e.MediaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PlaylistAssignment>(entity =>
        {
            entity.ToTable("playlist_assignments");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.DeviceId)
                .HasColumnName("device_id")
                .IsRequired();

            entity.Property(e => e.PlaylistId)
                .HasColumnName("playlist_id")
                .IsRequired();

            entity.Property(e => e.AssignedByUserId)
                .HasColumnName("assigned_by_user_id")
                .IsRequired();

            entity.Property(e => e.AssignedAt)
                .HasColumnName("assigned_at")
                .IsRequired();

            entity.Property(e => e.UnassignedAt)
                .HasColumnName("unassigned_at");

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            entity.HasIndex(e => new { e.DeviceId, e.PlaylistId, e.IsActive })
                .IsUnique();

            entity.HasIndex(e => e.DeviceId);
            entity.HasIndex(e => e.PlaylistId);
            entity.HasIndex(e => e.AssignedByUserId);

            entity.HasOne(e => e.Device)
                .WithMany()
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Playlist)
                .WithMany()
                .HasForeignKey(e => e.PlaylistId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AssignedByUser)
                .WithMany()
                .HasForeignKey(e => e.AssignedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SyncLog>(entity =>
        {
            entity.ToTable("sync_logs");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.DeviceId).HasColumnName("device_id");

            entity.Property(e => e.PlaylistId).HasColumnName("playlist_id");

            entity.Property(e => e.SyncedVersion).HasColumnName("synced_version");

            entity.Property(e => e.StartedAt).HasColumnName("started_at");

            entity.Property(e => e.FinishedAt).HasColumnName("finished_at");

            entity.Property(e => e.Result).HasColumnName("result");

            entity.Property(e => e.Message).HasColumnName("message");

            entity.Property(e => e.DownloadedFilesCount).HasColumnName("downloaded_files_count");

            entity.Property(e => e.DeletedFilesCount).HasColumnName("deleted_files_count");

            entity.HasOne(e => e.Device)
                .WithMany()
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Playlist)
                .WithMany()
                .HasForeignKey(e => e.PlaylistId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}