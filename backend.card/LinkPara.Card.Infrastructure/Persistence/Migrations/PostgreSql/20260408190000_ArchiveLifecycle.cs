using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Card.Infrastructure.Persistence.Migrations.PostgreSql
{
    public partial class ArchiveLifecycle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "archive");

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS archive.ingestion_file
                (
                    id uuid NOT NULL PRIMARY KEY,
                    file_key character varying(128) NOT NULL,
                    file_name character varying(256) NOT NULL,
                    file_path character varying(1024) NOT NULL,
                    source_type character varying(50) NOT NULL,
                    file_type character varying(50) NOT NULL,
                    content_type character varying(50) NOT NULL,
                    status character varying(50) NOT NULL,
                    message character varying(4000),
                    expected_line_count bigint NOT NULL,
                    processed_line_count bigint NOT NULL,
                    successful_line_count bigint NOT NULL,
                    failed_line_count bigint NOT NULL,
                    last_processed_line_number bigint NOT NULL,
                    last_processed_byte_offset bigint NOT NULL,
                    is_archived boolean NOT NULL,
                    create_date timestamp without time zone NOT NULL,
                    update_date timestamp without time zone NULL,
                    created_by character varying(50) NOT NULL,
                    last_modified_by character varying(50) NULL,
                    record_status character varying(50) NOT NULL,
                    archive_run_id uuid NOT NULL,
                    archived_at timestamp without time zone NOT NULL,
                    archived_by character varying(100) NOT NULL
                );
                CREATE INDEX IF NOT EXISTS ix_archive_ingestion_file_archive_run_id ON archive.ingestion_file (archive_run_id);
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS archive.ingestion_file_line
                (
                    id uuid NOT NULL PRIMARY KEY,
                    file_id uuid NOT NULL,
                    line_number bigint NOT NULL,
                    byte_offset bigint NOT NULL,
                    byte_length integer NOT NULL,
                    line_type character varying(8),
                    raw_content text NULL,
                    parsed_content text NULL,
                    status character varying(50) NOT NULL,
                    message character varying(4000) NULL,
                    retry_count integer NOT NULL,
                    correlation_key character varying(256) NULL,
                    correlation_value character varying(1024) NULL,
                    duplicate_detection_key character varying(256) NULL,
                    duplicate_status character varying(64) NULL,
                    duplicate_group_id uuid NULL,
                    reconciliation_status character varying(32) NULL,
                    create_date timestamp without time zone NOT NULL,
                    update_date timestamp without time zone NULL,
                    created_by character varying(50) NOT NULL,
                    last_modified_by character varying(50) NULL,
                    record_status character varying(50) NOT NULL,
                    archive_run_id uuid NOT NULL,
                    archived_at timestamp without time zone NOT NULL,
                    archived_by character varying(100) NOT NULL,
                    CONSTRAINT fk_archive_ingestion_file_line_file
                        FOREIGN KEY (file_id) REFERENCES archive.ingestion_file (id)
                );
                CREATE INDEX IF NOT EXISTS ix_archive_ingestion_file_line_file_id ON archive.ingestion_file_line (file_id);
                CREATE INDEX IF NOT EXISTS ix_archive_ingestion_file_line_archive_run_id ON archive.ingestion_file_line (archive_run_id);
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS archive.reconciliation_evaluation
                (
                    id uuid NOT NULL PRIMARY KEY,
                    file_line_id uuid NOT NULL,
                    group_id uuid NOT NULL,
                    status character varying(50) NOT NULL,
                    message character varying(1000) NULL,
                    operation_count integer NOT NULL,
                    create_date timestamp without time zone NOT NULL,
                    update_date timestamp without time zone NULL,
                    created_by character varying(50) NOT NULL,
                    last_modified_by character varying(50) NULL,
                    record_status character varying(50) NOT NULL,
                    archive_run_id uuid NOT NULL,
                    archived_at timestamp without time zone NOT NULL,
                    archived_by character varying(100) NOT NULL,
                    CONSTRAINT fk_archive_reconciliation_evaluation_file_line
                        FOREIGN KEY (file_line_id) REFERENCES archive.ingestion_file_line (id)
                );
                CREATE INDEX IF NOT EXISTS ix_archive_reconciliation_evaluation_file_line_id ON archive.reconciliation_evaluation (file_line_id);
                CREATE INDEX IF NOT EXISTS ix_archive_reconciliation_evaluation_archive_run_id ON archive.reconciliation_evaluation (archive_run_id);
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS archive.reconciliation_operation
                (
                    id uuid NOT NULL PRIMARY KEY,
                    file_line_id uuid NOT NULL,
                    evaluation_id uuid NOT NULL,
                    group_id uuid NOT NULL,
                    sequence_number integer NOT NULL,
                    parent_sequence_number integer NULL,
                    code character varying(200) NULL,
                    note character varying(2000) NULL,
                    payload text NULL,
                    is_manual boolean NOT NULL,
                    branch character varying(50) NULL,
                    status character varying(50) NOT NULL,
                    lease_owner character varying(200) NULL,
                    lease_expires_at timestamp without time zone NULL,
                    retry_count integer NOT NULL,
                    max_retry_count integer NOT NULL,
                    next_attempt_at timestamp without time zone NULL,
                    idempotency_key character varying(200) NULL,
                    last_error character varying(2000) NULL,
                    create_date timestamp without time zone NOT NULL,
                    update_date timestamp without time zone NULL,
                    created_by character varying(50) NOT NULL,
                    last_modified_by character varying(50) NULL,
                    record_status character varying(50) NOT NULL,
                    archive_run_id uuid NOT NULL,
                    archived_at timestamp without time zone NOT NULL,
                    archived_by character varying(100) NOT NULL,
                    CONSTRAINT fk_archive_reconciliation_operation_file_line
                        FOREIGN KEY (file_line_id) REFERENCES archive.ingestion_file_line (id),
                    CONSTRAINT fk_archive_reconciliation_operation_evaluation
                        FOREIGN KEY (evaluation_id) REFERENCES archive.reconciliation_evaluation (id)
                );
                CREATE INDEX IF NOT EXISTS ix_archive_reconciliation_operation_file_line_id ON archive.reconciliation_operation (file_line_id);
                CREATE INDEX IF NOT EXISTS ix_archive_reconciliation_operation_evaluation_id ON archive.reconciliation_operation (evaluation_id);
                CREATE INDEX IF NOT EXISTS ix_archive_reconciliation_operation_archive_run_id ON archive.reconciliation_operation (archive_run_id);
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS archive.reconciliation_review
                (
                    id uuid NOT NULL PRIMARY KEY,
                    operation_id uuid NOT NULL,
                    reviewer_id uuid NULL,
                    decision character varying(50) NOT NULL,
                    comment character varying(2000) NULL,
                    decision_at timestamp without time zone NULL,
                    expires_at timestamp without time zone NULL,
                    expiration_action character varying(32) NOT NULL,
                    expiration_flow_action character varying(32) NOT NULL,
                    file_line_id uuid NOT NULL,
                    group_id uuid NOT NULL,
                    evaluation_id uuid NOT NULL,
                    create_date timestamp without time zone NOT NULL,
                    update_date timestamp without time zone NULL,
                    created_by character varying(50) NOT NULL,
                    last_modified_by character varying(50) NULL,
                    record_status character varying(50) NOT NULL,
                    archive_run_id uuid NOT NULL,
                    archived_at timestamp without time zone NOT NULL,
                    archived_by character varying(100) NOT NULL,
                    CONSTRAINT fk_archive_reconciliation_review_file_line
                        FOREIGN KEY (file_line_id) REFERENCES archive.ingestion_file_line (id),
                    CONSTRAINT fk_archive_reconciliation_review_evaluation
                        FOREIGN KEY (evaluation_id) REFERENCES archive.reconciliation_evaluation (id),
                    CONSTRAINT fk_archive_reconciliation_review_operation
                        FOREIGN KEY (operation_id) REFERENCES archive.reconciliation_operation (id)
                );
                CREATE INDEX IF NOT EXISTS ix_archive_reconciliation_review_file_line_id ON archive.reconciliation_review (file_line_id);
                CREATE INDEX IF NOT EXISTS ix_archive_reconciliation_review_evaluation_id ON archive.reconciliation_review (evaluation_id);
                CREATE INDEX IF NOT EXISTS ix_archive_reconciliation_review_operation_id ON archive.reconciliation_review (operation_id);
                CREATE INDEX IF NOT EXISTS ix_archive_reconciliation_review_archive_run_id ON archive.reconciliation_review (archive_run_id);
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS archive.reconciliation_operation_execution
                (
                    id uuid NOT NULL PRIMARY KEY,
                    file_line_id uuid NOT NULL,
                    group_id uuid NOT NULL,
                    evaluation_id uuid NOT NULL,
                    operation_id uuid NOT NULL,
                    attempt_number integer NOT NULL,
                    started_at timestamp without time zone NOT NULL,
                    finished_at timestamp without time zone NULL,
                    status character varying(50) NOT NULL,
                    request_payload text NULL,
                    response_payload text NULL,
                    result_code character varying(100) NULL,
                    result_message character varying(2000) NULL,
                    error_code character varying(100) NULL,
                    error_message character varying(4000) NULL,
                    create_date timestamp without time zone NOT NULL,
                    update_date timestamp without time zone NULL,
                    created_by character varying(50) NOT NULL,
                    last_modified_by character varying(50) NULL,
                    record_status character varying(50) NOT NULL,
                    archive_run_id uuid NOT NULL,
                    archived_at timestamp without time zone NOT NULL,
                    archived_by character varying(100) NOT NULL,
                    CONSTRAINT fk_archive_reconciliation_operation_execution_file_line
                        FOREIGN KEY (file_line_id) REFERENCES archive.ingestion_file_line (id),
                    CONSTRAINT fk_archive_reconciliation_operation_execution_evaluation
                        FOREIGN KEY (evaluation_id) REFERENCES archive.reconciliation_evaluation (id),
                    CONSTRAINT fk_archive_reconciliation_operation_execution_operation
                        FOREIGN KEY (operation_id) REFERENCES archive.reconciliation_operation (id)
                );
                CREATE INDEX IF NOT EXISTS ix_archive_reconciliation_operation_execution_file_line_id ON archive.reconciliation_operation_execution (file_line_id);
                CREATE INDEX IF NOT EXISTS ix_archive_reconciliation_operation_execution_evaluation_id ON archive.reconciliation_operation_execution (evaluation_id);
                CREATE INDEX IF NOT EXISTS ix_archive_reconciliation_operation_execution_operation_id ON archive.reconciliation_operation_execution (operation_id);
                CREATE INDEX IF NOT EXISTS ix_archive_reconciliation_operation_execution_archive_run_id ON archive.reconciliation_operation_execution (archive_run_id);
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS archive.reconciliation_alert
                (
                    id uuid NOT NULL PRIMARY KEY,
                    file_line_id uuid NOT NULL,
                    group_id uuid NOT NULL,
                    evaluation_id uuid NOT NULL,
                    operation_id uuid NOT NULL,
                    severity character varying(20) NULL,
                    alert_type character varying(200) NULL,
                    message character varying(2000) NULL,
                    alert_status character varying(50) NOT NULL,
                    create_date timestamp without time zone NOT NULL,
                    update_date timestamp without time zone NULL,
                    created_by character varying(50) NOT NULL,
                    last_modified_by character varying(50) NULL,
                    record_status character varying(50) NOT NULL,
                    archive_run_id uuid NOT NULL,
                    archived_at timestamp without time zone NOT NULL,
                    archived_by character varying(100) NOT NULL,
                    CONSTRAINT fk_archive_reconciliation_alert_file_line
                        FOREIGN KEY (file_line_id) REFERENCES archive.ingestion_file_line (id),
                    CONSTRAINT fk_archive_reconciliation_alert_evaluation
                        FOREIGN KEY (evaluation_id) REFERENCES archive.reconciliation_evaluation (id),
                    CONSTRAINT fk_archive_reconciliation_alert_operation
                        FOREIGN KEY (operation_id) REFERENCES archive.reconciliation_operation (id)
                );
                CREATE INDEX IF NOT EXISTS ix_archive_reconciliation_alert_file_line_id ON archive.reconciliation_alert (file_line_id);
                CREATE INDEX IF NOT EXISTS ix_archive_reconciliation_alert_evaluation_id ON archive.reconciliation_alert (evaluation_id);
                CREATE INDEX IF NOT EXISTS ix_archive_reconciliation_alert_operation_id ON archive.reconciliation_alert (operation_id);
                CREATE INDEX IF NOT EXISTS ix_archive_reconciliation_alert_archive_run_id ON archive.reconciliation_alert (archive_run_id);
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS archive.archive_batch
                (
                    id uuid NOT NULL PRIMARY KEY,
                    requested_at timestamp without time zone NOT NULL,
                    started_at timestamp without time zone NOT NULL,
                    completed_at timestamp without time zone NULL,
                    requested_by character varying(100) NOT NULL,
                    filter_json text NULL,
                    status character varying(32) NOT NULL,
                    processed_count integer NOT NULL,
                    archived_count integer NOT NULL,
                    skipped_count integer NOT NULL,
                    failed_count integer NOT NULL,
                    create_date timestamp without time zone NOT NULL,
                    update_date timestamp without time zone NULL,
                    created_by character varying(50) NOT NULL,
                    last_modified_by character varying(50) NULL,
                    record_status character varying(50) NOT NULL
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS archive.archive_batch_item
                (
                    id uuid NOT NULL PRIMARY KEY,
                    batch_id uuid NOT NULL,
                    ingestion_file_id uuid NOT NULL,
                    archive_run_id uuid NULL,
                    status character varying(32) NOT NULL,
                    message character varying(2000) NULL,
                    failure_reasons_json text NULL,
                    processed_at timestamp without time zone NOT NULL,
                    create_date timestamp without time zone NOT NULL,
                    update_date timestamp without time zone NULL,
                    created_by character varying(50) NOT NULL,
                    last_modified_by character varying(50) NULL,
                    record_status character varying(50) NOT NULL,
                    CONSTRAINT fk_archive_batch_item_batch
                        FOREIGN KEY (batch_id) REFERENCES archive.archive_batch (id)
                );
                CREATE INDEX IF NOT EXISTS ix_archive_batch_item_batch_id ON archive.archive_batch_item (batch_id);
                CREATE INDEX IF NOT EXISTS ix_archive_batch_item_file_id ON archive.archive_batch_item (ingestion_file_id);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS archive.archive_batch_item;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS archive.archive_batch;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS archive.reconciliation_alert;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS archive.reconciliation_operation_execution;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS archive.reconciliation_review;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS archive.reconciliation_operation;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS archive.reconciliation_evaluation;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS archive.ingestion_file_line;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS archive.ingestion_file;");
        }
    }
}
