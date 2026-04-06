using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PostgreInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "bank");

            migrationBuilder.EnsureSchema(
                name: "merchant");

            migrationBuilder.EnsureSchema(
                name: "posting");

            migrationBuilder.EnsureSchema(
                name: "vpos");

            migrationBuilder.EnsureSchema(
                name: "card");

            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.EnsureSchema(
                name: "limit");

            migrationBuilder.CreateTable(
                name: "bank",
                schema: "bank",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                name: "batch_status",
                schema: "posting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    posting_batch_level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    batch_summary = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_critical_error = table.Column<bool>(type: "boolean", nullable: false),
                    posting_date = table.Column<DateTime>(type: "date", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    finish_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    batch_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    batch_order = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_batch_status", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contact_person",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contact_person_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    identity_number = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    surname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    company_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    birth_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    company_phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    mobile_phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    mobile_phone_number_second = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contact_person", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "counter",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number_counter = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_counter", x => x.id);
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
                    number = table.Column<int>(type: "integer", maxLength: 5, nullable: false),
                    currency_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                name: "integrator",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    commission_rate = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_integrator", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "item",
                schema: "posting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    error_count = table.Column<int>(type: "integer", nullable: false),
                    total_count = table.Column<int>(type: "integer", nullable: false),
                    posting_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    batch_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mcc",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    max_individual_installment_count = table.Column<int>(type: "integer", nullable: false),
                    max_corporate_installment_count = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mcc", x => x.id);
                    table.UniqueConstraint("ak_mcc_code", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "response_code",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    response_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    display_message_tr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    display_message_en = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_response_code", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "store",
                schema: "card",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    card_number_hashed = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    card_number_encrypted = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    expire_date_encrypted = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_store", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transaction",
                schema: "posting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transaction_date = table.Column<DateTime>(type: "date", nullable: false),
                    posting_date = table.Column<DateTime>(type: "date", nullable: false),
                    payment_date = table.Column<DateTime>(type: "date", nullable: false),
                    old_payment_date = table.Column<DateTime>(type: "date", nullable: false),
                    card_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    order_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    installment_count = table.Column<int>(type: "integer", nullable: false),
                    currency = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    point_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    bank_commission_rate = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    bank_commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    amount_without_bank_commission = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    pf_commission_rate = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    pf_per_transaction_fee = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    pf_commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    pf_net_commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    amount_without_commissions = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    pricing_profile_number = table.Column<string>(type: "text", nullable: false),
                    batch_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    blockage_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    merchant_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    posting_bank_balance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    posting_balance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acquire_bank_code = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transaction", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "acquire_bank",
                schema: "bank",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_code = table.Column<int>(type: "integer", nullable: false),
                    end_of_day_hour = table.Column<int>(type: "integer", nullable: false),
                    end_of_day_minute = table.Column<int>(type: "integer", nullable: false),
                    accept_amex = table.Column<bool>(type: "boolean", nullable: false),
                    has_submerchant_integration = table.Column<bool>(type: "boolean", nullable: false),
                    card_network = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_acquire_bank", x => x.id);
                    table.ForeignKey(
                        name: "fk_acquire_bank_bank_bank_id",
                        column: x => x.bank_code,
                        principalSchema: "bank",
                        principalTable: "bank",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bin",
                schema: "card",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bin_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    card_brand = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    card_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    card_sub_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    card_network = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    country = table.Column<int>(type: "integer", nullable: false),
                    country_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_virtual = table.Column<bool>(type: "boolean", nullable: false),
                    bank_code = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bin", x => x.id);
                    table.ForeignKey(
                        name: "fk_bin_bank_bank_code",
                        column: x => x.bank_code,
                        principalSchema: "bank",
                        principalTable: "bank",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "loyalty",
                schema: "card",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    bank_code = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_loyalty", x => x.id);
                    table.ForeignKey(
                        name: "fk_loyalty_bank_bank_code",
                        column: x => x.bank_code,
                        principalSchema: "bank",
                        principalTable: "bank",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "loyalty_exception",
                schema: "card",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_code = table.Column<int>(type: "integer", nullable: false),
                    counter_bank_code = table.Column<int>(type: "integer", nullable: false),
                    allow_on_us = table.Column<bool>(type: "boolean", nullable: false),
                    allow_installment = table.Column<bool>(type: "boolean", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_loyalty_exception", x => x.id);
                    table.ForeignKey(
                        name: "fk_loyalty_exception_bank_bank_code",
                        column: x => x.bank_code,
                        principalSchema: "bank",
                        principalTable: "bank",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_loyalty_exception_bank_counter_bank_code",
                        column: x => x.counter_bank_code,
                        principalSchema: "bank",
                        principalTable: "bank",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transaction",
                schema: "bank",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transaction_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    order_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    point_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency = table.Column<int>(type: "integer", nullable: false),
                    installment_count = table.Column<int>(type: "integer", nullable: false),
                    card_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_reverse = table.Column<bool>(type: "boolean", nullable: false),
                    reverse_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is3ds = table.Column<bool>(type: "boolean", nullable: false),
                    issuer_bank_code = table.Column<int>(type: "integer", nullable: false),
                    acquire_bank_code = table.Column<int>(type: "integer", nullable: false),
                    merchant_code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    sub_merchant_code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    bank_order_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    rrn_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    approval_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    bank_response_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    bank_response_description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    bank_transaction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    transaction_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    transaction_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    vpos_id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transaction", x => x.id);
                    table.ForeignKey(
                        name: "fk_transaction_bank_acquire_bank_code",
                        column: x => x.acquire_bank_code,
                        principalSchema: "bank",
                        principalTable: "bank",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transaction_bank_issuer_bank_code",
                        column: x => x.issuer_bank_code,
                        principalSchema: "bank",
                        principalTable: "bank",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "customer",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    company_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    commercial_title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    trade_registration_number = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    tax_administration = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tax_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    mersis_number = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    country = table.Column<int>(type: "integer", nullable: false),
                    country_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    city = table.Column<int>(type: "integer", nullable: false),
                    city_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    district = table.Column<int>(type: "integer", nullable: false),
                    district_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    postal_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    address = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    contact_person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_contact_person_contact_person_id",
                        column: x => x.contact_person_id,
                        principalSchema: "core",
                        principalTable: "contact_person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pool",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_pool_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    merchant_name = table.Column<string>(type: "text", nullable: true),
                    company_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    commercial_title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    web_site_url = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    monthly_turnover = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    postal_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    address = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    phone_code = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    country = table.Column<int>(type: "integer", nullable: false),
                    country_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    city = table.Column<int>(type: "integer", nullable: false),
                    city_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    district = table.Column<int>(type: "integer", nullable: false),
                    district_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tax_administration = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tax_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    trade_registration_number = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    iban = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    bank_code = table.Column<int>(type: "integer", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(10)", nullable: true),
                    reject_reason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    parameter_value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    company_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    authorized_person_identity_number = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    authorized_person_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    authorized_person_surname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    authorized_person_birth_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    authorized_person_company_phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    authorized_person_mobile_phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    authorized_person_mobile_phone_number_second = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pool", x => x.id);
                    table.ForeignKey(
                        name: "fk_pool_bank_bank_code",
                        column: x => x.bank_code,
                        principalSchema: "bank",
                        principalTable: "bank",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pool_currency_currency_code",
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
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    pricing_profile_number = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    activation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    profile_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(10)", nullable: true),
                    per_transaction_fee = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pricing_profile", x => x.id);
                    table.ForeignKey(
                        name: "fk_pricing_profile_currency_currency_id",
                        column: x => x.currency_code,
                        principalSchema: "core",
                        principalTable: "currency",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "response_code",
                schema: "bank",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_code = table.Column<int>(type: "integer", nullable: false),
                    response_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    process_timeout_management = table.Column<bool>(type: "boolean", nullable: false),
                    merchant_response_code_id = table.Column<Guid>(type: "uuid", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_response_code", x => x.id);
                    table.ForeignKey(
                        name: "fk_response_code_bank_bank_code",
                        column: x => x.bank_code,
                        principalSchema: "bank",
                        principalTable: "bank",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_response_code_merchant_response_code_merchant_response_code",
                        column: x => x.merchant_response_code_id,
                        principalSchema: "merchant",
                        principalTable: "response_code",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "api_key",
                schema: "bank",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    acquire_bank_id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    mapping_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_api_key", x => x.id);
                    table.ForeignKey(
                        name: "fk_api_key_acquire_bank_acquire_bank_id",
                        column: x => x.acquire_bank_id,
                        principalSchema: "bank",
                        principalTable: "acquire_bank",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vpos",
                schema: "vpos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    vpos_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    acquire_bank_id = table.Column<Guid>(type: "uuid", nullable: false),
                    security_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vpos", x => x.id);
                    table.ForeignKey(
                        name: "fk_vpos_acquire_bank_acquire_bank_id",
                        column: x => x.acquire_bank_id,
                        principalSchema: "bank",
                        principalTable: "acquire_bank",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "merchant",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    number = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    merchant_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    application_channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    integration_mode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    mcc_code = table.Column<string>(type: "character varying(4)", nullable: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    web_site_url = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    monthly_turnover = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    phone_code = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    agreement_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sales_person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payment_due_day = table.Column<int>(type: "integer", nullable: false),
                    is3d_required = table.Column<bool>(type: "boolean", nullable: false),
                    is_document_required = table.Column<bool>(type: "boolean", nullable: false),
                    is_rubik3d_required = table.Column<bool>(type: "boolean", nullable: false),
                    half_secure_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    installment_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    international_card_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    pre_authorization_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    financial_transaction_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    payment_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    reject_reason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    parameter_value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    pricing_profile_number = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    merchant_pool_id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_integrator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    contact_person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    global_merchant_id = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    annulment_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    annulment_description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    annulment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_merchant", x => x.id);
                    table.ForeignKey(
                        name: "fk_merchant_contact_person_contact_person_id",
                        column: x => x.contact_person_id,
                        principalSchema: "core",
                        principalTable: "contact_person",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_merchant_customer_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "core",
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_merchant_mcc_mcc_id",
                        column: x => x.mcc_code,
                        principalSchema: "merchant",
                        principalTable: "mcc",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_merchant_merchant_integrator_merchant_integrator_id",
                        column: x => x.merchant_integrator_id,
                        principalSchema: "merchant",
                        principalTable: "integrator",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_merchant_merchant_pool_merchant_pool_id",
                        column: x => x.merchant_pool_id,
                        principalSchema: "merchant",
                        principalTable: "pool",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pricing_profile_item",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile_card_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    installment_number = table.Column<int>(type: "integer", nullable: false),
                    installment_number_end = table.Column<int>(type: "integer", nullable: false),
                    commission_rate = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    blocked_day_number = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    pricing_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                name: "bank_api_info",
                schema: "vpos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vpos_id = table.Column<Guid>(type: "uuid", nullable: false),
                    key_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bank_api_info", x => x.id);
                    table.ForeignKey(
                        name: "fk_bank_api_info_api_key_key_id",
                        column: x => x.key_id,
                        principalSchema: "bank",
                        principalTable: "api_key",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bank_api_info_vpos_vpos_id",
                        column: x => x.vpos_id,
                        principalSchema: "vpos",
                        principalTable: "vpos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cost_profile",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    activation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    point_commission = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    service_commission = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    profile_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    vpos_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(10)", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cost_profile", x => x.id);
                    table.ForeignKey(
                        name: "fk_cost_profile_currency_currency_id",
                        column: x => x.currency_code,
                        principalSchema: "core",
                        principalTable: "currency",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cost_profile_vpos_vpos_id",
                        column: x => x.vpos_id,
                        principalSchema: "vpos",
                        principalTable: "vpos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "currency",
                schema: "vpos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vpos_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(10)", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_currency", x => x.id);
                    table.ForeignKey(
                        name: "fk_currency_currency_currency_code",
                        column: x => x.currency_code,
                        principalSchema: "core",
                        principalTable: "currency",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_currency_vpos_vpos_id",
                        column: x => x.vpos_id,
                        principalSchema: "vpos",
                        principalTable: "vpos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "three_d_verification",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    order_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    card_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    installment_count = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    point_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    current_step = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    callback_url = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    issuer_bank_code = table.Column<int>(type: "integer", maxLength: 3, nullable: false),
                    acquire_bank_code = table.Column<int>(type: "integer", maxLength: 3, nullable: false),
                    merchant_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sub_merchant_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    bin_number = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    currency = table.Column<int>(type: "integer", nullable: false),
                    session_expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    bank_commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    bank_commission_rate = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    md = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    md_status = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    md_error_message = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    xid = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    eci = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cavv = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    payer_txn_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    txn_stat = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    three_d_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    hash_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bank_transaction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    bank_response_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    bank_response_description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    vpos_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_three_d_verification", x => x.id);
                    table.ForeignKey(
                        name: "fk_three_d_verification_vpos_vpos_id",
                        column: x => x.vpos_id,
                        principalSchema: "vpos",
                        principalTable: "vpos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "api_key",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    public_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    private_key_encrypted = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_api_key", x => x.id);
                    table.ForeignKey(
                        name: "fk_api_key_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "api_log",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    request = table.Column<string>(type: "text", nullable: true),
                    response = table.Column<string>(type: "text", nullable: true),
                    error_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    error_message = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_api_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_api_log_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "api_validation_log",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    error_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    error_message = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    point_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    card_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    currency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    installment_count = table.Column<int>(type: "integer", nullable: false),
                    three_d_session_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    conversation_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    original_reference_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    client_ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    language_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    api_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_api_validation_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_api_validation_log_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "balance",
                schema: "posting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_date = table.Column<DateTime>(type: "date", nullable: false),
                    posting_date = table.Column<DateTime>(type: "date", nullable: false),
                    payment_date = table.Column<DateTime>(type: "date", nullable: false),
                    currency = table.Column<int>(type: "integer", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_point_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_bank_commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_amount_without_bank_commission = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_pf_commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_pf_net_commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_amount_without_commissions = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_due_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_paying_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_chargeback_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_suspicious_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    money_transfer_payment_date = table.Column<DateTime>(type: "date", nullable: false),
                    money_transfer_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    money_transfer_reference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    batch_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    blockage_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_balance", x => x.id);
                    table.ForeignKey(
                        name: "fk_balance_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bank_account",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    iban = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    bank_code = table.Column<int>(type: "integer", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bank_account", x => x.id);
                    table.ForeignKey(
                        name: "fk_bank_account_bank_bank_code",
                        column: x => x.bank_code,
                        principalSchema: "bank",
                        principalTable: "bank",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bank_account_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bank_balance",
                schema: "posting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acquire_bank_code = table.Column<int>(type: "integer", nullable: false),
                    posting_date = table.Column<DateTime>(type: "date", nullable: false),
                    payment_date = table.Column<DateTime>(type: "date", nullable: false),
                    currency = table.Column<int>(type: "integer", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_point_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_bank_commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_amount_without_bank_commission = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_pf_commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_pf_net_commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_amount_without_commissions = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_due_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_chargeback_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_suspicious_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_paying_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    batch_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    blockage_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bank_balance", x => x.id);
                    table.ForeignKey(
                        name: "fk_bank_balance_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "blockage",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    blockage_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    remaining_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    merchant_blockage_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blockage", x => x.id);
                    table.ForeignKey(
                        name: "fk_blockage_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document", x => x.id);
                    table.ForeignKey(
                        name: "fk_document_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "email",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    email_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    report_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_email", x => x.id);
                    table.ForeignKey(
                        name: "fk_email_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "history",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_operation_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    new_data = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    old_data = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    detail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_history_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "merchant_daily_usage",
                schema: "limit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_merchant_daily_usage", x => x.id);
                    table.ForeignKey(
                        name: "fk_merchant_daily_usage_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "merchant_limit",
                schema: "limit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_limit_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    period = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    limit_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    max_piece = table.Column<int>(type: "integer", nullable: true),
                    max_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_merchant_limit", x => x.id);
                    table.ForeignKey(
                        name: "fk_merchant_limit_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "merchant_monthly_usage",
                schema: "limit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_merchant_monthly_usage", x => x.id);
                    table.ForeignKey(
                        name: "fk_merchant_monthly_usage_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "score",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    has_score_card = table.Column<bool>(type: "boolean", nullable: false),
                    score_card_score = table.Column<int>(type: "integer", nullable: true),
                    has_findeks_risk_report = table.Column<bool>(type: "boolean", nullable: false),
                    findeks_score = table.Column<int>(type: "integer", nullable: true),
                    alexa_rank = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    google_rank = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_score", x => x.id);
                    table.ForeignKey(
                        name: "fk_score_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "time_out_transaction",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    timeout_transaction_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transaction_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    card_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    original_order_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    conversation_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    order_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    sub_merchant_code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    transaction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acquire_bank_code = table.Column<int>(type: "integer", nullable: false),
                    vpos_id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency = table.Column<int>(type: "integer", nullable: false),
                    language_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    next_try_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    pos_error_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    pos_error_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    error_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    error_message = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    response_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    response_message = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_time_out_transaction", x => x.id);
                    table.ForeignKey(
                        name: "fk_time_out_transaction_bank_acquire_bank_code",
                        column: x => x.acquire_bank_code,
                        principalSchema: "bank",
                        principalTable: "bank",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_time_out_transaction_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "token",
                schema: "card",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    card_store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cvv_encrypted = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_token", x => x.id);
                    table.ForeignKey(
                        name: "fk_token_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_token_store_card_store_id",
                        column: x => x.card_store_id,
                        principalSchema: "card",
                        principalTable: "store",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transaction",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conversation_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    transaction_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transaction_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    order_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    point_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency = table.Column<int>(type: "integer", nullable: false),
                    installment_count = table.Column<int>(type: "integer", nullable: false),
                    bin_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    card_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    has_cvv = table.Column<bool>(type: "boolean", nullable: false),
                    has_expiry_date = table.Column<bool>(type: "boolean", nullable: false),
                    is_international = table.Column<bool>(type: "boolean", nullable: false),
                    is_amex = table.Column<bool>(type: "boolean", nullable: false),
                    is_reverse = table.Column<bool>(type: "boolean", nullable: false),
                    reverse_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_return = table.Column<bool>(type: "boolean", nullable: false),
                    return_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    return_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    returned_transaction_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_pre_close = table.Column<bool>(type: "boolean", nullable: false),
                    pre_close_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    pre_close_transaction_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is3ds = table.Column<bool>(type: "boolean", nullable: false),
                    three_d_session_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    bank_commission_rate = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    bank_commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    issuer_bank_code = table.Column<int>(type: "integer", nullable: false),
                    acquire_bank_code = table.Column<int>(type: "integer", nullable: false),
                    card_transaction_type = table.Column<string>(type: "varchar(50)", nullable: true),
                    integration_mode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    response_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    response_description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    transaction_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    transaction_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    vpos_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    batch_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    card_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transaction_date = table.Column<DateTime>(type: "date", nullable: false),
                    is_chargeback = table.Column<bool>(type: "boolean", nullable: false),
                    is_suspecious = table.Column<bool>(type: "boolean", nullable: false),
                    suspecious_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    merchant_customer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    merchant_customer_phone_number = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    card_holder_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transaction", x => x.id);
                    table.ForeignKey(
                        name: "fk_transaction_bank_acquire_bank_code",
                        column: x => x.acquire_bank_code,
                        principalSchema: "bank",
                        principalTable: "bank",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transaction_bank_issuer_bank_code",
                        column: x => x.issuer_bank_code,
                        principalSchema: "bank",
                        principalTable: "bank",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transaction_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    surname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    mobile_phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    role_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    role_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vpos",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vpos_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sub_merchant_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vpos", x => x.id);
                    table.ForeignKey(
                        name: "fk_vpos_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_vpos_vpos_vpos_id",
                        column: x => x.vpos_id,
                        principalSchema: "vpos",
                        principalTable: "vpos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cost_profile_item",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    card_transaction_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    profile_card_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    installment_number = table.Column<int>(type: "integer", nullable: false),
                    installment_number_end = table.Column<int>(type: "integer", nullable: false),
                    commission_rate = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    blocked_day_number = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    installment_support = table.Column<bool>(type: "boolean", nullable: true),
                    cost_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cost_profile_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_cost_profile_item_cost_profile_cost_profile_id",
                        column: x => x.cost_profile_id,
                        principalSchema: "core",
                        principalTable: "cost_profile",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "blockage_detail",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    posting_date = table.Column<DateTime>(type: "date", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    blockage_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    remaining_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    blockage_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    merchant_blockage_id = table.Column<Guid>(type: "uuid", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blockage_detail", x => x.id);
                    table.ForeignKey(
                        name: "fk_blockage_detail_blockage_merchant_blockage_id",
                        column: x => x.merchant_blockage_id,
                        principalSchema: "merchant",
                        principalTable: "blockage",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_blockage_detail_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transfer_error",
                schema: "posting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    posting_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    merchant_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    transfer_error_category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    error_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    stack_trace = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transfer_error", x => x.id);
                    table.ForeignKey(
                        name: "fk_transfer_error_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_transfer_error_transaction_merchant_transaction_id",
                        column: x => x.merchant_transaction_id,
                        principalSchema: "merchant",
                        principalTable: "transaction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_acquire_bank_bank_code",
                schema: "bank",
                table: "acquire_bank",
                column: "bank_code");

            migrationBuilder.CreateIndex(
                name: "ix_api_key_acquire_bank_id",
                schema: "bank",
                table: "api_key",
                column: "acquire_bank_id");

            migrationBuilder.CreateIndex(
                name: "ix_api_key_merchant_id",
                schema: "merchant",
                table: "api_key",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_api_log_merchant_id",
                schema: "merchant",
                table: "api_log",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_api_validation_log_merchant_id",
                schema: "merchant",
                table: "api_validation_log",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_balance_merchant_id",
                schema: "posting",
                table: "balance",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_bank_account_bank_code",
                schema: "merchant",
                table: "bank_account",
                column: "bank_code");

            migrationBuilder.CreateIndex(
                name: "ix_bank_account_merchant_id",
                schema: "merchant",
                table: "bank_account",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_bank_api_info_key_id",
                schema: "vpos",
                table: "bank_api_info",
                column: "key_id");

            migrationBuilder.CreateIndex(
                name: "ix_bank_api_info_vpos_id",
                schema: "vpos",
                table: "bank_api_info",
                column: "vpos_id");

            migrationBuilder.CreateIndex(
                name: "ix_bank_balance_merchant_id",
                schema: "posting",
                table: "bank_balance",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_batch_status_posting_date",
                schema: "posting",
                table: "batch_status",
                column: "posting_date");

            migrationBuilder.CreateIndex(
                name: "ix_bin_bank_code",
                schema: "card",
                table: "bin",
                column: "bank_code");

            migrationBuilder.CreateIndex(
                name: "ix_bin_bin_number",
                schema: "card",
                table: "bin",
                column: "bin_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_blockage_merchant_id",
                schema: "merchant",
                table: "blockage",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_blockage_detail_merchant_blockage_id",
                schema: "merchant",
                table: "blockage_detail",
                column: "merchant_blockage_id");

            migrationBuilder.CreateIndex(
                name: "ix_blockage_detail_merchant_id",
                schema: "merchant",
                table: "blockage_detail",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_cost_profile_currency_code",
                schema: "core",
                table: "cost_profile",
                column: "currency_code");

            migrationBuilder.CreateIndex(
                name: "ix_cost_profile_vpos_id",
                schema: "core",
                table: "cost_profile",
                column: "vpos_id");

            migrationBuilder.CreateIndex(
                name: "ix_cost_profile_item_cost_profile_id",
                schema: "core",
                table: "cost_profile_item",
                column: "cost_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_counter_number_counter",
                schema: "merchant",
                table: "counter",
                column: "number_counter",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_currency_code",
                schema: "core",
                table: "currency",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_currency_currency_code",
                schema: "vpos",
                table: "currency",
                column: "currency_code");

            migrationBuilder.CreateIndex(
                name: "ix_currency_vpos_id",
                schema: "vpos",
                table: "currency",
                column: "vpos_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_contact_person_id",
                schema: "core",
                table: "customer",
                column: "contact_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_document_merchant_id",
                schema: "merchant",
                table: "document",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_email_merchant_id",
                schema: "merchant",
                table: "email",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_history_merchant_id",
                schema: "merchant",
                table: "history",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_loyalty_bank_code",
                schema: "card",
                table: "loyalty",
                column: "bank_code");

            migrationBuilder.CreateIndex(
                name: "ix_loyalty_exception_bank_code",
                schema: "card",
                table: "loyalty_exception",
                column: "bank_code");

            migrationBuilder.CreateIndex(
                name: "ix_loyalty_exception_counter_bank_code",
                schema: "card",
                table: "loyalty_exception",
                column: "counter_bank_code");

            migrationBuilder.CreateIndex(
                name: "ix_merchant_contact_person_id",
                schema: "merchant",
                table: "merchant",
                column: "contact_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_merchant_customer_id",
                schema: "merchant",
                table: "merchant",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_merchant_mcc_code",
                schema: "merchant",
                table: "merchant",
                column: "mcc_code");

            migrationBuilder.CreateIndex(
                name: "ix_merchant_merchant_integrator_id",
                schema: "merchant",
                table: "merchant",
                column: "merchant_integrator_id");

            migrationBuilder.CreateIndex(
                name: "ix_merchant_merchant_pool_id",
                schema: "merchant",
                table: "merchant",
                column: "merchant_pool_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_merchant_daily_usage_merchant_id",
                schema: "limit",
                table: "merchant_daily_usage",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_merchant_limit_merchant_id",
                schema: "limit",
                table: "merchant_limit",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_merchant_monthly_usage_merchant_id",
                schema: "limit",
                table: "merchant_monthly_usage",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_pool_bank_code",
                schema: "merchant",
                table: "pool",
                column: "bank_code");

            migrationBuilder.CreateIndex(
                name: "ix_pool_currency_code",
                schema: "merchant",
                table: "pool",
                column: "currency_code");

            migrationBuilder.CreateIndex(
                name: "ix_pricing_profile_currency_code",
                schema: "core",
                table: "pricing_profile",
                column: "currency_code");

            migrationBuilder.CreateIndex(
                name: "ix_pricing_profile_item_pricing_profile_id",
                schema: "core",
                table: "pricing_profile_item",
                column: "pricing_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_response_code_bank_code",
                schema: "bank",
                table: "response_code",
                column: "bank_code");

            migrationBuilder.CreateIndex(
                name: "ix_response_code_merchant_response_code_id",
                schema: "bank",
                table: "response_code",
                column: "merchant_response_code_id");

            migrationBuilder.CreateIndex(
                name: "ix_score_merchant_id",
                schema: "merchant",
                table: "score",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_three_d_verification_vpos_id",
                schema: "core",
                table: "three_d_verification",
                column: "vpos_id");

            migrationBuilder.CreateIndex(
                name: "ix_time_out_transaction_acquire_bank_code",
                schema: "core",
                table: "time_out_transaction",
                column: "acquire_bank_code");

            migrationBuilder.CreateIndex(
                name: "ix_time_out_transaction_merchant_id",
                schema: "core",
                table: "time_out_transaction",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_token_card_store_id",
                schema: "card",
                table: "token",
                column: "card_store_id");

            migrationBuilder.CreateIndex(
                name: "ix_token_expiry_date",
                schema: "card",
                table: "token",
                column: "expiry_date");

            migrationBuilder.CreateIndex(
                name: "ix_token_merchant_id",
                schema: "card",
                table: "token",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_acquire_bank_code",
                schema: "bank",
                table: "transaction",
                column: "acquire_bank_code");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_issuer_bank_code",
                schema: "bank",
                table: "transaction",
                column: "issuer_bank_code");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_acquire_bank_code1",
                schema: "merchant",
                table: "transaction",
                column: "acquire_bank_code");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_batch_status_record_status",
                schema: "merchant",
                table: "transaction",
                columns: new[] { "batch_status", "record_status" });

            migrationBuilder.CreateIndex(
                name: "ix_transaction_issuer_bank_code1",
                schema: "merchant",
                table: "transaction",
                column: "issuer_bank_code");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_merchant_id",
                schema: "merchant",
                table: "transaction",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_batch_status_record_status1",
                schema: "posting",
                table: "transaction",
                columns: new[] { "batch_status", "record_status" });

            migrationBuilder.CreateIndex(
                name: "ix_transaction_merchant_transaction_id",
                schema: "posting",
                table: "transaction",
                column: "merchant_transaction_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transfer_error_merchant_id",
                schema: "posting",
                table: "transfer_error",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_transfer_error_merchant_transaction_id",
                schema: "posting",
                table: "transfer_error",
                column: "merchant_transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_merchant_id",
                schema: "merchant",
                table: "user",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_vpos_merchant_id",
                schema: "merchant",
                table: "vpos",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_vpos_vpos_id",
                schema: "merchant",
                table: "vpos",
                column: "vpos_id");

            migrationBuilder.CreateIndex(
                name: "ix_vpos_acquire_bank_id",
                schema: "vpos",
                table: "vpos",
                column: "acquire_bank_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "api_key",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "api_log",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "api_validation_log",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "balance",
                schema: "posting");

            migrationBuilder.DropTable(
                name: "bank_account",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "bank_api_info",
                schema: "vpos");

            migrationBuilder.DropTable(
                name: "bank_balance",
                schema: "posting");

            migrationBuilder.DropTable(
                name: "batch_status",
                schema: "posting");

            migrationBuilder.DropTable(
                name: "bin",
                schema: "card");

            migrationBuilder.DropTable(
                name: "blockage_detail",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "cost_profile_item",
                schema: "core");

            migrationBuilder.DropTable(
                name: "counter",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "currency",
                schema: "vpos");

            migrationBuilder.DropTable(
                name: "document",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "email",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "history",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "item",
                schema: "posting");

            migrationBuilder.DropTable(
                name: "loyalty",
                schema: "card");

            migrationBuilder.DropTable(
                name: "loyalty_exception",
                schema: "card");

            migrationBuilder.DropTable(
                name: "merchant_daily_usage",
                schema: "limit");

            migrationBuilder.DropTable(
                name: "merchant_limit",
                schema: "limit");

            migrationBuilder.DropTable(
                name: "merchant_monthly_usage",
                schema: "limit");

            migrationBuilder.DropTable(
                name: "pricing_profile_item",
                schema: "core");

            migrationBuilder.DropTable(
                name: "response_code",
                schema: "bank");

            migrationBuilder.DropTable(
                name: "score",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "three_d_verification",
                schema: "core");

            migrationBuilder.DropTable(
                name: "time_out_transaction",
                schema: "core");

            migrationBuilder.DropTable(
                name: "token",
                schema: "card");

            migrationBuilder.DropTable(
                name: "transaction",
                schema: "bank");

            migrationBuilder.DropTable(
                name: "transaction",
                schema: "posting");

            migrationBuilder.DropTable(
                name: "transfer_error",
                schema: "posting");

            migrationBuilder.DropTable(
                name: "user",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "vpos",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "api_key",
                schema: "bank");

            migrationBuilder.DropTable(
                name: "blockage",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "cost_profile",
                schema: "core");

            migrationBuilder.DropTable(
                name: "pricing_profile",
                schema: "core");

            migrationBuilder.DropTable(
                name: "response_code",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "store",
                schema: "card");

            migrationBuilder.DropTable(
                name: "transaction",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "vpos",
                schema: "vpos");

            migrationBuilder.DropTable(
                name: "merchant",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "acquire_bank",
                schema: "bank");

            migrationBuilder.DropTable(
                name: "customer",
                schema: "core");

            migrationBuilder.DropTable(
                name: "mcc",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "integrator",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "pool",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "contact_person",
                schema: "core");

            migrationBuilder.DropTable(
                name: "bank",
                schema: "bank");

            migrationBuilder.DropTable(
                name: "currency",
                schema: "core");
        }
    }
}
