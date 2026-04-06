using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PostgreInitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.EnsureSchema(
                name: "limit");

            migrationBuilder.EnsureSchema(
                name: "partner");

            migrationBuilder.CreateTable(
                name: "account",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    phone_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    identity_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    account_kyc_level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    account_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    opening_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    closing_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    suspended_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    kyc_change_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    change_reason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_account", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bank",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bank", x => x.id);
                    table.UniqueConstraint("ak_bank_code", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "currency",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    symbol = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    currency_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_currency", x => x.id);
                    table.UniqueConstraint("ak_currency_code", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "partner",
                schema: "partner",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    partner_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_partner", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "partner_counter",
                schema: "partner",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    index = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_partner_counter", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "return_transaction_request",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transfer_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    receiver_iban_number = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    receiver_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    receiver_tax_number = table.Column<string>(type: "text", nullable: true),
                    receiver_bank_code = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    incoming_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_bank_code = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    money_transfer_payment_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    money_transfer_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    money_transfer_reference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_return_transaction_request", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "withdraw_request",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    withdraw_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transfer_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    wallet_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    internal_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    receiver_iban_number = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    receiver_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    receiver_tax_number = table.Column<string>(type: "text", nullable: true),
                    receiver_bank_code = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    description = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    transaction_bank_code = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    money_transfer_payment_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    money_transfer_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    money_transfer_reference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    receiver_bank_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    transaction_bank_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_withdraw_request", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "account_user",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    firstname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    lastname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    phone_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_account_user", x => x.id);
                    table.ForeignKey(
                        name: "fk_account_user_account_account_id",
                        column: x => x.account_id,
                        principalSchema: "core",
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bank_logo",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bytes = table.Column<byte[]>(type: "bytea", nullable: true),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    bank_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bank_logo", x => x.id);
                    table.ForeignKey(
                        name: "fk_bank_logo_bank_bank_id",
                        column: x => x.bank_id,
                        principalSchema: "core",
                        principalTable: "bank",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "saved_account",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    iban = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    bank_id = table.Column<Guid>(type: "uuid", nullable: true),
                    wallet_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    wallet_owner_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_saved_account", x => x.id);
                    table.ForeignKey(
                        name: "fk_saved_account_bank_bank_id",
                        column: x => x.bank_id,
                        principalSchema: "core",
                        principalTable: "bank",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account_current_level",
                schema: "limit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tier_level_id = table.Column<Guid>(type: "uuid", nullable: false),
                    level_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    daily_internal_transfer_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    daily_internal_transfer_count = table.Column<int>(type: "integer", nullable: false),
                    monthly_internal_transfer_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    monthly_internal_transfer_count = table.Column<int>(type: "integer", nullable: false),
                    daily_deposit_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    daily_deposit_count = table.Column<int>(type: "integer", nullable: false),
                    monthly_deposit_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    monthly_deposit_count = table.Column<int>(type: "integer", nullable: false),
                    daily_withdrawal_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    daily_withdrawal_count = table.Column<int>(type: "integer", nullable: false),
                    monthly_withdrawal_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    monthly_withdrawal_count = table.Column<int>(type: "integer", nullable: false),
                    daily_corporate_wallet_transfer_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    daily_corporate_wallet_transfer_count = table.Column<int>(type: "integer", nullable: false),
                    monthly_corporate_wallet_transfer_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    monthly_corporate_wallet_transfer_count = table.Column<int>(type: "integer", nullable: false),
                    daily_international_transfer_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    daily_international_transfer_count = table.Column<int>(type: "integer", nullable: false),
                    monthly_international_transfer_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    monthly_international_transfer_count = table.Column<int>(type: "integer", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(10)", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_account_current_level", x => x.id);
                    table.ForeignKey(
                        name: "fk_account_current_level_currency_currency_id",
                        column: x => x.currency_code,
                        principalSchema: "core",
                        principalTable: "currency",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pricing_profile",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activation_date_start = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    activation_date_end = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    transfer_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    bank_code = table.Column<int>(type: "integer", nullable: true),
                    currency_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pricing_profile", x => x.id);
                    table.ForeignKey(
                        name: "fk_pricing_profile_bank_bank_id",
                        column: x => x.bank_code,
                        principalSchema: "core",
                        principalTable: "bank",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pricing_profile_currency_currency_id",
                        column: x => x.currency_code,
                        principalSchema: "core",
                        principalTable: "currency",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tier_level",
                schema: "limit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tier_level_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    max_balance = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    max_balance_corporate_wallet = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    daily_max_internal_transfer_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    daily_max_internal_transfer_count = table.Column<int>(type: "integer", nullable: false),
                    monthly_max_internal_transfer_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    monthly_max_internal_transfer_count = table.Column<int>(type: "integer", nullable: false),
                    daily_max_deposit_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    daily_max_deposit_count = table.Column<int>(type: "integer", nullable: false),
                    monthly_max_deposit_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    monthly_max_deposit_count = table.Column<int>(type: "integer", nullable: false),
                    daily_max_withdrawal_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    daily_max_withdrawal_count = table.Column<int>(type: "integer", nullable: false),
                    monthly_max_withdrawal_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    monthly_max_withdrawal_count = table.Column<int>(type: "integer", nullable: false),
                    daily_max_corporate_wallet_transfer_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    daily_max_corporate_wallet_transfer_count = table.Column<int>(type: "integer", nullable: false),
                    monthly_max_corporate_wallet_transfer_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    monthly_max_corporate_wallet_transfer_count = table.Column<int>(type: "integer", nullable: false),
                    daily_max_international_transfer_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    daily_max_international_transfer_count = table.Column<int>(type: "integer", nullable: false),
                    monthly_max_international_transfer_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    monthly_max_international_transfer_count = table.Column<int>(type: "integer", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(10)", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tier_level", x => x.id);
                    table.ForeignKey(
                        name: "fk_tier_level_currency_currency_id",
                        column: x => x.currency_code,
                        principalSchema: "core",
                        principalTable: "currency",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transfer_order",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_wallet_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    sender_name_surname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sender_user_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    receiver_account_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    receiver_account_value = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    receiver_name_surname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    receiver_wallet_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    transfer_date = table.Column<DateTime>(type: "date", nullable: false),
                    transfer_order_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    receiver_phone_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    currency_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transfer_order", x => x.id);
                    table.ForeignKey(
                        name: "fk_transfer_order_currency_currency_id",
                        column: x => x.currency_code,
                        principalSchema: "core",
                        principalTable: "currency",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "wallet",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    wallet_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    friendly_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(10)", nullable: false),
                    current_balance_credit = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    current_balance_cash = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    blocked_balance = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    last_activity_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_main_wallet = table.Column<bool>(type: "boolean", nullable: false),
                    is_blocked = table.Column<bool>(type: "boolean", nullable: false),
                    exceeded_limits_this_month = table.Column<bool>(type: "boolean", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    opening_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    closing_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallet", x => x.id);
                    table.ForeignKey(
                        name: "fk_wallet_account_account_id",
                        column: x => x.account_id,
                        principalSchema: "core",
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_wallet_currency_currency_id",
                        column: x => x.currency_code,
                        principalSchema: "core",
                        principalTable: "currency",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "api_key",
                schema: "partner",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    partner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    public_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    private_key = table.Column<string>(type: "character varying(4001)", maxLength: 4001, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_api_key", x => x.id);
                    table.ForeignKey(
                        name: "fk_api_key_partner_partner_id",
                        column: x => x.partner_id,
                        principalSchema: "partner",
                        principalTable: "partner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pricing_profile_item",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fee = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    commission_rate = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    min_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    max_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    wallet_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    pricing_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pricing_profile_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_pricing_profile_item_pricing_profile_pricing_profile_id",
                        column: x => x.pricing_profile_id,
                        principalSchema: "core",
                        principalTable: "pricing_profile",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account_custom_tier",
                schema: "limit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tier_level_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_account_custom_tier", x => x.id);
                    table.ForeignKey(
                        name: "fk_account_custom_tier_tier_level_tier_level_id",
                        column: x => x.tier_level_id,
                        principalSchema: "limit",
                        principalTable: "tier_level",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transaction",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    payment_method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transaction_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(10)", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    pre_balance = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    current_balance = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    tag = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    transaction_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    external_transaction_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    external_reference_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    related_transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    incoming_transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    withdraw_request_id = table.Column<Guid>(type: "uuid", nullable: true),
                    receiver_bank_code = table.Column<int>(type: "integer", nullable: true),
                    receiver_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sender_bank_code = table.Column<int>(type: "integer", nullable: true),
                    sender_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    returned_transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    counter_wallet_id = table.Column<Guid>(type: "uuid", nullable: true),
                    channel = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transaction", x => x.id);
                    table.ForeignKey(
                        name: "fk_transaction_currency_currency_id",
                        column: x => x.currency_code,
                        principalSchema: "core",
                        principalTable: "currency",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transaction_wallet_wallet_id",
                        column: x => x.wallet_id,
                        principalSchema: "core",
                        principalTable: "wallet",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "provision",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    provision_source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    provision_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    conversation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    client_ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    error_code = table.Column<string>(type: "text", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    is_return = table.Column<bool>(type: "boolean", nullable: false),
                    return_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    partner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    provision_reference = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "000000000000000"),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_provision", x => x.id);
                    table.ForeignKey(
                        name: "fk_provision_partner_partner_id",
                        column: x => x.partner_id,
                        principalSchema: "partner",
                        principalTable: "partner",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_provision_transaction_transaction_id",
                        column: x => x.transaction_id,
                        principalSchema: "core",
                        principalTable: "transaction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_account_current_level_currency_code",
                schema: "limit",
                table: "account_current_level",
                column: "currency_code");

            migrationBuilder.CreateIndex(
                name: "ix_account_custom_tier_tier_level_id",
                schema: "limit",
                table: "account_custom_tier",
                column: "tier_level_id");

            migrationBuilder.CreateIndex(
                name: "ix_account_user_account_id",
                schema: "core",
                table: "account_user",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_api_key_partner_id",
                schema: "partner",
                table: "api_key",
                column: "partner_id");

            migrationBuilder.CreateIndex(
                name: "ix_api_key_public_key",
                schema: "partner",
                table: "api_key",
                column: "public_key");

            migrationBuilder.CreateIndex(
                name: "ix_bank_code",
                schema: "core",
                table: "bank",
                column: "code");

            migrationBuilder.CreateIndex(
                name: "ix_bank_logo_bank_id",
                schema: "core",
                table: "bank_logo",
                column: "bank_id");

            migrationBuilder.CreateIndex(
                name: "ix_currency_code",
                schema: "core",
                table: "currency",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_partner_partner_number",
                schema: "partner",
                table: "partner",
                column: "partner_number");

            migrationBuilder.CreateIndex(
                name: "ix_partner_counter_index",
                schema: "partner",
                table: "partner_counter",
                column: "index");

            migrationBuilder.CreateIndex(
                name: "ix_pricing_profile_bank_code",
                schema: "core",
                table: "pricing_profile",
                column: "bank_code");

            migrationBuilder.CreateIndex(
                name: "ix_pricing_profile_currency_code",
                schema: "core",
                table: "pricing_profile",
                column: "currency_code");

            migrationBuilder.CreateIndex(
                name: "ix_pricing_profile_status",
                schema: "core",
                table: "pricing_profile",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_pricing_profile_transfer_type",
                schema: "core",
                table: "pricing_profile",
                column: "transfer_type");

            migrationBuilder.CreateIndex(
                name: "ix_pricing_profile_item_pricing_profile_id",
                schema: "core",
                table: "pricing_profile_item",
                column: "pricing_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_provision_conversation_id",
                schema: "core",
                table: "provision",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "ix_provision_partner_id",
                schema: "core",
                table: "provision",
                column: "partner_id");

            migrationBuilder.CreateIndex(
                name: "ix_provision_provision_reference",
                schema: "core",
                table: "provision",
                column: "provision_reference");

            migrationBuilder.CreateIndex(
                name: "ix_provision_transaction_id",
                schema: "core",
                table: "provision",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_saved_account_bank_id",
                schema: "core",
                table: "saved_account",
                column: "bank_id");

            migrationBuilder.CreateIndex(
                name: "ix_saved_account_user_id",
                schema: "core",
                table: "saved_account",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tier_level_currency_code",
                schema: "limit",
                table: "tier_level",
                column: "currency_code");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_currency_code",
                schema: "core",
                table: "transaction",
                column: "currency_code");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_related_transaction_id",
                schema: "core",
                table: "transaction",
                column: "related_transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_transaction_date",
                schema: "core",
                table: "transaction",
                column: "transaction_date");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_transaction_status",
                schema: "core",
                table: "transaction",
                column: "transaction_status");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_wallet_id",
                schema: "core",
                table: "transaction",
                column: "wallet_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_withdraw_request_id",
                schema: "core",
                table: "transaction",
                column: "withdraw_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_transfer_order_currency_code",
                schema: "core",
                table: "transfer_order",
                column: "currency_code");

            migrationBuilder.CreateIndex(
                name: "ix_transfer_order_sender_wallet_number",
                schema: "core",
                table: "transfer_order",
                column: "sender_wallet_number");

            migrationBuilder.CreateIndex(
                name: "ix_transfer_order_transfer_date",
                schema: "core",
                table: "transfer_order",
                column: "transfer_date");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_account_id",
                schema: "core",
                table: "wallet",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_currency_code",
                schema: "core",
                table: "wallet",
                column: "currency_code");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_user_id",
                schema: "core",
                table: "wallet",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_wallet_number",
                schema: "core",
                table: "wallet",
                column: "wallet_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_withdraw_request_create_date",
                schema: "core",
                table: "withdraw_request",
                column: "create_date");

            migrationBuilder.CreateIndex(
                name: "ix_withdraw_request_wallet_number",
                schema: "core",
                table: "withdraw_request",
                column: "wallet_number");

            migrationBuilder.CreateIndex(
                name: "ix_withdraw_request_withdraw_status_record_status",
                schema: "core",
                table: "withdraw_request",
                columns: new[] { "withdraw_status", "record_status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_current_level",
                schema: "limit");

            migrationBuilder.DropTable(
                name: "account_custom_tier",
                schema: "limit");

            migrationBuilder.DropTable(
                name: "account_user",
                schema: "core");

            migrationBuilder.DropTable(
                name: "api_key",
                schema: "partner");

            migrationBuilder.DropTable(
                name: "bank_logo",
                schema: "core");

            migrationBuilder.DropTable(
                name: "partner_counter",
                schema: "partner");

            migrationBuilder.DropTable(
                name: "pricing_profile_item",
                schema: "core");

            migrationBuilder.DropTable(
                name: "provision",
                schema: "core");

            migrationBuilder.DropTable(
                name: "return_transaction_request",
                schema: "core");

            migrationBuilder.DropTable(
                name: "saved_account",
                schema: "core");

            migrationBuilder.DropTable(
                name: "transfer_order",
                schema: "core");

            migrationBuilder.DropTable(
                name: "withdraw_request",
                schema: "core");

            migrationBuilder.DropTable(
                name: "tier_level",
                schema: "limit");

            migrationBuilder.DropTable(
                name: "pricing_profile",
                schema: "core");

            migrationBuilder.DropTable(
                name: "partner",
                schema: "partner");

            migrationBuilder.DropTable(
                name: "transaction",
                schema: "core");

            migrationBuilder.DropTable(
                name: "bank",
                schema: "core");

            migrationBuilder.DropTable(
                name: "wallet",
                schema: "core");

            migrationBuilder.DropTable(
                name: "account",
                schema: "core");

            migrationBuilder.DropTable(
                name: "currency",
                schema: "core");
        }
    }
}
