using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class WalletPaymentRequestTableAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_account_current_level_currency_currency_id",
                schema: "limit",
                table: "account_current_level");

            migrationBuilder.DropForeignKey(
                name: "fk_pricing_profile_bank_bank_id",
                schema: "core",
                table: "pricing_profile");

            migrationBuilder.DropForeignKey(
                name: "fk_pricing_profile_currency_currency_id",
                schema: "core",
                table: "pricing_profile");

            migrationBuilder.DropForeignKey(
                name: "fk_tier_level_currency_currency_id",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropForeignKey(
                name: "fk_transaction_currency_currency_id",
                schema: "core",
                table: "transaction");

            migrationBuilder.DropForeignKey(
                name: "fk_transfer_order_currency_currency_id",
                schema: "core",
                table: "transfer_order");

            migrationBuilder.DropForeignKey(
                name: "fk_wallet_currency_currency_id",
                schema: "core",
                table: "wallet");

            migrationBuilder.DropTable(
                name: "passwordless_payment_request",
                schema: "core");

            migrationBuilder.CreateTable(
                name: "wallet_payment_request",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_reference_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    internal_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    sender_wallet_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sender_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    receiver_wallet_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    receiver_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_logged_in = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallet_payment_request", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_wallet_payment_request_payment_reference_id",
                schema: "core",
                table: "wallet_payment_request",
                column: "payment_reference_id");

            migrationBuilder.AddForeignKey(
                name: "fk_account_current_level_currency_currency_code",
                schema: "limit",
                table: "account_current_level",
                column: "currency_code",
                principalSchema: "core",
                principalTable: "currency",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_pricing_profile_bank_bank_code",
                schema: "core",
                table: "pricing_profile",
                column: "bank_code",
                principalSchema: "core",
                principalTable: "bank",
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

            migrationBuilder.AddForeignKey(
                name: "fk_tier_level_currency_currency_code",
                schema: "limit",
                table: "tier_level",
                column: "currency_code",
                principalSchema: "core",
                principalTable: "currency",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_transaction_currency_currency_code",
                schema: "core",
                table: "transaction",
                column: "currency_code",
                principalSchema: "core",
                principalTable: "currency",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_transfer_order_currency_currency_code",
                schema: "core",
                table: "transfer_order",
                column: "currency_code",
                principalSchema: "core",
                principalTable: "currency",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_wallet_currency_currency_code",
                schema: "core",
                table: "wallet",
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
                name: "fk_account_current_level_currency_currency_code",
                schema: "limit",
                table: "account_current_level");

            migrationBuilder.DropForeignKey(
                name: "fk_pricing_profile_bank_bank_code",
                schema: "core",
                table: "pricing_profile");

            migrationBuilder.DropForeignKey(
                name: "fk_pricing_profile_currency_currency_code",
                schema: "core",
                table: "pricing_profile");

            migrationBuilder.DropForeignKey(
                name: "fk_tier_level_currency_currency_code",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropForeignKey(
                name: "fk_transaction_currency_currency_code",
                schema: "core",
                table: "transaction");

            migrationBuilder.DropForeignKey(
                name: "fk_transfer_order_currency_currency_code",
                schema: "core",
                table: "transfer_order");

            migrationBuilder.DropForeignKey(
                name: "fk_wallet_currency_currency_code",
                schema: "core",
                table: "wallet");

            migrationBuilder.DropTable(
                name: "wallet_payment_request",
                schema: "core");

            migrationBuilder.CreateTable(
                name: "passwordless_payment_request",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    internal_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    payment_reference_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    receiver_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    receiver_wallet_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sender_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    sender_wallet_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_passwordless_payment_request", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_passwordless_payment_request_payment_reference_id",
                schema: "core",
                table: "passwordless_payment_request",
                column: "payment_reference_id");

            migrationBuilder.AddForeignKey(
                name: "fk_account_current_level_currency_currency_id",
                schema: "limit",
                table: "account_current_level",
                column: "currency_code",
                principalSchema: "core",
                principalTable: "currency",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_pricing_profile_bank_bank_id",
                schema: "core",
                table: "pricing_profile",
                column: "bank_code",
                principalSchema: "core",
                principalTable: "bank",
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

            migrationBuilder.AddForeignKey(
                name: "fk_tier_level_currency_currency_id",
                schema: "limit",
                table: "tier_level",
                column: "currency_code",
                principalSchema: "core",
                principalTable: "currency",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_transaction_currency_currency_id",
                schema: "core",
                table: "transaction",
                column: "currency_code",
                principalSchema: "core",
                principalTable: "currency",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_transfer_order_currency_currency_id",
                schema: "core",
                table: "transfer_order",
                column: "currency_code",
                principalSchema: "core",
                principalTable: "currency",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_wallet_currency_currency_id",
                schema: "core",
                table: "wallet",
                column: "currency_code",
                principalSchema: "core",
                principalTable: "currency",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
