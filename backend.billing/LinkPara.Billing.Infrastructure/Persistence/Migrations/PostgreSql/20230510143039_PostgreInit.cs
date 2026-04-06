using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Billing.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PostgreInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.CreateTable(
                name: "authorization_token",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    access_token = table.Column<string>(type: "text", nullable: false),
                    refresh_token = table.Column<string>(type: "text", nullable: false),
                    token_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    vendor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    register_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_authorization_token", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sector",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sector", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "synchronization_log",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_name = table.Column<string>(type: "text", nullable: false),
                    vendor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    synchronization_item = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    synchronization_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    synchronization_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_synchronization_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vendor",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vendor", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "institution",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sector_id = table.Column<Guid>(type: "uuid", nullable: false),
                    active_vendor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_mode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    field_requirement_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_institution", x => x.id);
                    table.ForeignKey(
                        name: "fk_institution_sector_sector_id",
                        column: x => x.sector_id,
                        principalSchema: "core",
                        principalTable: "sector",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_institution_vendor_active_vendor_id",
                        column: x => x.active_vendor_id,
                        principalSchema: "core",
                        principalTable: "vendor",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "sector_mapping",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sector_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vendor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vendor_sector_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sector_mapping", x => x.id);
                    table.ForeignKey(
                        name: "fk_sector_mapping_sector_sector_id",
                        column: x => x.sector_id,
                        principalSchema: "core",
                        principalTable: "sector",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_sector_mapping_vendor_vendor_id",
                        column: x => x.vendor_id,
                        principalSchema: "core",
                        principalTable: "vendor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "summary",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vendor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_payment_count = table.Column<int>(type: "integer", nullable: false),
                    total_payment_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_cancel_count = table.Column<int>(type: "integer", nullable: false),
                    total_cancel_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    vendor_total_payment_count = table.Column<int>(type: "integer", nullable: false),
                    vendor_total_payment_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    vendor_total_cancel_count = table.Column<int>(type: "integer", nullable: false),
                    vendor_total_cancel_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    reconciliation_date = table.Column<DateTime>(type: "date", nullable: false),
                    reconciliation_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_summary", x => x.id);
                    table.ForeignKey(
                        name: "fk_summary_vendor_vendor_id",
                        column: x => x.vendor_id,
                        principalSchema: "core",
                        principalTable: "vendor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "commission",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vendor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    institution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rate = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    fee = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    min_value = table.Column<int>(type: "integer", nullable: false),
                    max_value = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_commission", x => x.id);
                    table.ForeignKey(
                        name: "fk_commission_institution_institution_id",
                        column: x => x.institution_id,
                        principalSchema: "core",
                        principalTable: "institution",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_commission_vendor_vendor_id",
                        column: x => x.vendor_id,
                        principalSchema: "core",
                        principalTable: "vendor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "field",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    mask = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    pattern = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    placeholder = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    length = table.Column<int>(type: "integer", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    prefix = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    suffix = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    institution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_field", x => x.id);
                    table.ForeignKey(
                        name: "fk_field_institution_institution_id",
                        column: x => x.institution_id,
                        principalSchema: "core",
                        principalTable: "institution",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "institutio_mapping",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    institution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vendor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer = table.Column<string>(type: "text", nullable: true),
                    vendor_institution_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_institutio_mapping", x => x.id);
                    table.ForeignKey(
                        name: "fk_institutio_mapping_institution_institution_id",
                        column: x => x.institution_id,
                        principalSchema: "core",
                        principalTable: "institution",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_institutio_mapping_vendor_vendor_id",
                        column: x => x.vendor_id,
                        principalSchema: "core",
                        principalTable: "vendor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "institutio_summary",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vendor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    institution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_payment_count = table.Column<int>(type: "integer", nullable: false),
                    total_payment_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_cancel_count = table.Column<int>(type: "integer", nullable: false),
                    total_cancel_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    vendor_total_payment_count = table.Column<int>(type: "integer", nullable: false),
                    vendor_total_payment_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    vendor_total_cancel_count = table.Column<int>(type: "integer", nullable: false),
                    vendor_total_cancel_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    reconciliation_date = table.Column<DateTime>(type: "date", nullable: false),
                    reconciliation_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_institutio_summary", x => x.id);
                    table.ForeignKey(
                        name: "fk_institutio_summary_institution_institution_id",
                        column: x => x.institution_id,
                        principalSchema: "core",
                        principalTable: "institution",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_institutio_summary_vendor_vendor_id",
                        column: x => x.vendor_id,
                        principalSchema: "core",
                        principalTable: "vendor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "saved_bill",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    institution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subscriber_number1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    subscriber_number2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    subscriber_number3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    bill_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_saved_bill", x => x.id);
                    table.ForeignKey(
                        name: "fk_saved_bill_institution_institution_id",
                        column: x => x.institution_id,
                        principalSchema: "core",
                        principalTable: "institution",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "timeout_transaction",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vendor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    institution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bill_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    commission_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    currency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    bill_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    bill_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    subscription_number1 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    subscription_number2 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    subscription_number3 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reference_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    voucher_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    wallet_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    bill_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    bill_due_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    payment_date = table.Column<DateTime>(type: "date", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    accounting_reference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provision_reference_id = table.Column<string>(type: "text", nullable: true),
                    subscriber_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    payee_full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    payee_email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    payee_mobile = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    service_request_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    error_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    next_try_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    timeout_transaction_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    timeout_transaction_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_timeout_transaction", x => x.id);
                    table.ForeignKey(
                        name: "fk_timeout_transaction_institution_institution_id",
                        column: x => x.institution_id,
                        principalSchema: "core",
                        principalTable: "institution",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transaction",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vendor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    institution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bill_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    bill_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    bill_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    subscription_number1 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    subscription_number2 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    subscription_number3 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reference_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    voucher_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    wallet_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    bill_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    bill_due_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    payment_date = table.Column<DateTime>(type: "date", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    accounting_reference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provision_reference_id = table.Column<string>(type: "text", nullable: true),
                    subscriber_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    payee_full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    payee_email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    payee_mobile = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    service_request_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    error_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    transaction_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                        name: "fk_transaction_institution_institution_id",
                        column: x => x.institution_id,
                        principalSchema: "core",
                        principalTable: "institution",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "institution_detail",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    institution_summary_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reconciliation_date = table.Column<DateTime>(type: "date", nullable: false),
                    bill_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    bill_due_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    bill_number = table.Column<string>(type: "text", nullable: true),
                    bill_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    bill_currency = table.Column<string>(type: "text", nullable: true),
                    payment_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    payment_currency = table.Column<string>(type: "text", nullable: true),
                    payment_reference_id = table.Column<string>(type: "text", nullable: true),
                    payment_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    reconciliation_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    vendor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    institution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_institution_detail", x => x.id);
                    table.ForeignKey(
                        name: "fk_institution_detail_transaction_transaction_id",
                        column: x => x.transaction_id,
                        principalSchema: "core",
                        principalTable: "transaction",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_authorization_token_vendor_id_expiry_date",
                schema: "core",
                table: "authorization_token",
                columns: new[] { "vendor_id", "expiry_date" });

            migrationBuilder.CreateIndex(
                name: "ix_commission_institution_id",
                schema: "core",
                table: "commission",
                column: "institution_id");

            migrationBuilder.CreateIndex(
                name: "ix_commission_vendor_id",
                schema: "core",
                table: "commission",
                column: "vendor_id");

            migrationBuilder.CreateIndex(
                name: "ix_field_institution_id",
                schema: "core",
                table: "field",
                column: "institution_id");

            migrationBuilder.CreateIndex(
                name: "ix_field_label_institution_id",
                schema: "core",
                table: "field",
                columns: new[] { "label", "institution_id" });

            migrationBuilder.CreateIndex(
                name: "ix_institutio_mapping_institution_id_vendor_id",
                schema: "core",
                table: "institutio_mapping",
                columns: new[] { "institution_id", "vendor_id" });

            migrationBuilder.CreateIndex(
                name: "ix_institutio_mapping_vendor_id",
                schema: "core",
                table: "institutio_mapping",
                column: "vendor_id");

            migrationBuilder.CreateIndex(
                name: "ix_institutio_summary_institution_id",
                schema: "core",
                table: "institutio_summary",
                column: "institution_id");

            migrationBuilder.CreateIndex(
                name: "ix_institutio_summary_vendor_id",
                schema: "core",
                table: "institutio_summary",
                column: "vendor_id");

            migrationBuilder.CreateIndex(
                name: "ix_institution_active_vendor_id",
                schema: "core",
                table: "institution",
                column: "active_vendor_id");

            migrationBuilder.CreateIndex(
                name: "ix_institution_sector_id",
                schema: "core",
                table: "institution",
                column: "sector_id");

            migrationBuilder.CreateIndex(
                name: "ix_institution_detail_transaction_id",
                schema: "core",
                table: "institution_detail",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_saved_bill_institution_id",
                schema: "core",
                table: "saved_bill",
                column: "institution_id");

            migrationBuilder.CreateIndex(
                name: "ix_sector_mapping_sector_id_vendor_id",
                schema: "core",
                table: "sector_mapping",
                columns: new[] { "sector_id", "vendor_id" });

            migrationBuilder.CreateIndex(
                name: "ix_sector_mapping_vendor_id",
                schema: "core",
                table: "sector_mapping",
                column: "vendor_id");

            migrationBuilder.CreateIndex(
                name: "ix_summary_vendor_id",
                schema: "core",
                table: "summary",
                column: "vendor_id");

            migrationBuilder.CreateIndex(
                name: "ix_timeout_transaction_institution_id",
                schema: "core",
                table: "timeout_transaction",
                column: "institution_id");

            migrationBuilder.CreateIndex(
                name: "ix_timeout_transaction_timeout_transaction_status_next_try_tim",
                schema: "core",
                table: "timeout_transaction",
                columns: new[] { "timeout_transaction_status", "next_try_time", "retry_count" });

            migrationBuilder.CreateIndex(
                name: "ix_transaction_bill_number",
                schema: "core",
                table: "transaction",
                column: "bill_number");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_institution_id",
                schema: "core",
                table: "transaction",
                column: "institution_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_payee_full_name",
                schema: "core",
                table: "transaction",
                column: "payee_full_name");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_subscription_number1",
                schema: "core",
                table: "transaction",
                column: "subscription_number1");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_transaction_status",
                schema: "core",
                table: "transaction",
                column: "transaction_status");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_vendor_id",
                schema: "core",
                table: "transaction",
                column: "vendor_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "authorization_token",
                schema: "core");

            migrationBuilder.DropTable(
                name: "commission",
                schema: "core");

            migrationBuilder.DropTable(
                name: "field",
                schema: "core");

            migrationBuilder.DropTable(
                name: "institutio_mapping",
                schema: "core");

            migrationBuilder.DropTable(
                name: "institutio_summary",
                schema: "core");

            migrationBuilder.DropTable(
                name: "institution_detail",
                schema: "core");

            migrationBuilder.DropTable(
                name: "saved_bill",
                schema: "core");

            migrationBuilder.DropTable(
                name: "sector_mapping",
                schema: "core");

            migrationBuilder.DropTable(
                name: "summary",
                schema: "core");

            migrationBuilder.DropTable(
                name: "synchronization_log");

            migrationBuilder.DropTable(
                name: "timeout_transaction",
                schema: "core");

            migrationBuilder.DropTable(
                name: "transaction",
                schema: "core");

            migrationBuilder.DropTable(
                name: "institution",
                schema: "core");

            migrationBuilder.DropTable(
                name: "sector",
                schema: "core");

            migrationBuilder.DropTable(
                name: "vendor",
                schema: "core");
        }
    }
}
