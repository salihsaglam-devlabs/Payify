using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Fraud.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PostgreInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.CreateTable(
                name: "search_log",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    search_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    birth_year = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    search_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    match_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    match_rate = table.Column<int>(type: "integer", nullable: false),
                    is_black_list = table.Column<bool>(type: "boolean", nullable: false),
                    blacklist_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_search_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transaction_monitoring",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    command_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    transfer_request = table.Column<string>(type: "text", nullable: false),
                    command_json = table.Column<string>(type: "text", nullable: false),
                    sender_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    receiver_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    transaction_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    total_score = table.Column<int>(type: "integer", nullable: false),
                    monitoring_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    risk_level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    error_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    error_message = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transaction_monitoring", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "triggered_rule_set_key",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    rule_set_key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_triggered_rule_set_key", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "integration_log",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_monitoring_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request = table.Column<string>(type: "text", nullable: true),
                    response = table.Column<string>(type: "text", nullable: true),
                    is_success = table.Column<bool>(type: "boolean", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_integration_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_integration_log_transaction_monitoring_transaction_monitori",
                        column: x => x.transaction_monitoring_id,
                        principalSchema: "core",
                        principalTable: "transaction_monitoring",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "triggered_rule",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    rule_key = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false),
                    transaction_monitoring_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_triggered_rule", x => x.id);
                    table.ForeignKey(
                        name: "fk_triggered_rule_transaction_monitoring_transaction_monitorin",
                        column: x => x.transaction_monitoring_id,
                        principalSchema: "core",
                        principalTable: "transaction_monitoring",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_integration_log_transaction_monitoring_id",
                schema: "core",
                table: "integration_log",
                column: "transaction_monitoring_id");

            migrationBuilder.CreateIndex(
                name: "ix_triggered_rule_transaction_monitoring_id",
                schema: "core",
                table: "triggered_rule",
                column: "transaction_monitoring_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "integration_log",
                schema: "core");

            migrationBuilder.DropTable(
                name: "search_log",
                schema: "core");

            migrationBuilder.DropTable(
                name: "triggered_rule",
                schema: "core");

            migrationBuilder.DropTable(
                name: "triggered_rule_set_key",
                schema: "core");

            migrationBuilder.DropTable(
                name: "transaction_monitoring",
                schema: "core");
        }
    }
}
