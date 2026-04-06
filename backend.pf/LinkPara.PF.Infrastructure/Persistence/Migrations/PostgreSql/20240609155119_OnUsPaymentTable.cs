using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class OnUsPaymentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "on_us_payment",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    payment_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    webhook_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    merchant_number = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    surname = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    phone_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    wallet_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    emoney_reference_number = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    emoney_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    webhook_retry_count = table.Column<int>(type: "integer", nullable: false),
                    callback_url = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    order_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_on_us_payment", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_on_us_payment_expiry_date",
                schema: "core",
                table: "on_us_payment",
                column: "expiry_date");

            migrationBuilder.CreateIndex(
                name: "ix_on_us_payment_merchant_transaction_id",
                schema: "core",
                table: "on_us_payment",
                column: "merchant_transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_on_us_payment_payment_status",
                schema: "core",
                table: "on_us_payment",
                column: "payment_status");

            migrationBuilder.CreateIndex(
                name: "ix_on_us_payment_status",
                schema: "core",
                table: "on_us_payment",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_on_us_payment_webhook_status",
                schema: "core",
                table: "on_us_payment",
                column: "webhook_status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "on_us_payment",
                schema: "core");
        }
    }
}
