using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LinkPara.Approval.Infrastructure.Persistence.Migrations.PostgreSql
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
                name: "case",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    base_url = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    action_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    case_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    resource = table.Column<string>(type: "text", nullable: true),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    module_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_case", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "request",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    resource = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    action_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    url = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    query_parameters = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: true),
                    body = table.Column<string>(type: "character varying(4001)", maxLength: 4001, nullable: true),
                    maker_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    checker_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    second_checker_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    maker_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    checker_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    second_checker_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    maker_full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    checker_full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    second_checker_full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reason = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: true),
                    operation_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    maker_description = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: true),
                    first_approver_description = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: true),
                    second_approver_description = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: true),
                    first_approve_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    second_approve_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reject_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    exception_message = table.Column<string>(type: "character varying(4001)", maxLength: 4001, nullable: true),
                    exception_details = table.Column<string>(type: "character varying(4001)", maxLength: 4001, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_request", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "maker_checker",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    maker_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    checker_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    second_checker_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_maker_checker", x => x.id);
                    table.ForeignKey(
                        name: "fk_maker_checker_case_case_id",
                        column: x => x.case_id,
                        principalSchema: "core",
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_case_create_date",
                schema: "core",
                table: "case",
                column: "create_date");

            migrationBuilder.CreateIndex(
                name: "ix_case_record_status",
                schema: "core",
                table: "case",
                column: "record_status");

            migrationBuilder.CreateIndex(
                name: "ix_maker_checker_case_id",
                schema: "core",
                table: "maker_checker",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "ix_maker_checker_record_status",
                schema: "core",
                table: "maker_checker",
                column: "record_status");

            migrationBuilder.CreateIndex(
                name: "ix_request_operation_type",
                schema: "core",
                table: "request",
                column: "operation_type");

            migrationBuilder.CreateIndex(
                name: "ix_request_reference_id",
                schema: "core",
                table: "request",
                column: "reference_id");

            migrationBuilder.CreateIndex(
                name: "ix_request_status",
                schema: "core",
                table: "request",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_request_update_date",
                schema: "core",
                table: "request",
                column: "update_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maker_checker",
                schema: "core");

            migrationBuilder.DropTable(
                name: "request",
                schema: "core");

            migrationBuilder.DropTable(
                name: "case",
                schema: "core");
        }
    }
}
