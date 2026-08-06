using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalSignage.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceExchangeRateSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "device_exchange_rate_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    series_id = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    display_name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_exchange_rate_settings", x => x.id);
                    table.ForeignKey(
                        name: "FK_device_exchange_rate_settings_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_device_exchange_rate_settings_device_id_position",
                table: "device_exchange_rate_settings",
                columns: new[] { "device_id", "position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_device_exchange_rate_settings_device_id_series_id",
                table: "device_exchange_rate_settings",
                columns: new[] { "device_id", "series_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "device_exchange_rate_settings");
        }
    }
}
