using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Card.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class Initial : Migration
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
                    transaction_source = table.Column<string>(type: "character(1)", nullable: false),
                    card_dci = table.Column<string>(type: "character(1)", nullable: false),
                    card_brand = table.Column<string>(type: "character(1)", nullable: false),
                    entry_type = table.Column<string>(type: "character(1)", nullable: false),
                    partial_acceptor = table.Column<bool>(type: "boolean", nullable: true),
                    transfer_information_type = table.Column<string>(type: "character(1)", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "imported_files",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_family = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    source_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    source_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    file_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    total_line_count = table.Column<int>(type: "integer", nullable: false),
                    header_code = table.Column<string>(type: "text", nullable: true),
                    footer_code = table.Column<string>(type: "text", nullable: true),
                    declared_file_date = table.Column<string>(type: "text", nullable: true),
                    declared_file_no = table.Column<string>(type: "text", nullable: true),
                    declared_file_version_number = table.Column<string>(type: "text", nullable: true),
                    declared_txn_count = table.Column<string>(type: "text", nullable: true),
                    processed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_imported_files", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "process_execution_lock",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lock_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    owner_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    acquired_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    released_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    job_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_process_execution_lock", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "imported_file_rows",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    imported_file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_no = table.Column<int>(type: "integer", nullable: false),
                    row_type = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    raw_line = table.Column<string>(type: "text", nullable: false),
                    parsed_json = table.Column<string>(type: "text", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_imported_file_rows", x => x.id);
                    table.ForeignKey(
                        name: "fk_imported_file_rows_imported_files_imported_file_id",
                        column: x => x.imported_file_id,
                        principalSchema: "core",
                        principalTable: "imported_files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "card_transaction_records",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    imported_file_row_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_date = table.Column<DateOnly>(type: "date", nullable: true),
                    transaction_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    value_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_of_day_date = table.Column<DateOnly>(type: "date", nullable: true),
                    card_no = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ocean_txn_guid = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ocean_main_txn_guid = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    branch_id = table.Column<string>(type: "text", nullable: true),
                    rrn = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    arn = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    provision_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    stan = table.Column<string>(type: "text", nullable: true),
                    member_ref_no = table.Column<string>(type: "text", nullable: true),
                    trace_id = table.Column<string>(type: "text", nullable: true),
                    otc = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    ots = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    txn_install_type = table.Column<string>(type: "text", nullable: true),
                    banking_txn_code = table.Column<string>(type: "text", nullable: true),
                    txn_description = table.Column<string>(type: "text", nullable: true),
                    merchant_name = table.Column<string>(type: "text", nullable: true),
                    merchant_city = table.Column<string>(type: "text", nullable: true),
                    merchant_state = table.Column<string>(type: "text", nullable: true),
                    merchant_country = table.Column<string>(type: "text", nullable: true),
                    financial_type = table.Column<string>(type: "text", nullable: true),
                    txn_effect = table.Column<string>(type: "text", nullable: true),
                    txn_source = table.Column<string>(type: "text", nullable: true),
                    txn_region = table.Column<string>(type: "text", nullable: true),
                    terminal_type = table.Column<string>(type: "text", nullable: true),
                    channel_code = table.Column<string>(type: "text", nullable: true),
                    terminal_id = table.Column<string>(type: "text", nullable: true),
                    merchant_id = table.Column<string>(type: "text", nullable: true),
                    mcc = table.Column<string>(type: "text", nullable: true),
                    acquirer_id = table.Column<string>(type: "text", nullable: true),
                    security_level_indicator = table.Column<string>(type: "text", nullable: true),
                    is_txn_settle = table.Column<string>(type: "text", nullable: true),
                    txn_stat = table.Column<string>(type: "text", nullable: true),
                    response_code = table.Column<string>(type: "text", nullable: true),
                    is_successful_txn = table.Column<string>(type: "text", nullable: true),
                    txn_origin = table.Column<int>(type: "integer", nullable: true),
                    install_count = table.Column<int>(type: "integer", nullable: true),
                    install_order = table.Column<int>(type: "integer", nullable: true),
                    operator_code = table.Column<string>(type: "text", nullable: true),
                    original_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    original_currency = table.Column<string>(type: "text", nullable: true),
                    settlement_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    settlement_currency = table.Column<string>(type: "text", nullable: true),
                    card_holder_billing_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    card_holder_billing_currency = table.Column<string>(type: "text", nullable: true),
                    billing_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    billing_currency = table.Column<string>(type: "text", nullable: true),
                    tax1 = table.Column<decimal>(type: "numeric", nullable: true),
                    tax2 = table.Column<decimal>(type: "numeric", nullable: true),
                    cashback_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    surcharge_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    point_type = table.Column<string>(type: "text", nullable: true),
                    bc_point = table.Column<decimal>(type: "numeric", nullable: true),
                    mc_point = table.Column<decimal>(type: "numeric", nullable: true),
                    cc_point = table.Column<decimal>(type: "numeric", nullable: true),
                    bc_point_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    mc_point_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    cc_point_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    correlation_key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    reconciliation_state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    reconciliation_state_updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    last_reconciliation_run_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_reconciliation_execution_group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reconciliation_state_reason = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_card_transaction_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_card_transaction_records_imported_file_rows_imported_file_r",
                        column: x => x.imported_file_row_id,
                        principalSchema: "core",
                        principalTable: "imported_file_rows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "clearing_records",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    imported_file_row_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    txn_type = table.Column<string>(type: "text", nullable: true),
                    io_date = table.Column<DateOnly>(type: "date", nullable: true),
                    io_flag = table.Column<string>(type: "text", nullable: true),
                    ocean_txn_guid = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    clr_no = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    rrn = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    arn = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    reason_code = table.Column<string>(type: "text", nullable: true),
                    reserved = table.Column<string>(type: "text", nullable: true),
                    provision_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    card_no = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    card_dci = table.Column<string>(type: "text", nullable: true),
                    mcc_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    mtid = table.Column<string>(type: "text", nullable: true),
                    function_code = table.Column<string>(type: "text", nullable: true),
                    process_code = table.Column<string>(type: "text", nullable: true),
                    reversal_indicator = table.Column<string>(type: "text", nullable: true),
                    tc = table.Column<string>(type: "text", nullable: true),
                    usage_code = table.Column<string>(type: "text", nullable: true),
                    dispute_code = table.Column<string>(type: "text", nullable: true),
                    control_stat = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    source_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    source_currency = table.Column<string>(type: "text", nullable: true),
                    destination_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    destination_currency = table.Column<string>(type: "text", nullable: true),
                    cashback_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    reimbursement_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    reimbursement_attribute = table.Column<string>(type: "text", nullable: true),
                    ancillary_transaction_code = table.Column<string>(type: "text", nullable: true),
                    ancillary_transaction_amount = table.Column<string>(type: "text", nullable: true),
                    microfilm_number = table.Column<string>(type: "text", nullable: true),
                    merchant_city = table.Column<string>(type: "text", nullable: true),
                    merchant_name = table.Column<string>(type: "text", nullable: true),
                    card_acceptor_id = table.Column<string>(type: "text", nullable: true),
                    txn_date = table.Column<DateOnly>(type: "date", nullable: true),
                    txn_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    file_id = table.Column<string>(type: "text", nullable: true),
                    correlation_key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clearing_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_clearing_records_imported_file_rows_imported_file_row_id",
                        column: x => x.imported_file_row_id,
                        principalSchema: "core",
                        principalTable: "imported_file_rows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reconciliation_evaluations",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    card_transaction_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    run_id = table.Column<Guid>(type: "uuid", nullable: true),
                    execution_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    decision_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    decision_code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    decision_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    has_clearing = table.Column<bool>(type: "boolean", nullable: false),
                    clearing_record_id = table.Column<Guid>(type: "uuid", nullable: true),
                    planned_operation_count = table.Column<int>(type: "integer", nullable: false),
                    planned_operation_codes = table.Column<string>(type: "text", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reconciliation_evaluations", x => x.id);
                    table.ForeignKey(
                        name: "fk_reconciliation_evaluations_card_transaction_records_card_tr",
                        column: x => x.card_transaction_record_id,
                        principalSchema: "core",
                        principalTable: "card_transaction_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reconciliation_evaluations_clearing_records_clearing_record",
                        column: x => x.clearing_record_id,
                        principalSchema: "core",
                        principalTable: "clearing_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reconciliation_operations",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    card_transaction_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    clearing_record_id = table.Column<Guid>(type: "uuid", nullable: true),
                    run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    execution_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_index = table.Column<int>(type: "integer", nullable: false),
                    depends_on_index = table.Column<int>(type: "integer", nullable: true),
                    is_approval_gate = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    operation_code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    mode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fingerprint = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    error_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    error_text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    payload = table.Column<string>(type: "text", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reconciliation_operations", x => x.id);
                    table.ForeignKey(
                        name: "fk_reconciliation_operations_card_transaction_records_card_tra",
                        column: x => x.card_transaction_record_id,
                        principalSchema: "core",
                        principalTable: "card_transaction_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reconciliation_operations_clearing_records_clearing_record_",
                        column: x => x.clearing_record_id,
                        principalSchema: "core",
                        principalTable: "clearing_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reconciliation_manual_review_items",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reconciliation_operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    execution_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    review_note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    reviewed_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reconciliation_manual_review_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_reconciliation_manual_review_items_reconciliation_operation",
                        column: x => x.reconciliation_operation_id,
                        principalSchema: "core",
                        principalTable: "reconciliation_operations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reconciliation_operation_executions",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reconciliation_operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    execution_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_no = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ended_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    outcome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    error_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    error_text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    request_payload = table.Column<string>(type: "text", nullable: true),
                    response_payload = table.Column<string>(type: "text", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reconciliation_operation_executions", x => x.id);
                    table.ForeignKey(
                        name: "fk_reconciliation_operation_executions_reconciliation_operatio",
                        column: x => x.reconciliation_operation_id,
                        principalSchema: "core",
                        principalTable: "reconciliation_operations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_card_transaction_records_card_no_otc_ots_card_holder_billin",
                schema: "core",
                table: "card_transaction_records",
                columns: new[] { "card_no", "otc", "ots", "card_holder_billing_amount" });

            migrationBuilder.CreateIndex(
                name: "ix_card_transaction_records_correlation_key",
                schema: "core",
                table: "card_transaction_records",
                column: "correlation_key");

            migrationBuilder.CreateIndex(
                name: "ix_card_transaction_records_imported_file_row_id",
                schema: "core",
                table: "card_transaction_records",
                column: "imported_file_row_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_card_transaction_records_last_reconciliation_execution_grou",
                schema: "core",
                table: "card_transaction_records",
                column: "last_reconciliation_execution_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_card_transaction_records_ocean_txn_guid",
                schema: "core",
                table: "card_transaction_records",
                column: "ocean_txn_guid");

            migrationBuilder.CreateIndex(
                name: "ix_card_transaction_records_reconciliation_state_reconciliatio",
                schema: "core",
                table: "card_transaction_records",
                columns: new[] { "reconciliation_state", "reconciliation_state_updated_at" });

            migrationBuilder.CreateIndex(
                name: "ix_clearing_records_clr_no_control_stat_provider",
                schema: "core",
                table: "clearing_records",
                columns: new[] { "clr_no", "control_stat", "provider" });

            migrationBuilder.CreateIndex(
                name: "ix_clearing_records_control_stat_card_no_create_date",
                schema: "core",
                table: "clearing_records",
                columns: new[] { "control_stat", "card_no", "create_date" });

            migrationBuilder.CreateIndex(
                name: "ix_clearing_records_correlation_key",
                schema: "core",
                table: "clearing_records",
                column: "correlation_key");

            migrationBuilder.CreateIndex(
                name: "ix_clearing_records_correlation_key_create_date",
                schema: "core",
                table: "clearing_records",
                columns: new[] { "correlation_key", "create_date" });

            migrationBuilder.CreateIndex(
                name: "ix_clearing_records_imported_file_row_id",
                schema: "core",
                table: "clearing_records",
                column: "imported_file_row_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_clearing_records_rrn_arn_provision_code_mcc_code_source_amo",
                schema: "core",
                table: "clearing_records",
                columns: new[] { "rrn", "arn", "provision_code", "mcc_code", "source_amount", "source_currency" });

            migrationBuilder.CreateIndex(
                name: "ix_debit_authorization_ocean_txn_guid",
                schema: "core",
                table: "debit_authorization",
                column: "ocean_txn_guid");

            migrationBuilder.CreateIndex(
                name: "ix_imported_file_rows_imported_file_id_line_no",
                schema: "core",
                table: "imported_file_rows",
                columns: new[] { "imported_file_id", "line_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_imported_files_file_family_file_name_source_type",
                schema: "core",
                table: "imported_files",
                columns: new[] { "file_family", "file_name", "source_type" });

            migrationBuilder.CreateIndex(
                name: "ix_imported_files_file_family_source_type_declared_file_date_d",
                schema: "core",
                table: "imported_files",
                columns: new[] { "file_family", "source_type", "declared_file_date", "declared_file_no", "declared_file_version_number" });

            migrationBuilder.CreateIndex(
                name: "ix_imported_files_file_hash",
                schema: "core",
                table: "imported_files",
                column: "file_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_process_execution_lock_acquired_at",
                schema: "core",
                table: "process_execution_lock",
                column: "acquired_at");

            migrationBuilder.CreateIndex(
                name: "ix_process_execution_lock_lock_name_status_expires_at",
                schema: "core",
                table: "process_execution_lock",
                columns: new[] { "lock_name", "status", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_evaluations_card_transaction_record_id",
                schema: "core",
                table: "reconciliation_evaluations",
                column: "card_transaction_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_evaluations_clearing_record_id",
                schema: "core",
                table: "reconciliation_evaluations",
                column: "clearing_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_evaluations_decision_type_create_date",
                schema: "core",
                table: "reconciliation_evaluations",
                columns: new[] { "decision_type", "create_date" });

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_evaluations_execution_group_id",
                schema: "core",
                table: "reconciliation_evaluations",
                column: "execution_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_manual_review_items_execution_group_id",
                schema: "core",
                table: "reconciliation_manual_review_items",
                column: "execution_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_manual_review_items_reconciliation_operation",
                schema: "core",
                table: "reconciliation_manual_review_items",
                column: "reconciliation_operation_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_manual_review_items_review_status_create_date",
                schema: "core",
                table: "reconciliation_manual_review_items",
                columns: new[] { "review_status", "create_date" });

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_manual_review_items_run_id",
                schema: "core",
                table: "reconciliation_manual_review_items",
                column: "run_id");

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_operation_executions_execution_group_id",
                schema: "core",
                table: "reconciliation_operation_executions",
                column: "execution_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_operation_executions_outcome_create_date",
                schema: "core",
                table: "reconciliation_operation_executions",
                columns: new[] { "outcome", "create_date" });

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_operation_executions_reconciliation_operatio",
                schema: "core",
                table: "reconciliation_operation_executions",
                column: "reconciliation_operation_id");

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_operation_executions_reconciliation_operatio1",
                schema: "core",
                table: "reconciliation_operation_executions",
                columns: new[] { "reconciliation_operation_id", "attempt_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_operations_card_transaction_record_id_create",
                schema: "core",
                table: "reconciliation_operations",
                columns: new[] { "card_transaction_record_id", "create_date" });

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_operations_card_transaction_record_id_run_id",
                schema: "core",
                table: "reconciliation_operations",
                columns: new[] { "card_transaction_record_id", "run_id" });

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_operations_clearing_record_id",
                schema: "core",
                table: "reconciliation_operations",
                column: "clearing_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_operations_execution_group_id",
                schema: "core",
                table: "reconciliation_operations",
                column: "execution_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_operations_idempotency_key",
                schema: "core",
                table: "reconciliation_operations",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_operations_mode_status_create_date",
                schema: "core",
                table: "reconciliation_operations",
                columns: new[] { "mode", "status", "create_date" });

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_operations_run_id_operation_index",
                schema: "core",
                table: "reconciliation_operations",
                columns: new[] { "run_id", "operation_index" },
                unique: true);
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

            migrationBuilder.DropTable(
                name: "process_execution_lock",
                schema: "core");

            migrationBuilder.DropTable(
                name: "reconciliation_evaluations",
                schema: "core");

            migrationBuilder.DropTable(
                name: "reconciliation_manual_review_items",
                schema: "core");

            migrationBuilder.DropTable(
                name: "reconciliation_operation_executions",
                schema: "core");

            migrationBuilder.DropTable(
                name: "reconciliation_operations",
                schema: "core");

            migrationBuilder.DropTable(
                name: "card_transaction_records",
                schema: "core");

            migrationBuilder.DropTable(
                name: "clearing_records",
                schema: "core");

            migrationBuilder.DropTable(
                name: "imported_file_rows",
                schema: "core");

            migrationBuilder.DropTable(
                name: "imported_files",
                schema: "core");
        }
    }
}
