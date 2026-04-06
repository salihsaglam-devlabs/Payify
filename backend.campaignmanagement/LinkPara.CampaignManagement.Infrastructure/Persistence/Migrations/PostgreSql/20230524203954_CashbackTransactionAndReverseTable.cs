using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.CampaignManagement.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class CashbackTransactionAndReverseTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "terminal_name",
                schema: "core",
                table: "i_wallet_charge",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "provision_reference_id",
                schema: "core",
                table: "i_wallet_charge",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                schema: "core",
                table: "i_wallet_charge",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "wallet_number",
                schema: "core",
                table: "i_wallet_charge",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "i_wallet_cash_back_transaction",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    i_wallet_charge_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    oid = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    external_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    vat_rate = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    commission_rate = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    external_order_id = table.Column<int>(type: "integer", nullable: false),
                    i_wallet_card_id = table.Column<int>(type: "integer", nullable: false),
                    wallet_id = table.Column<int>(type: "integer", nullable: false),
                    merchant_id = table.Column<int>(type: "integer", nullable: false),
                    merchant_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    merchant_branch_id = table.Column<int>(type: "integer", nullable: false),
                    merchant_branch_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    pos_id = table.Column<int>(type: "integer", nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    customer_branch_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    load_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    qr_code = table.Column<int>(type: "integer", nullable: false),
                    wallet_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    hash_data = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_i_wallet_cash_back_transaction", x => x.id);
                    table.ForeignKey(
                        name: "fk_i_wallet_cash_back_transaction_i_wallet_charge_i_wallet_cha",
                        column: x => x.i_wallet_charge_id,
                        principalSchema: "core",
                        principalTable: "i_wallet_charge",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "i_wallet_reverse_charge",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    i_wallet_charge_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reversed_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    cash_back_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    reverse_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    error_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_i_wallet_reverse_charge", x => x.id);
                    table.ForeignKey(
                        name: "fk_i_wallet_reverse_charge_i_wallet_charge_i_wallet_charge_id",
                        column: x => x.i_wallet_charge_id,
                        principalSchema: "core",
                        principalTable: "i_wallet_charge",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_i_wallet_cash_back_transaction_i_wallet_charge_id",
                schema: "core",
                table: "i_wallet_cash_back_transaction",
                column: "i_wallet_charge_id");

            migrationBuilder.CreateIndex(
                name: "ix_i_wallet_reverse_charge_i_wallet_charge_id",
                schema: "core",
                table: "i_wallet_reverse_charge",
                column: "i_wallet_charge_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "i_wallet_cash_back_transaction",
                schema: "core");

            migrationBuilder.DropTable(
                name: "i_wallet_reverse_charge",
                schema: "core");

            migrationBuilder.DropColumn(
                name: "provision_reference_id",
                schema: "core",
                table: "i_wallet_charge");

            migrationBuilder.DropColumn(
                name: "user_id",
                schema: "core",
                table: "i_wallet_charge");

            migrationBuilder.DropColumn(
                name: "wallet_number",
                schema: "core",
                table: "i_wallet_charge");

            migrationBuilder.AlterColumn<string>(
                name: "terminal_name",
                schema: "core",
                table: "i_wallet_charge",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);
        }
    }
}
