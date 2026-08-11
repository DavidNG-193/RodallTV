using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalSignage.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_references",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    reference_date = table.Column<DateOnly>(type: "date", nullable: false),
                    client = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    operation_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    operation_display_name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    document = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    customs_office_number = table.Column<int>(type: "integer", nullable: false),
                    customs_office = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status_description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    last_external_update_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_references", x => x.id);
                    table.ForeignKey(
                        name: "FK_daily_references_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_daily_references_created_by_user_id",
                table: "daily_references",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_daily_references_reference_number",
                table: "daily_references",
                column: "reference_number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_references");
        }
    }
}
