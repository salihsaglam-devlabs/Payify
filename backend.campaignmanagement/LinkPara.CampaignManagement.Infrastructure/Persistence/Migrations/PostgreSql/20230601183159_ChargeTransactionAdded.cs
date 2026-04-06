using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.CampaignManagement.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class ChargeTransactionAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "i_wallet_reverse_charge",
                schema: "core");

            migrationBuilder.RenameColumn(
                name: "provision_reference_id",
                schema: "core",
                table: "i_wallet_charge",
                newName: "provision_reference_number");

            migrationBuilder.AddColumn<string>(
                name: "card_number",
                schema: "core",
                table: "i_wallet_qr_code",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "merchant_branch_id",
                schema: "core",
                table: "i_wallet_charge",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "merchant_branch_name",
                schema: "core",
                table: "i_wallet_charge",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "merchant_id",
                schema: "core",
                table: "i_wallet_charge",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "merchant_name",
                schema: "core",
                table: "i_wallet_charge",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provision_conversation_id",
                schema: "core",
                table: "i_wallet_charge",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "i_wallet_charge_transaction",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    i_wallet_charge_id = table.Column<Guid>(type: "uuid", nullable: false),
                    charge_transaction_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_i_wallet_charge_transaction", x => x.id);
                    table.ForeignKey(
                        name: "fk_i_wallet_charge_transaction_i_wallet_charge_i_wallet_charge",
                        column: x => x.i_wallet_charge_id,
                        principalSchema: "core",
                        principalTable: "i_wallet_charge",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_i_wallet_charge_transaction_i_wallet_charge_id",
                schema: "core",
                table: "i_wallet_charge_transaction",
                column: "i_wallet_charge_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "i_wallet_charge_transaction",
                schema: "core");

            migrationBuilder.DropColumn(
                name: "card_number",
                schema: "core",
                table: "i_wallet_qr_code");

            migrationBuilder.DropColumn(
                name: "merchant_branch_id",
                schema: "core",
                table: "i_wallet_charge");

            migrationBuilder.DropColumn(
                name: "merchant_branch_name",
                schema: "core",
                table: "i_wallet_charge");

            migrationBuilder.DropColumn(
                name: "merchant_id",
                schema: "core",
                table: "i_wallet_charge");

            migrationBuilder.DropColumn(
                name: "merchant_name",
                schema: "core",
                table: "i_wallet_charge");

            migrationBuilder.DropColumn(
                name: "provision_conversation_id",
                schema: "core",
                table: "i_wallet_charge");

            migrationBuilder.RenameColumn(
                name: "provision_reference_number",
                schema: "core",
                table: "i_wallet_charge",
                newName: "provision_reference_id");

            migrationBuilder.CreateTable(
                name: "i_wallet_reverse_charge",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    i_wallet_charge_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cash_back_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    error_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    reverse_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    reversed_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
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
                name: "ix_i_wallet_reverse_charge_i_wallet_charge_id",
                schema: "core",
                table: "i_wallet_reverse_charge",
                column: "i_wallet_charge_id");
        }
    }
}
