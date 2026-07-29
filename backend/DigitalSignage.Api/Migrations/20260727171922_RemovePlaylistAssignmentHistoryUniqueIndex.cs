using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalSignage.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemovePlaylistAssignmentHistoryUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_playlist_assignments_device_id_playlist_id_is_active",
                table: "playlist_assignments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_playlist_assignments_device_id_playlist_id_is_active",
                table: "playlist_assignments",
                columns: new[] { "device_id", "playlist_id", "is_active" },
                unique: true);
        }
    }
}
