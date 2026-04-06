using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Accounting.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PaymentTableEditedForRegisterInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "transaction_type",
                schema: "core",
                table: "register_invoice",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "receiver_payment_invoice_status",
                schema: "core",
                table: "payment",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "receiver_register_invoice_id",
                schema: "core",
                table: "payment",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "sender_payment_invoice_status",
                schema: "core",
                table: "payment",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "sender_register_invoice_id",
                schema: "core",
                table: "payment",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "transaction_type",
                schema: "core",
                table: "register_invoice");

            migrationBuilder.DropColumn(
                name: "receiver_payment_invoice_status",
                schema: "core",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "receiver_register_invoice_id",
                schema: "core",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "sender_payment_invoice_status",
                schema: "core",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "sender_register_invoice_id",
                schema: "core",
                table: "payment");
        }
    }
}
