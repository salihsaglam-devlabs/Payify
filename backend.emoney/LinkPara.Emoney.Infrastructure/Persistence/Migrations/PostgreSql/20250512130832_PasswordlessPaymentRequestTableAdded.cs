using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PasswordlessPaymentRequestTableAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "passwordless_payment_request",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_reference_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    internal_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    sender_wallet_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sender_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    receiver_wallet_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    receiver_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_passwordless_payment_request", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_passwordless_payment_request_payment_reference_id",
                schema: "core",
                table: "passwordless_payment_request",
                column: "payment_reference_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "passwordless_payment_request",
                schema: "core");
        }
    }
}
