using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PhysicalPosTransactionsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "transaction_source",
                schema: "posting",
                table: "transaction",
                newName: "pf_transaction_source");

            migrationBuilder.RenameColumn(
                name: "transaction_source",
                schema: "merchant",
                table: "transaction",
                newName: "pf_transaction_source");

            migrationBuilder.AddColumn<Guid>(
                name: "merchant_physical_pos_id",
                schema: "posting",
                table: "transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "end_of_day_status",
                schema: "merchant",
                table: "transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<Guid>(
                name: "merchant_physical_pos_id",
                schema: "merchant",
                table: "transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "physical_pos_eod_id",
                schema: "merchant",
                table: "transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "physical_pos_old_eod_id",
                schema: "merchant",
                table: "transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "end_of_day_status",
                schema: "bank",
                table: "transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<Guid>(
                name: "merchant_physical_pos_id",
                schema: "bank",
                table: "transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "physical_pos_eod_id",
                schema: "bank",
                table: "transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "pos_merchant_id",
                schema: "merchant",
                table: "merchant_pyhsical_pos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pos_terminal_id",
                schema: "merchant",
                table: "merchant_pyhsical_pos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "end_of_day",
                schema: "physical",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    batch_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    pos_merchant_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    pos_terminal_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    sale_count = table.Column<int>(type: "integer", nullable: false),
                    void_count = table.Column<int>(type: "integer", nullable: false),
                    refund_count = table.Column<int>(type: "integer", nullable: false),
                    installment_sale_count = table.Column<int>(type: "integer", nullable: false),
                    failed_count = table.Column<int>(type: "integer", nullable: false),
                    sale_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    void_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    refund_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    installment_sale_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    institution_id = table.Column<int>(type: "integer", nullable: false),
                    vendor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    serial_number = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_end_of_day", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reconciliation_transaction",
                schema: "physical",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    batch_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    merchant_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    terminal_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    point_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    installment = table.Column<int>(type: "integer", nullable: false),
                    masked_card_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    bin_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    provision_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    issuer_bank_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    acquirer_response_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    institution_id = table.Column<int>(type: "integer", nullable: false),
                    vendor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    rrn = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    stan = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    pos_entry_mode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    pin_entry_info = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    bank_ref = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    original_ref = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    pf_merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conversation_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    client_ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    serial_number = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    reconciliation_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    merchant_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unacceptable_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    physical_pos_eod_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reconciliation_transaction", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "unacceptable_transaction",
                schema: "physical",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    batch_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    merchant_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    terminal_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    point_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    installment = table.Column<int>(type: "integer", nullable: false),
                    masked_card_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    bin_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    provision_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    issuer_bank_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    acquirer_response_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    institution_id = table.Column<int>(type: "integer", nullable: false),
                    vendor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    rrn = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    stan = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    pos_entry_mode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    pin_entry_info = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    bank_ref = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    original_ref = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    pf_merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conversation_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    client_ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    serial_number = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    gateway = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    error_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    error_message = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    current_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    physical_pos_eod_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_unacceptable_transaction", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "end_of_day",
                schema: "physical");

            migrationBuilder.DropTable(
                name: "reconciliation_transaction",
                schema: "physical");

            migrationBuilder.DropTable(
                name: "unacceptable_transaction",
                schema: "physical");

            migrationBuilder.DropColumn(
                name: "merchant_physical_pos_id",
                schema: "posting",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "end_of_day_status",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "merchant_physical_pos_id",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "physical_pos_eod_id",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "physical_pos_old_eod_id",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "end_of_day_status",
                schema: "bank",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "merchant_physical_pos_id",
                schema: "bank",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "physical_pos_eod_id",
                schema: "bank",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "pos_merchant_id",
                schema: "merchant",
                table: "merchant_pyhsical_pos");

            migrationBuilder.DropColumn(
                name: "pos_terminal_id",
                schema: "merchant",
                table: "merchant_pyhsical_pos");

            migrationBuilder.RenameColumn(
                name: "pf_transaction_source",
                schema: "posting",
                table: "transaction",
                newName: "transaction_source");

            migrationBuilder.RenameColumn(
                name: "pf_transaction_source",
                schema: "merchant",
                table: "transaction",
                newName: "transaction_source");
        }
    }
}
