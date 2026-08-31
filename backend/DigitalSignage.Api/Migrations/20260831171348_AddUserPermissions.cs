using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DigitalSignage.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "must_change_password",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "session_version",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_permissions",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_permissions", x => new { x.user_id, x.permission_id });
                    table.ForeignKey(
                        name: "FK_user_permissions_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_permissions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "id", "code", "description" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "dashboard.view", "Ver resumen" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "devices.view", "Ver dispositivos" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "devices.manage", "Administrar dispositivos" },
                    { new Guid("10000000-0000-0000-0000-000000000004"), "media.view", "Ver archivos multimedia" },
                    { new Guid("10000000-0000-0000-0000-000000000005"), "media.manage", "Administrar archivos multimedia" },
                    { new Guid("10000000-0000-0000-0000-000000000006"), "playlists.view", "Ver playlists" },
                    { new Guid("10000000-0000-0000-0000-000000000007"), "playlists.manage", "Administrar playlists" },
                    { new Guid("10000000-0000-0000-0000-000000000008"), "assignments.view", "Ver asignaciones" },
                    { new Guid("10000000-0000-0000-0000-000000000009"), "assignments.manage", "Administrar asignaciones" },
                    { new Guid("10000000-0000-0000-0000-000000000010"), "references.view", "Ver referencias" },
                    { new Guid("10000000-0000-0000-0000-000000000011"), "references.manage", "Administrar referencias" },
                    { new Guid("10000000-0000-0000-0000-000000000012"), "sync_logs.view", "Ver registros de sincronización" },
                    { new Guid("10000000-0000-0000-0000-000000000013"), "users.manage", "Administrar usuarios" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_permissions_code",
                table: "permissions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_permissions_permission_id",
                table: "user_permissions",
                column: "permission_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_permissions");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropColumn(
                name: "must_change_password",
                table: "users");

            migrationBuilder.DropColumn(
                name: "session_version",
                table: "users");
        }
    }
}
