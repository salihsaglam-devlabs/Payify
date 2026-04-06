using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedMerchantInstallmentTransactionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "profile_settlement_mode",
                schema: "core",
                table: "pricing_profile");

            migrationBuilder.AddColumn<bool>(
                name: "is_per_installment",
                schema: "merchant",
                table: "transaction",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "cost_profile_item_id",
                schema: "core",
                table: "three_d_verification",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "is_per_installment",
                schema: "core",
                table: "three_d_verification",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "installment_transaction",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sub_merchant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sub_merchant_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    sub_merchant_number = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    conversation_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    transaction_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transaction_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    order_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    point_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    point_commission_rate = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    point_commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    service_commission_rate = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    service_commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency = table.Column<int>(type: "integer", nullable: false),
                    installment_count = table.Column<int>(type: "integer", nullable: false),
                    bin_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    card_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    has_cvv = table.Column<bool>(type: "boolean", nullable: false),
                    has_expiry_date = table.Column<bool>(type: "boolean", nullable: false),
                    is_international = table.Column<bool>(type: "boolean", nullable: false),
                    is_amex = table.Column<bool>(type: "boolean", nullable: false),
                    is_reverse = table.Column<bool>(type: "boolean", nullable: false),
                    is_manual_return = table.Column<bool>(type: "boolean", nullable: false),
                    is_on_us_payment = table.Column<bool>(type: "boolean", nullable: false),
                    is_insurance_payment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    reverse_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_return = table.Column<bool>(type: "boolean", nullable: false),
                    return_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    return_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    returned_transaction_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_pre_close = table.Column<bool>(type: "boolean", nullable: false),
                    pre_close_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    pre_close_transaction_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is3ds = table.Column<bool>(type: "boolean", nullable: false),
                    three_d_session_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    bank_commission_rate = table.Column<decimal>(type: "numeric(5,3)", precision: 5, scale: 3, nullable: false),
                    bank_commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    issuer_bank_code = table.Column<int>(type: "integer", nullable: false),
                    acquire_bank_code = table.Column<int>(type: "integer", nullable: false),
                    card_transaction_type = table.Column<string>(type: "varchar(50)", nullable: true),
                    integration_mode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    response_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    response_description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    transaction_start_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    transaction_end_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    vpos_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    batch_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    card_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transaction_date = table.Column<DateTime>(type: "date", nullable: false),
                    is_chargeback = table.Column<bool>(type: "boolean", nullable: false),
                    is_top_up_payment = table.Column<bool>(type: "boolean", nullable: true),
                    is_suspecious = table.Column<bool>(type: "boolean", nullable: false),
                    suspecious_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    last_chargeback_activity_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    merchant_customer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    merchant_customer_phone_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    merchant_customer_phone_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    card_holder_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    card_holder_identity_number = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    return_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_name_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    pf_commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    pf_net_commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    pf_commission_rate = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    pf_per_transaction_fee = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    parent_merchant_commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    parent_merchant_commission_rate = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    amount_without_commissions = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    amount_without_bank_commission = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    amount_without_parent_merchant_commission = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    pricing_profile_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bsmv_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    provision_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    vpos_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    pf_payment_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    bank_payment_date = table.Column<DateTime>(type: "date", nullable: false),
                    posting_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    blockage_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    pf_transaction_source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "VirtualPos"),
                    merchant_physical_pos_id = table.Column<Guid>(type: "uuid", nullable: false),
                    physical_pos_eod_id = table.Column<Guid>(type: "uuid", nullable: false),
                    physical_pos_old_eod_id = table.Column<Guid>(type: "uuid", nullable: false),
                    end_of_day_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_installment_transaction", x => x.id);
                    table.ForeignKey(
                        name: "fk_installment_transaction_bank_acquire_bank_code",
                        column: x => x.acquire_bank_code,
                        principalSchema: "bank",
                        principalTable: "bank",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_installment_transaction_bank_issuer_bank_code",
                        column: x => x.issuer_bank_code,
                        principalSchema: "bank",
                        principalTable: "bank",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_installment_transaction_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_installment_transaction_acquire_bank_code",
                schema: "merchant",
                table: "installment_transaction",
                column: "acquire_bank_code");

            migrationBuilder.CreateIndex(
                name: "ix_installment_transaction_batch_status_record_status",
                schema: "merchant",
                table: "installment_transaction",
                columns: new[] { "batch_status", "record_status" });

            migrationBuilder.CreateIndex(
                name: "ix_installment_transaction_issuer_bank_code",
                schema: "merchant",
                table: "installment_transaction",
                column: "issuer_bank_code");

            migrationBuilder.CreateIndex(
                name: "ix_installment_transaction_merchant_id",
                schema: "merchant",
                table: "installment_transaction",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_installment_transaction_posting_item_id",
                schema: "merchant",
                table: "installment_transaction",
                column: "posting_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_installment_transaction_transaction_date",
                schema: "merchant",
                table: "installment_transaction",
                column: "transaction_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "installment_transaction",
                schema: "merchant");

            migrationBuilder.DropColumn(
                name: "is_per_installment",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "cost_profile_item_id",
                schema: "core",
                table: "three_d_verification");

            migrationBuilder.DropColumn(
                name: "is_per_installment",
                schema: "core",
                table: "three_d_verification");

            migrationBuilder.AddColumn<string>(
                name: "profile_settlement_mode",
                schema: "core",
                table: "pricing_profile",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "SingleBlock");
        }
    }
}
