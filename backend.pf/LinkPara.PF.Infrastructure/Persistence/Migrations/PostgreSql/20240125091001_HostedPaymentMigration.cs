using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class HostedPaymentMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "hpp");

            migrationBuilder.AddColumn<bool>(
                name: "is_hosted_payment3d_required",
                schema: "merchant",
                table: "merchant",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_return_approved",
                schema: "merchant",
                table: "merchant",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "hosted_payment",
                schema: "hpp",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tracking_id = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    hpp_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    hpp_payment_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    webhook_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    order_id = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency = table.Column<int>(type: "integer", nullable: false),
                    is3d_required = table.Column<bool>(type: "boolean", nullable: false),
                    callback_url = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    surname = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    client_ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    language_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    merchant_number = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    webhook_retry_count = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hosted_payment", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hosted_payment_installment",
                schema: "hpp",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    hosted_payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    installment = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hosted_payment_installment", x => x.id);
                    table.ForeignKey(
                        name: "fk_hosted_payment_installment_hosted_payment_hosted_payment_id",
                        column: x => x.hosted_payment_id,
                        principalSchema: "hpp",
                        principalTable: "hosted_payment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_hosted_payment_expiry_date",
                schema: "hpp",
                table: "hosted_payment",
                column: "expiry_date");

            migrationBuilder.CreateIndex(
                name: "ix_hosted_payment_tracking_id",
                schema: "hpp",
                table: "hosted_payment",
                column: "tracking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hosted_payment_webhook_status",
                schema: "hpp",
                table: "hosted_payment",
                column: "webhook_status");

            migrationBuilder.CreateIndex(
                name: "ix_hosted_payment_installment_hosted_payment_id",
                schema: "hpp",
                table: "hosted_payment_installment",
                column: "hosted_payment_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hosted_payment_installment",
                schema: "hpp");

            migrationBuilder.DropTable(
                name: "hosted_payment",
                schema: "hpp");

            migrationBuilder.DropColumn(
                name: "is_hosted_payment3d_required",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.DropColumn(
                name: "is_return_approved",
                schema: "merchant",
                table: "merchant");
        }
    }
}
