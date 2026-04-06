using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class MainMerchantPostingMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "amount_without_parent_merchant_commission",
                schema: "posting",
                table: "transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "parent_merchant_commission_amount",
                schema: "posting",
                table: "transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "parent_merchant_commission_rate",
                schema: "posting",
                table: "transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "parent_merchant_id",
                schema: "posting",
                table: "transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "related_posting_balance_id",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "sub_merchant_id",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "sub_merchant_name",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sub_merchant_number",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "sub_merchant_deduction_id",
                schema: "merchant",
                table: "merchant_deduction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "parent_merchant_id",
                schema: "posting",
                table: "bank_balance",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "total_parent_merchant_commission_amount",
                schema: "posting",
                table: "bank_balance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "parent_merchant_id",
                schema: "posting",
                table: "balance",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "total_parent_merchant_commission_amount",
                schema: "posting",
                table: "balance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "amount_without_parent_merchant_commission",
                schema: "posting",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "parent_merchant_commission_amount",
                schema: "posting",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "parent_merchant_commission_rate",
                schema: "posting",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "parent_merchant_id",
                schema: "posting",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "related_posting_balance_id",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "sub_merchant_id",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "sub_merchant_name",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "sub_merchant_number",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "sub_merchant_deduction_id",
                schema: "merchant",
                table: "merchant_deduction");

            migrationBuilder.DropColumn(
                name: "parent_merchant_id",
                schema: "posting",
                table: "bank_balance");

            migrationBuilder.DropColumn(
                name: "total_parent_merchant_commission_amount",
                schema: "posting",
                table: "bank_balance");

            migrationBuilder.DropColumn(
                name: "parent_merchant_id",
                schema: "posting",
                table: "balance");

            migrationBuilder.DropColumn(
                name: "total_parent_merchant_commission_amount",
                schema: "posting",
                table: "balance");
        }
    }
}
