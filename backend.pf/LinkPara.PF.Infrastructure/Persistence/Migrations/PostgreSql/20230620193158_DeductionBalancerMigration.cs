using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DeductionBalancerMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_balance_posting_bank_balance_posting_bank_balance_id",
                schema: "posting",
                table: "balance");

            migrationBuilder.DropColumn(
                name: "total_chargeback_amount",
                schema: "posting",
                table: "bank_balance");

            migrationBuilder.DropColumn(
                name: "total_due_amount",
                schema: "posting",
                table: "bank_balance");

            migrationBuilder.DropColumn(
                name: "total_suspicious_amount",
                schema: "posting",
                table: "bank_balance");

            migrationBuilder.AddColumn<int>(
                name: "currency",
                schema: "merchant",
                table: "merchant_deduction",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "execution_date",
                schema: "merchant",
                table: "merchant_deduction",
                type: "Date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "posting_bank_balance_id",
                schema: "posting",
                table: "balance",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "posting_balance_type",
                schema: "posting",
                table: "balance",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_merchant_deduction_merchant_transaction_id",
                schema: "merchant",
                table: "merchant_deduction",
                column: "merchant_transaction_id");

            migrationBuilder.AddForeignKey(
                name: "fk_balance_posting_bank_balance_posting_bank_balance_id",
                schema: "posting",
                table: "balance",
                column: "posting_bank_balance_id",
                principalSchema: "posting",
                principalTable: "bank_balance",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_merchant_deduction_merchant_transaction_merchant_transactio",
                schema: "merchant",
                table: "merchant_deduction",
                column: "merchant_transaction_id",
                principalSchema: "merchant",
                principalTable: "transaction",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_balance_posting_bank_balance_posting_bank_balance_id",
                schema: "posting",
                table: "balance");

            migrationBuilder.DropForeignKey(
                name: "fk_merchant_deduction_merchant_transaction_merchant_transactio",
                schema: "merchant",
                table: "merchant_deduction");

            migrationBuilder.DropIndex(
                name: "ix_merchant_deduction_merchant_transaction_id",
                schema: "merchant",
                table: "merchant_deduction");

            migrationBuilder.DropColumn(
                name: "currency",
                schema: "merchant",
                table: "merchant_deduction");

            migrationBuilder.DropColumn(
                name: "execution_date",
                schema: "merchant",
                table: "merchant_deduction");

            migrationBuilder.DropColumn(
                name: "posting_balance_type",
                schema: "posting",
                table: "balance");

            migrationBuilder.AddColumn<decimal>(
                name: "total_chargeback_amount",
                schema: "posting",
                table: "bank_balance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "total_due_amount",
                schema: "posting",
                table: "bank_balance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "total_suspicious_amount",
                schema: "posting",
                table: "bank_balance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<Guid>(
                name: "posting_bank_balance_id",
                schema: "posting",
                table: "balance",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_balance_posting_bank_balance_posting_bank_balance_id",
                schema: "posting",
                table: "balance",
                column: "posting_bank_balance_id",
                principalSchema: "posting",
                principalTable: "bank_balance",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
