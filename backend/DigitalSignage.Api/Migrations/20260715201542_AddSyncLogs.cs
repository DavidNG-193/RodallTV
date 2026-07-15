using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalSignage.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sync_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    playlist_id = table.Column<Guid>(type: "uuid", nullable: true),
                    synced_version = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    finished_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    result = table.Column<string>(type: "text", nullable: false),
                    message = table.Column<string>(type: "text", nullable: true),
                    downloaded_files_count = table.Column<int>(type: "integer", nullable: false),
                    deleted_files_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_sync_logs_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sync_logs_playlists_playlist_id",
                        column: x => x.playlist_id,
                        principalTable: "playlists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sync_logs_device_id",
                table: "sync_logs",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_sync_logs_playlist_id",
                table: "sync_logs",
                column: "playlist_id");

            // Add check constraints using SQL
            migrationBuilder.Sql(
                "ALTER TABLE sync_logs ADD CONSTRAINT ck_sync_logs_result " +
                "CHECK (result IN ('Success', 'Failed', 'NoChanges'));"
            );

            migrationBuilder.Sql(
                "ALTER TABLE sync_logs ADD CONSTRAINT ck_sync_logs_downloaded_files_count " +
                "CHECK (downloaded_files_count >= 0);"
            );

            migrationBuilder.Sql(
                "ALTER TABLE sync_logs ADD CONSTRAINT ck_sync_logs_deleted_files_count " +
                "CHECK (deleted_files_count >= 0);"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sync_logs");
        }
    }
}
