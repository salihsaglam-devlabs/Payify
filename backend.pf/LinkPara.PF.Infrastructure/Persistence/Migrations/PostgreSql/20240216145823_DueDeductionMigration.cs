using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class DueDeductionMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "remaining_due_amount",
                schema: "merchant",
                table: "merchant_due");

            migrationBuilder.DropColumn(
                name: "bank_code",
                schema: "merchant",
                table: "merchant_deduction");

            migrationBuilder.RenameColumn(
                name: "total_due_amount",
                schema: "merchant",
                table: "merchant_due",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "due_status",
                schema: "merchant",
                table: "merchant_due",
                newName: "occurence_interval");

            migrationBuilder.AddColumn<int>(
                name: "currency",
                schema: "merchant",
                table: "merchant_due",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_execution_date",
                schema: "merchant",
                table: "merchant_due",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "total_execution_count",
                schema: "merchant",
                table: "merchant_due",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "merchant_due_id",
                schema: "merchant",
                table: "merchant_deduction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "deduction_transaction",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    posting_balance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_deduction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    deduction_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_deduction_transaction", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deduction_transaction",
                schema: "merchant");

            migrationBuilder.DropColumn(
                name: "currency",
                schema: "merchant",
                table: "merchant_due");

            migrationBuilder.DropColumn(
                name: "last_execution_date",
                schema: "merchant",
                table: "merchant_due");

            migrationBuilder.DropColumn(
                name: "total_execution_count",
                schema: "merchant",
                table: "merchant_due");

            migrationBuilder.DropColumn(
                name: "merchant_due_id",
                schema: "merchant",
                table: "merchant_deduction");

            migrationBuilder.RenameColumn(
                name: "occurence_interval",
                schema: "merchant",
                table: "merchant_due",
                newName: "due_status");

            migrationBuilder.RenameColumn(
                name: "amount",
                schema: "merchant",
                table: "merchant_due",
                newName: "total_due_amount");

            migrationBuilder.AddColumn<decimal>(
                name: "remaining_due_amount",
                schema: "merchant",
                table: "merchant_due",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "bank_code",
                schema: "merchant",
                table: "merchant_deduction",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
