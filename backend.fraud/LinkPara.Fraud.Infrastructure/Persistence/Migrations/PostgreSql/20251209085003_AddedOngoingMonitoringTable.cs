using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Fraud.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedOngoingMonitoringTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "reference_number",
                schema: "core",
                table: "search_log",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ongoing_monitoring",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    search_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    search_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    scan_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    period = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_ongoing_list = table.Column<bool>(type: "boolean", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ongoing_monitoring", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ongoing_monitoring",
                schema: "core");

            migrationBuilder.DropColumn(
                name: "reference_number",
                schema: "core",
                table: "search_log");
        }
    }
}
