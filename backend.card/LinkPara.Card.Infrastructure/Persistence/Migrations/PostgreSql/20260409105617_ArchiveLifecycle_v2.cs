using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Card.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class ArchiveLifecycle_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "archive_transition_run_id",
                schema: "archive",
                table: "ingestion_file_line",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "archive_transitioned_at",
                schema: "archive",
                table: "ingestion_file_line",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "archive_children_transitioned_at",
                schema: "archive",
                table: "ingestion_file",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "archive_cleanup_completed_at",
                schema: "archive",
                table: "ingestion_file",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "archive_cleanup_eligible_at",
                schema: "archive",
                table: "ingestion_file",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "archive_record_run_id",
                schema: "archive",
                table: "ingestion_file",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "archive_record_written_at",
                schema: "archive",
                table: "ingestion_file",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "archive_transition_run_id",
                schema: "ingestion",
                table: "file_line",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "archive_transitioned_at",
                schema: "ingestion",
                table: "file_line",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "archive_children_transitioned_at",
                schema: "ingestion",
                table: "file",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "archive_cleanup_completed_at",
                schema: "ingestion",
                table: "file",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "archive_cleanup_eligible_at",
                schema: "ingestion",
                table: "file",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "archive_record_run_id",
                schema: "ingestion",
                table: "file",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "archive_record_written_at",
                schema: "ingestion",
                table: "file",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "archive_transition_run_id",
                schema: "archive",
                table: "ingestion_file_line");

            migrationBuilder.DropColumn(
                name: "archive_transitioned_at",
                schema: "archive",
                table: "ingestion_file_line");

            migrationBuilder.DropColumn(
                name: "archive_children_transitioned_at",
                schema: "archive",
                table: "ingestion_file");

            migrationBuilder.DropColumn(
                name: "archive_cleanup_completed_at",
                schema: "archive",
                table: "ingestion_file");

            migrationBuilder.DropColumn(
                name: "archive_cleanup_eligible_at",
                schema: "archive",
                table: "ingestion_file");

            migrationBuilder.DropColumn(
                name: "archive_record_run_id",
                schema: "archive",
                table: "ingestion_file");

            migrationBuilder.DropColumn(
                name: "archive_record_written_at",
                schema: "archive",
                table: "ingestion_file");

            migrationBuilder.DropColumn(
                name: "archive_transition_run_id",
                schema: "ingestion",
                table: "file_line");

            migrationBuilder.DropColumn(
                name: "archive_transitioned_at",
                schema: "ingestion",
                table: "file_line");

            migrationBuilder.DropColumn(
                name: "archive_children_transitioned_at",
                schema: "ingestion",
                table: "file");

            migrationBuilder.DropColumn(
                name: "archive_cleanup_completed_at",
                schema: "ingestion",
                table: "file");

            migrationBuilder.DropColumn(
                name: "archive_cleanup_eligible_at",
                schema: "ingestion",
                table: "file");

            migrationBuilder.DropColumn(
                name: "archive_record_run_id",
                schema: "ingestion",
                table: "file");

            migrationBuilder.DropColumn(
                name: "archive_record_written_at",
                schema: "ingestion",
                table: "file");
        }
    }
}
