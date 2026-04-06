using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class OnUsPaymentRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "onus_payment_request",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    merchant_number = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    phone_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    wallet_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    error_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    error_message = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    cancel_description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    conversation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    order_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_onus_payment_request", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "onus_payment_request",
                schema: "core");
        }
    }
}
