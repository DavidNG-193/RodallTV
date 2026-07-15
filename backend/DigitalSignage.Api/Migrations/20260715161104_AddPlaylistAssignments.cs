using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalSignage.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaylistAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "playlist_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    playlist_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    unassigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_playlist_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_playlist_assignments_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_playlist_assignments_playlists_playlist_id",
                        column: x => x.playlist_id,
                        principalTable: "playlists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_playlist_assignments_users_assigned_by_user_id",
                        column: x => x.assigned_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_playlist_assignments_assigned_by_user_id",
                table: "playlist_assignments",
                column: "assigned_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_playlist_assignments_device_id",
                table: "playlist_assignments",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_playlist_assignments_device_id_playlist_id_is_active",
                table: "playlist_assignments",
                columns: new[] { "device_id", "playlist_id", "is_active" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_playlist_assignments_playlist_id",
                table: "playlist_assignments",
                column: "playlist_id");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX ux_playlist_assignments_active_device " +
                "ON playlist_assignments (device_id) " +
                "WHERE is_active = TRUE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "playlist_assignments");

            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS ux_playlist_assignments_active_device;");
        }
    }
}
