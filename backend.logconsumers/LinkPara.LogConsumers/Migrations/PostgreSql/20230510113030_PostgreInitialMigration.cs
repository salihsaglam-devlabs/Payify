using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.LogConsumers.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PostgreInitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.CreateTable(
                name: "audit_log",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    log_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    source_application = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    resource = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    operation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    user_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_success = table.Column<bool>(type: "boolean", nullable: false),
                    channel = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    details = table.Column<string>(type: "text", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entity_change_log",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    table_name = table.Column<string>(type: "text", nullable: true),
                    schema_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    crud_operation_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    client_ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    key_values = table.Column<string>(type: "text", nullable: true),
                    old_values = table.Column<string>(type: "text", nullable: true),
                    new_values = table.Column<string>(type: "text", nullable: true),
                    affected_columns = table.Column<string>(type: "text", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entity_change_log", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log",
                schema: "core");

            migrationBuilder.DropTable(
                name: "entity_change_log",
                schema: "core");
        }
    }
}
