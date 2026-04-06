using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Accounting.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class DeductionAccountingUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "bank_commission_amount",
                schema: "core",
                table: "payment",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "chargeback_amount",
                schema: "core",
                table: "payment",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "chargeback_return_amount",
                schema: "core",
                table: "payment",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "due_amount",
                schema: "core",
                table: "payment",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "return_amount",
                schema: "core",
                table: "payment",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "suspicious_amount",
                schema: "core",
                table: "payment",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "suspicious_return_amount",
                schema: "core",
                table: "payment",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "account_tag",
                schema: "core",
                table: "bank_account",
                type: "character varying(350)",
                maxLength: 350,
                nullable: false,
                defaultValue: "{{BankAccountNumber}}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bank_commission_amount",
                schema: "core",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "chargeback_amount",
                schema: "core",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "chargeback_return_amount",
                schema: "core",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "due_amount",
                schema: "core",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "return_amount",
                schema: "core",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "suspicious_amount",
                schema: "core",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "suspicious_return_amount",
                schema: "core",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "account_tag",
                schema: "core",
                table: "bank_account");
        }
    }
}
