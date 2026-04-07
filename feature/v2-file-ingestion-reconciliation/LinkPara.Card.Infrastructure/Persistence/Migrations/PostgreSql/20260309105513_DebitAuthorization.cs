using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Card.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class DebitAuthorization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.CreateTable(
                name: "customer_wallet_card",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    banking_customer_no = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    wallet_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    card_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    product_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_wallet_card", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "debit_authorization",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    correlation_id = table.Column<long>(type: "bigint", nullable: false),
                    ocean_txn_guid = table.Column<long>(type: "bigint", nullable: false),
                    banking_customer_no = table.Column<string>(type: "text", nullable: true),
                    card_no = table.Column<string>(type: "text", nullable: true),
                    account_no = table.Column<string>(type: "text", nullable: true),
                    account_branch = table.Column<string>(type: "text", nullable: true),
                    account_suffix = table.Column<string>(type: "text", nullable: true),
                    account_currency = table.Column<int>(type: "integer", nullable: true),
                    iban = table.Column<string>(type: "text", nullable: true),
                    acquirer_country_code = table.Column<string>(type: "text", nullable: true),
                    national_switch_id = table.Column<string>(type: "text", nullable: true),
                    acquirer_id = table.Column<string>(type: "text", nullable: true),
                    terminal_id = table.Column<string>(type: "text", nullable: true),
                    merchant_id = table.Column<string>(type: "text", nullable: true),
                    merchant_name = table.Column<string>(type: "text", nullable: true),
                    rrn = table.Column<string>(type: "text", nullable: true),
                    provision_code = table.Column<string>(type: "text", nullable: true),
                    transaction_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    transaction_currency = table.Column<int>(type: "integer", nullable: false),
                    billing_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    billing_currency = table.Column<int>(type: "integer", nullable: false),
                    replacement_transaction_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    replacement_transaction_currency = table.Column<int>(type: "integer", nullable: true),
                    replacement_billing_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    replacement_billing_currency = table.Column<int>(type: "integer", nullable: true),
                    request_date = table.Column<long>(type: "bigint", nullable: false),
                    request_time = table.Column<long>(type: "bigint", nullable: false),
                    mcc = table.Column<string>(type: "text", nullable: true),
                    is_simulation = table.Column<bool>(type: "boolean", nullable: false),
                    is_advice = table.Column<bool>(type: "boolean", nullable: false),
                    request_type = table.Column<string>(type: "text", nullable: true),
                    transaction_type = table.Column<string>(type: "text", nullable: true),
                    expiration_time = table.Column<int>(type: "integer", nullable: true),
                    channel = table.Column<string>(type: "text", nullable: true),
                    terminal_type = table.Column<string>(type: "text", nullable: true),
                    banking_ref_no = table.Column<string>(type: "text", nullable: true),
                    transaction_source = table.Column<char>(type: "character(1)", nullable: false),
                    card_dci = table.Column<char>(type: "character(1)", nullable: false),
                    card_brand = table.Column<char>(type: "character(1)", nullable: false),
                    entry_type = table.Column<char>(type: "character(1)", nullable: false),
                    partial_acceptor = table.Column<bool>(type: "boolean", nullable: true),
                    transfer_information_type = table.Column<char>(type: "character(1)", nullable: true),
                    transfer_information_name = table.Column<string>(type: "text", nullable: true),
                    transfer_information_card_no = table.Column<string>(type: "text", nullable: true),
                    businesss_application_identifier = table.Column<string>(type: "text", nullable: true),
                    qr_data = table.Column<string>(type: "text", nullable: true),
                    security_level_indicator = table.Column<int>(type: "integer", nullable: true),
                    is_return = table.Column<bool>(type: "boolean", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_debit_authorization", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_debit_authorization_ocean_txn_guid",
                schema: "core",
                table: "debit_authorization",
                column: "ocean_txn_guid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_wallet_card",
                schema: "core");

            migrationBuilder.DropTable(
                name: "debit_authorization",
                schema: "core");
        }
    }
}
