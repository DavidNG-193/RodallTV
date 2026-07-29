using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalSignage.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDevicePowerCommands : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "pending_power_command_id",
                table: "devices",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "pending_power_command_requested_at",
                table: "devices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pending_power_command_type",
                table: "devices",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "pending_power_command_id",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "pending_power_command_requested_at",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "pending_power_command_type",
                table: "devices");
        }
    }
}
