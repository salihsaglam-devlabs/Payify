using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class EditCommissionRateFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_acquire_bank_bank_bank_id",
                schema: "bank",
                table: "acquire_bank");

            migrationBuilder.DropForeignKey(
                name: "fk_blockage_detail_blockage_merchant_blockage_id",
                schema: "merchant",
                table: "blockage_detail");

            migrationBuilder.DropForeignKey(
                name: "fk_cost_profile_currency_currency_id",
                schema: "core",
                table: "cost_profile");

            migrationBuilder.DropForeignKey(
                name: "fk_merchant_mcc_mcc_id",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.DropForeignKey(
                name: "fk_pricing_profile_currency_currency_id",
                schema: "core",
                table: "pricing_profile");

            migrationBuilder.AlterColumn<DateTime>(
                name: "transaction_start_date",
                schema: "posting",
                table: "transaction",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "transaction_end_date",
                schema: "posting",
                table: "transaction",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "transaction_start_date",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "transaction_end_date",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<decimal>(
                name: "service_commission",
                schema: "core",
                table: "cost_profile",
                type: "numeric(5,3)",
                precision: 5,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(4,2)",
                oldPrecision: 4,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "point_commission",
                schema: "core",
                table: "cost_profile",
                type: "numeric(5,3)",
                precision: 5,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(4,2)",
                oldPrecision: 4,
                oldScale: 2);

            migrationBuilder.AddForeignKey(
                name: "fk_acquire_bank_bank_bank_code",
                schema: "bank",
                table: "acquire_bank",
                column: "bank_code",
                principalSchema: "bank",
                principalTable: "bank",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_blockage_detail_blockage_merchant_blockage_id",
                schema: "merchant",
                table: "blockage_detail",
                column: "merchant_blockage_id",
                principalSchema: "merchant",
                principalTable: "blockage",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_cost_profile_currency_currency_code",
                schema: "core",
                table: "cost_profile",
                column: "currency_code",
                principalSchema: "core",
                principalTable: "currency",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_merchant_mcc_mcc_code",
                schema: "merchant",
                table: "merchant",
                column: "mcc_code",
                principalSchema: "merchant",
                principalTable: "mcc",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_pricing_profile_currency_currency_code",
                schema: "core",
                table: "pricing_profile",
                column: "currency_code",
                principalSchema: "core",
                principalTable: "currency",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_acquire_bank_bank_bank_code",
                schema: "bank",
                table: "acquire_bank");

            migrationBuilder.DropForeignKey(
                name: "fk_blockage_detail_blockage_merchant_blockage_id",
                schema: "merchant",
                table: "blockage_detail");

            migrationBuilder.DropForeignKey(
                name: "fk_cost_profile_currency_currency_code",
                schema: "core",
                table: "cost_profile");

            migrationBuilder.DropForeignKey(
                name: "fk_merchant_mcc_mcc_code",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.DropForeignKey(
                name: "fk_pricing_profile_currency_currency_code",
                schema: "core",
                table: "pricing_profile");

            migrationBuilder.AlterColumn<DateTime>(
                name: "transaction_start_date",
                schema: "posting",
                table: "transaction",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "transaction_end_date",
                schema: "posting",
                table: "transaction",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "transaction_start_date",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "transaction_end_date",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<decimal>(
                name: "service_commission",
                schema: "core",
                table: "cost_profile",
                type: "numeric(4,2)",
                precision: 4,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,3)",
                oldPrecision: 5,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "point_commission",
                schema: "core",
                table: "cost_profile",
                type: "numeric(4,2)",
                precision: 4,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,3)",
                oldPrecision: 5,
                oldScale: 3);

            migrationBuilder.AddForeignKey(
                name: "fk_acquire_bank_bank_bank_id",
                schema: "bank",
                table: "acquire_bank",
                column: "bank_code",
                principalSchema: "bank",
                principalTable: "bank",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_blockage_detail_blockage_merchant_blockage_id",
                schema: "merchant",
                table: "blockage_detail",
                column: "merchant_blockage_id",
                principalSchema: "merchant",
                principalTable: "blockage",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_cost_profile_currency_currency_id",
                schema: "core",
                table: "cost_profile",
                column: "currency_code",
                principalSchema: "core",
                principalTable: "currency",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_merchant_mcc_mcc_id",
                schema: "merchant",
                table: "merchant",
                column: "mcc_code",
                principalSchema: "merchant",
                principalTable: "mcc",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_pricing_profile_currency_currency_id",
                schema: "core",
                table: "pricing_profile",
                column: "currency_code",
                principalSchema: "core",
                principalTable: "currency",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
