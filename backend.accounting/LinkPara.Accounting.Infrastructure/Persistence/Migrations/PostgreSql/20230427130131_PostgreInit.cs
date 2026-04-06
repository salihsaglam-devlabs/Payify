using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Accounting.Infrastructure.Persistence.Migrations.PostgreSql
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
                name: "bank_account",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_name = table.Column<string>(type: "character varying(350)", maxLength: 350, nullable: false),
                    bank_code = table.Column<int>(type: "integer", nullable: false),
                    account_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    accounting_transaction_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bank_account", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "customer",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    phone_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    identity_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    currency_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    is_success = table.Column<bool>(type: "boolean", nullable: false),
                    result_message = table.Column<string>(type: "character varying(800)", maxLength: 800, nullable: true),
                    accounting_customer_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "external_currency",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_currency_id = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    account_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    accounting_customer_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_external_currency", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    operation_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    has_commission = table.Column<bool>(type: "boolean", nullable: false),
                    source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    destination = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    bsmv_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    is_success = table.Column<bool>(type: "boolean", nullable: false),
                    result_message = table.Column<string>(type: "character varying(800)", maxLength: 800, nullable: true),
                    accounting_transaction_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    bank_code = table.Column<int>(type: "integer", nullable: false),
                    client_reference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_canceled = table.Column<bool>(type: "boolean", nullable: false),
                    cancel_result_message = table.Column<string>(type: "character varying(800)", maxLength: 800, nullable: true),
                    accounting_customer_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "template",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    has_commission = table.Column<bool>(type: "boolean", nullable: false),
                    external_operation_type = table.Column<int>(type: "integer", nullable: false),
                    tran_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    direction = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    account_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    template_expense_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_template", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_external_currency_code_accounting_customer_type",
                schema: "core",
                table: "external_currency",
                columns: new[] { "code", "accounting_customer_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_reference_id",
                schema: "core",
                table: "payment",
                column: "reference_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bank_account",
                schema: "core");

            migrationBuilder.DropTable(
                name: "customer",
                schema: "core");

            migrationBuilder.DropTable(
                name: "external_currency",
                schema: "core");

            migrationBuilder.DropTable(
                name: "payment",
                schema: "core");

            migrationBuilder.DropTable(
                name: "template",
                schema: "core");
        }
    }
}
