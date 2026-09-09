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
    public DbSet<DeviceExchangeRateSetting> DeviceExchangeRateSettings => Set<DeviceExchangeRateSetting>();
    public DbSet<DeviceWeatherSetting> DeviceWeatherSettings => Set<DeviceWeatherSetting>();
    public DbSet<DailyReference> DailyReferences => Set<DailyReference>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();

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

            entity.Property(u => u.MustChangePassword)
                .HasColumnName("must_change_password")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(u => u.SessionVersion)
                .HasColumnName("session_version")
                .HasDefaultValue(1)
                .IsRequired();
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

            entity.Property(d => d.PendingPowerCommandId)
                .HasColumnName("pending_power_command_id");

            entity.Property(d => d.PendingPowerCommandType)
                .HasColumnName("pending_power_command_type")
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(d => d.PendingPowerCommandRequestedAt)
                .HasColumnName("pending_power_command_requested_at");

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

        modelBuilder.Entity<DeviceExchangeRateSetting>(entity =>
        {
            entity.ToTable("device_exchange_rate_settings");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.DeviceId)
                .HasColumnName("device_id")
                .IsRequired();

            entity.Property(x => x.SeriesId)
                .HasColumnName("series_id")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.DisplayName)
                .HasColumnName("display_name")
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(x => x.Unit)
                .HasColumnName("unit")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.Position)
                .HasColumnName("position")
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            entity.HasOne(x => x.Device)
                .WithMany(x => x.ExchangeRateSettings)
                .HasForeignKey(x => x.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.DeviceId, x.SeriesId })
                .IsUnique();

            entity.HasIndex(x => new { x.DeviceId, x.Position })
                .IsUnique();
        });

        modelBuilder.Entity<DeviceWeatherSetting>(entity =>
        {
            entity.ToTable("device_weather_settings");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.DeviceId)
                .HasColumnName("device_id")
                .IsRequired();

            entity.Property(x => x.LocationName)
                .HasColumnName("location_name")
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(x => x.Latitude)
                .HasColumnName("latitude")
                .IsRequired();

            entity.Property(x => x.Longitude)
                .HasColumnName("longitude")
                .IsRequired();

            entity.Property(x => x.Timezone)
                .HasColumnName("timezone")
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            entity.HasOne(x => x.Device)
                .WithOne(x => x.WeatherSetting)
                .HasForeignKey<DeviceWeatherSetting>(x => x.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.DeviceId)
                .IsUnique();
        });

        modelBuilder.Entity<DailyReference>(entity =>
        {
            entity.ToTable("daily_references");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ReferenceNumber)
                .HasColumnName("reference_number")
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(x => x.ReferenceDate)
                .HasColumnName("reference_date")
                .IsRequired();
            entity.Property(x => x.Client)
                .HasColumnName("client")
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(x => x.OperationCode)
                .HasColumnName("operation_code")
                .HasMaxLength(5)
                .IsRequired();
            entity.Property(x => x.OperationDisplayName)
                .HasColumnName("operation_display_name")
                .HasMaxLength(30)
                .IsRequired();
            entity.Property(x => x.Document)
                .HasColumnName("document")
                .HasMaxLength(30)
                .IsRequired();
            entity.Property(x => x.CustomsOfficeNumber)
                .HasColumnName("customs_office_number")
                .IsRequired();
            entity.Property(x => x.CustomsOffice)
                .HasColumnName("customs_office")
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(x => x.StatusCode)
                .HasColumnName("status_code")
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(x => x.StatusDescription)
                .HasColumnName("status_description")
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(x => x.LastExternalUpdateAt)
                .HasColumnName("last_external_update_at")
                .IsRequired();
            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();
            entity.Property(x => x.CreatedByUserId)
                .HasColumnName("created_by_user_id")
                .IsRequired();

            entity.HasIndex(x => x.ReferenceNumber).IsUnique();

            entity.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Id)
                .HasColumnName("id");

            entity.Property(p => p.Code)
                .HasColumnName("code")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(p => p.Description)
                .HasColumnName("description")
                .HasMaxLength(200)
                .IsRequired();

            entity.HasIndex(p => p.Code)
                .IsUnique();
        });

        modelBuilder.Entity<Permission>().HasData(
            new Permission
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Code = "dashboard.view",
                Description = "Ver resumen"
            },
            new Permission
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Code = "devices.view",
                Description = "Ver dispositivos"
            },
            new Permission
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Code = "devices.manage",
                Description = "Administrar dispositivos"
            },
            new Permission
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                Code = "media.view",
                Description = "Ver archivos multimedia"
            },
            new Permission
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000005"),
                Code = "media.manage",
                Description = "Administrar archivos multimedia"
            },
            new Permission
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000006"),
                Code = "playlists.view",
                Description = "Ver playlists"
            },
            new Permission
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000007"),
                Code = "playlists.manage",
                Description = "Administrar playlists"
            },
            new Permission
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000008"),
                Code = "assignments.view",
                Description = "Ver asignaciones"
            },
            new Permission
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000009"),
                Code = "assignments.manage",
                Description = "Administrar asignaciones"
            },
            new Permission
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000010"),
                Code = "references.view",
                Description = "Ver referencias"
            },
            new Permission
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000011"),
                Code = "references.manage",
                Description = "Administrar referencias"
            },
            new Permission
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000012"),
                Code = "sync_logs.view",
                Description = "Ver registros de sincronización"
            },
            new Permission
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000013"),
                Code = "users.manage",
                Description = "Administrar usuarios"
            });

        modelBuilder.Entity<UserPermission>(entity =>
        {
            entity.ToTable("user_permissions");

            entity.HasKey(up => new
            {
                up.UserId,
                up.PermissionId
            });

            entity.Property(up => up.UserId)
                .HasColumnName("user_id");

            entity.Property(up => up.PermissionId)
                .HasColumnName("permission_id");

            entity.HasOne(up => up.User)
                .WithMany(u => u.UserPermissions)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(up => up.Permission)
                .WithMany(p => p.UserPermissions)
                .HasForeignKey(up => up.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(up => up.PermissionId);
        });
    }
}
