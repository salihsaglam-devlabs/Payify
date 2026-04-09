using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Card.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class IngestionAndReconciliation_v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reconciliation");

            migrationBuilder.EnsureSchema(
                name: "archive");

            migrationBuilder.EnsureSchema(
                name: "ingestion");

            migrationBuilder.CreateTable(
                name: "archive_batch",
                schema: "archive",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    requested_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    filter_json = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    processed_count = table.Column<int>(type: "integer", nullable: false),
                    archived_count = table.Column<int>(type: "integer", nullable: false),
                    skipped_count = table.Column<int>(type: "integer", nullable: false),
                    failed_count = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_archive_batch", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "archive_batch_item",
                schema: "archive",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ingestion_file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    archive_run_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    failure_reasons_json = table.Column<string>(type: "text", nullable: true),
                    processed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_archive_batch_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "file",
                schema: "ingestion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    file_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    file_path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    source_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    content_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    expected_line_count = table.Column<long>(type: "bigint", nullable: false),
                    processed_line_count = table.Column<long>(type: "bigint", nullable: false),
                    successful_line_count = table.Column<long>(type: "bigint", nullable: false),
                    failed_line_count = table.Column<long>(type: "bigint", nullable: false),
                    last_processed_line_number = table.Column<long>(type: "bigint", nullable: false),
                    last_processed_byte_offset = table.Column<long>(type: "bigint", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_file", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ingestion_file",
                schema: "archive",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    archived_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    archived_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    archive_run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    file_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    file_path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    source_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    content_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    expected_line_count = table.Column<long>(type: "bigint", nullable: false),
                    processed_line_count = table.Column<long>(type: "bigint", nullable: false),
                    successful_line_count = table.Column<long>(type: "bigint", nullable: false),
                    failed_line_count = table.Column<long>(type: "bigint", nullable: false),
                    last_processed_line_number = table.Column<long>(type: "bigint", nullable: false),
                    last_processed_byte_offset = table.Column<long>(type: "bigint", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ingestion_file", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ingestion_file_line",
                schema: "archive",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    archived_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    archived_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    archive_run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_number = table.Column<long>(type: "bigint", nullable: false),
                    byte_offset = table.Column<long>(type: "bigint", nullable: false),
                    byte_length = table.Column<int>(type: "integer", nullable: false),
                    line_type = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    raw_content = table.Column<string>(type: "text", nullable: true),
                    parsed_content = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    correlation_key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    correlation_value = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    duplicate_detection_key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    duplicate_status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    duplicate_group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reconciliation_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ingestion_file_line", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reconciliation_alert",
                schema: "archive",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    archived_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    archived_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    archive_run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evaluation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    alert_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    alert_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reconciliation_alert", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reconciliation_evaluation",
                schema: "archive",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    archived_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    archived_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    archive_run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    operation_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reconciliation_evaluation", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reconciliation_operation",
                schema: "archive",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    archived_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    archived_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    archive_run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evaluation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence_number = table.Column<int>(type: "integer", nullable: false),
                    parent_sequence_number = table.Column<int>(type: "integer", nullable: true),
                    code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    payload = table.Column<string>(type: "text", nullable: true),
                    is_manual = table.Column<bool>(type: "boolean", nullable: false),
                    branch = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    lease_owner = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    lease_expires_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    max_retry_count = table.Column<int>(type: "integer", nullable: false),
                    next_attempt_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    last_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reconciliation_operation", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reconciliation_operation_execution",
                schema: "archive",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    archived_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    archived_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    archive_run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evaluation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_number = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    finished_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    request_payload = table.Column<string>(type: "text", nullable: true),
                    response_payload = table.Column<string>(type: "text", nullable: true),
                    result_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    result_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    error_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    error_message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reconciliation_operation_execution", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reconciliation_review",
                schema: "archive",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    archived_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    archived_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    archive_run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evaluation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    decision = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    decision_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    expiration_action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    expiration_flow_action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reconciliation_review", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "file_line",
                schema: "ingestion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_number = table.Column<long>(type: "bigint", nullable: false),
                    byte_offset = table.Column<long>(type: "bigint", nullable: false),
                    byte_length = table.Column<int>(type: "integer", nullable: false),
                    line_type = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    raw_content = table.Column<string>(type: "text", nullable: true),
                    parsed_content = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    correlation_key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    correlation_value = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    duplicate_detection_key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    duplicate_status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    duplicate_group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reconciliation_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_file_line", x => x.id);
                    table.ForeignKey(
                        name: "fk_file_line_file_file_id",
                        column: x => x.file_id,
                        principalSchema: "ingestion",
                        principalTable: "file",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "evaluation",
                schema: "reconciliation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    operation_count = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_evaluation", x => x.id);
                    table.ForeignKey(
                        name: "fk_evaluation_file_line_file_line_id",
                        column: x => x.file_line_id,
                        principalSchema: "ingestion",
                        principalTable: "file_line",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "operation",
                schema: "reconciliation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evaluation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence_number = table.Column<int>(type: "integer", nullable: false),
                    parent_sequence_number = table.Column<int>(type: "integer", nullable: true),
                    code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    payload = table.Column<string>(type: "text", nullable: true),
                    is_manual = table.Column<bool>(type: "boolean", nullable: false),
                    branch = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    lease_owner = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    lease_expires_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    max_retry_count = table.Column<int>(type: "integer", nullable: false),
                    next_attempt_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    last_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operation", x => x.id);
                    table.ForeignKey(
                        name: "fk_operation_evaluation_evaluation_id",
                        column: x => x.evaluation_id,
                        principalSchema: "reconciliation",
                        principalTable: "evaluation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_operation_file_line_file_line_id",
                        column: x => x.file_line_id,
                        principalSchema: "ingestion",
                        principalTable: "file_line",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "alert",
                schema: "reconciliation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evaluation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    alert_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    alert_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_alert", x => x.id);
                    table.ForeignKey(
                        name: "fk_alert_evaluation_evaluation_id",
                        column: x => x.evaluation_id,
                        principalSchema: "reconciliation",
                        principalTable: "evaluation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_alert_file_line_file_line_id",
                        column: x => x.file_line_id,
                        principalSchema: "ingestion",
                        principalTable: "file_line",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_alert_operation_operation_id",
                        column: x => x.operation_id,
                        principalSchema: "reconciliation",
                        principalTable: "operation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "operation_execution",
                schema: "reconciliation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evaluation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_number = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    finished_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    request_payload = table.Column<string>(type: "text", nullable: true),
                    response_payload = table.Column<string>(type: "text", nullable: true),
                    result_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    result_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    error_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    error_message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operation_execution", x => x.id);
                    table.ForeignKey(
                        name: "fk_operation_execution_evaluation_evaluation_id",
                        column: x => x.evaluation_id,
                        principalSchema: "reconciliation",
                        principalTable: "evaluation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_operation_execution_file_line_file_line_id",
                        column: x => x.file_line_id,
                        principalSchema: "ingestion",
                        principalTable: "file_line",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_operation_execution_operation_operation_id",
                        column: x => x.operation_id,
                        principalSchema: "reconciliation",
                        principalTable: "operation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "review",
                schema: "reconciliation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evaluation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    decision = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    decision_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    expiration_action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    expiration_flow_action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_review", x => x.id);
                    table.ForeignKey(
                        name: "fk_review_evaluation_evaluation_id",
                        column: x => x.evaluation_id,
                        principalSchema: "reconciliation",
                        principalTable: "evaluation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_review_file_line_file_line_id",
                        column: x => x.file_line_id,
                        principalSchema: "ingestion",
                        principalTable: "file_line",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_review_operation_operation_id",
                        column: x => x.operation_id,
                        principalSchema: "reconciliation",
                        principalTable: "operation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_alert_alert_status",
                schema: "reconciliation",
                table: "alert",
                column: "alert_status");

            migrationBuilder.CreateIndex(
                name: "ix_alert_alert_status_create_date",
                schema: "reconciliation",
                table: "alert",
                columns: new[] { "alert_status", "create_date" });

            migrationBuilder.CreateIndex(
                name: "ix_alert_evaluation_id",
                schema: "reconciliation",
                table: "alert",
                column: "evaluation_id");

            migrationBuilder.CreateIndex(
                name: "ix_alert_file_line_id",
                schema: "reconciliation",
                table: "alert",
                column: "file_line_id");

            migrationBuilder.CreateIndex(
                name: "ix_alert_group_id",
                schema: "reconciliation",
                table: "alert",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_alert_operation_id",
                schema: "reconciliation",
                table: "alert",
                column: "operation_id");

            migrationBuilder.CreateIndex(
                name: "ix_alert_severity",
                schema: "reconciliation",
                table: "alert",
                column: "severity");

            migrationBuilder.CreateIndex(
                name: "ix_archive_batch_item_batch_id",
                schema: "archive",
                table: "archive_batch_item",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "ix_archive_batch_item_ingestion_file_id",
                schema: "archive",
                table: "archive_batch_item",
                column: "ingestion_file_id");

            migrationBuilder.CreateIndex(
                name: "ix_evaluation_file_line_id",
                schema: "reconciliation",
                table: "evaluation",
                column: "file_line_id");

            migrationBuilder.CreateIndex(
                name: "ix_evaluation_group_id",
                schema: "reconciliation",
                table: "evaluation",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_file_file_key_file_name_source_type_file_type",
                schema: "ingestion",
                table: "file",
                columns: new[] { "file_key", "file_name", "source_type", "file_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_file_status",
                schema: "ingestion",
                table: "file",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_file_line_correlation_key",
                schema: "ingestion",
                table: "file_line",
                column: "correlation_key");

            migrationBuilder.CreateIndex(
                name: "ix_file_line_correlation_key_correlation_value",
                schema: "ingestion",
                table: "file_line",
                columns: new[] { "correlation_key", "correlation_value" });

            migrationBuilder.CreateIndex(
                name: "ix_file_line_duplicate_group_id",
                schema: "ingestion",
                table: "file_line",
                column: "duplicate_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_file_line_file_id_byte_offset",
                schema: "ingestion",
                table: "file_line",
                columns: new[] { "file_id", "byte_offset" });

            migrationBuilder.CreateIndex(
                name: "ix_file_line_file_id_duplicate_detection_key",
                schema: "ingestion",
                table: "file_line",
                columns: new[] { "file_id", "duplicate_detection_key" });

            migrationBuilder.CreateIndex(
                name: "ix_file_line_file_id_duplicate_status",
                schema: "ingestion",
                table: "file_line",
                columns: new[] { "file_id", "duplicate_status" });

            migrationBuilder.CreateIndex(
                name: "ix_file_line_file_id_line_number",
                schema: "ingestion",
                table: "file_line",
                columns: new[] { "file_id", "line_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_file_line_file_id_line_type_reconciliation_status_update_da",
                schema: "ingestion",
                table: "file_line",
                columns: new[] { "file_id", "line_type", "reconciliation_status", "update_date" });

            migrationBuilder.CreateIndex(
                name: "ix_file_line_file_id_line_type_status",
                schema: "ingestion",
                table: "file_line",
                columns: new[] { "file_id", "line_type", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_file_line_file_id_status",
                schema: "ingestion",
                table: "file_line",
                columns: new[] { "file_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_file_line_reconciliation_status",
                schema: "ingestion",
                table: "file_line",
                column: "reconciliation_status");

            migrationBuilder.CreateIndex(
                name: "ix_operation_evaluation_id",
                schema: "reconciliation",
                table: "operation",
                column: "evaluation_id");

            migrationBuilder.CreateIndex(
                name: "ix_operation_evaluation_id_sequence_number",
                schema: "reconciliation",
                table: "operation",
                columns: new[] { "evaluation_id", "sequence_number" });

            migrationBuilder.CreateIndex(
                name: "ix_operation_file_line_id",
                schema: "reconciliation",
                table: "operation",
                column: "file_line_id");

            migrationBuilder.CreateIndex(
                name: "ix_operation_group_id_evaluation_id_sequence_index",
                schema: "reconciliation",
                table: "operation",
                columns: new[] { "group_id", "evaluation_id", "sequence_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_operation_idempotency_key",
                schema: "reconciliation",
                table: "operation",
                column: "idempotency_key");

            migrationBuilder.CreateIndex(
                name: "ix_operation_lease_owner",
                schema: "reconciliation",
                table: "operation",
                column: "lease_owner");

            migrationBuilder.CreateIndex(
                name: "ix_operation_status",
                schema: "reconciliation",
                table: "operation",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_operation_status_next_attempt_at_lease_expires_at",
                schema: "reconciliation",
                table: "operation",
                columns: new[] { "status", "next_attempt_at", "lease_expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_operation_execution_evaluation_id",
                schema: "reconciliation",
                table: "operation_execution",
                column: "evaluation_id");

            migrationBuilder.CreateIndex(
                name: "ix_operation_execution_evaluation_id_operation_id",
                schema: "reconciliation",
                table: "operation_execution",
                columns: new[] { "evaluation_id", "operation_id" });

            migrationBuilder.CreateIndex(
                name: "ix_operation_execution_file_line_id",
                schema: "reconciliation",
                table: "operation_execution",
                column: "file_line_id");

            migrationBuilder.CreateIndex(
                name: "ix_operation_execution_group_id",
                schema: "reconciliation",
                table: "operation_execution",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_operation_execution_operation_id",
                schema: "reconciliation",
                table: "operation_execution",
                column: "operation_id");

            migrationBuilder.CreateIndex(
                name: "ix_operation_execution_operation_id_attempt_number",
                schema: "reconciliation",
                table: "operation_execution",
                columns: new[] { "operation_id", "attempt_number" });

            migrationBuilder.CreateIndex(
                name: "ix_review_decision_create_date",
                schema: "reconciliation",
                table: "review",
                columns: new[] { "decision", "create_date" });

            migrationBuilder.CreateIndex(
                name: "ix_review_evaluation_id",
                schema: "reconciliation",
                table: "review",
                column: "evaluation_id");

            migrationBuilder.CreateIndex(
                name: "ix_review_file_line_id",
                schema: "reconciliation",
                table: "review",
                column: "file_line_id");

            migrationBuilder.CreateIndex(
                name: "ix_review_group_id",
                schema: "reconciliation",
                table: "review",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_review_operation_id",
                schema: "reconciliation",
                table: "review",
                column: "operation_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alert",
                schema: "reconciliation");

            migrationBuilder.DropTable(
                name: "archive_batch",
                schema: "archive");

            migrationBuilder.DropTable(
                name: "archive_batch_item",
                schema: "archive");

            migrationBuilder.DropTable(
                name: "ingestion_file",
                schema: "archive");

            migrationBuilder.DropTable(
                name: "ingestion_file_line",
                schema: "archive");

            migrationBuilder.DropTable(
                name: "operation_execution",
                schema: "reconciliation");

            migrationBuilder.DropTable(
                name: "reconciliation_alert",
                schema: "archive");

            migrationBuilder.DropTable(
                name: "reconciliation_evaluation",
                schema: "archive");

            migrationBuilder.DropTable(
                name: "reconciliation_operation",
                schema: "archive");

            migrationBuilder.DropTable(
                name: "reconciliation_operation_execution",
                schema: "archive");

            migrationBuilder.DropTable(
                name: "reconciliation_review",
                schema: "archive");

            migrationBuilder.DropTable(
                name: "review",
                schema: "reconciliation");

            migrationBuilder.DropTable(
                name: "operation",
                schema: "reconciliation");

            migrationBuilder.DropTable(
                name: "evaluation",
                schema: "reconciliation");

            migrationBuilder.DropTable(
                name: "file_line",
                schema: "ingestion");

            migrationBuilder.DropTable(
                name: "file",
                schema: "ingestion");
        }
    }
}
