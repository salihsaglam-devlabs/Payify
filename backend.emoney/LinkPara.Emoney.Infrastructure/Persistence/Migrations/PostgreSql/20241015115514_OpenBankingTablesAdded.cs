using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class OpenBankingTablesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "changed_balance_log",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    consent_id = table.Column<string>(type: "text", nullable: true),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    has_balance_changed = table.Column<bool>(type: "boolean", nullable: false),
                    last_event_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_changed_balance_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_changed_balance_log_account_account_id",
                        column: x => x.account_id,
                        principalSchema: "core",
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_changed_balance_log_wallet_wallet_id",
                        column: x => x.wallet_id,
                        principalSchema: "core",
                        principalTable: "wallet",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payment_order",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    consent_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    consent_create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    yos_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    sender_title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    sender_wallet_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    receiver_title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    receiver_wallet_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    receiver_iban = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_success = table.Column<bool>(type: "boolean", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_order", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_changed_balance_log_account_id",
                schema: "core",
                table: "changed_balance_log",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_changed_balance_log_wallet_id",
                schema: "core",
                table: "changed_balance_log",
                column: "wallet_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "changed_balance_log",
                schema: "core");

            migrationBuilder.DropTable(
                name: "payment_order",
                schema: "core");
        }
    }
}
