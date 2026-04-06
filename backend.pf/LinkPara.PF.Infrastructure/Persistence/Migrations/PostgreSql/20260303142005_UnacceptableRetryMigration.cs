using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class UnacceptableRetryMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "issuer_bank_id",
                schema: "physical",
                table: "unacceptable_transaction");

            migrationBuilder.AddColumn<string>(
                name: "end_of_day_status",
                schema: "physical",
                table: "unacceptable_transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<Guid>(
                name: "merchant_transaction_id",
                schema: "physical",
                table: "unacceptable_transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "error_code",
                schema: "physical",
                table: "reconciliation_transaction",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "error_message",
                schema: "physical",
                table: "reconciliation_transaction",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "end_of_day_status",
                schema: "physical",
                table: "unacceptable_transaction");

            migrationBuilder.DropColumn(
                name: "merchant_transaction_id",
                schema: "physical",
                table: "unacceptable_transaction");

            migrationBuilder.DropColumn(
                name: "error_code",
                schema: "physical",
                table: "reconciliation_transaction");

            migrationBuilder.DropColumn(
                name: "error_message",
                schema: "physical",
                table: "reconciliation_transaction");

            migrationBuilder.AddColumn<string>(
                name: "issuer_bank_id",
                schema: "physical",
                table: "unacceptable_transaction",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
