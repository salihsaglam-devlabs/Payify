using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class LinkTransactionAddedColumnnAndIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_link_transaction_order_id",
                schema: "link",
                table: "link_transaction");

            migrationBuilder.AddColumn<int>(
                name: "installment_count",
                schema: "link",
                table: "link_transaction",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "transaction_date",
                schema: "link",
                table: "link_transaction",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "transaction_type",
                schema: "link",
                table: "link_transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_link_transaction_link_code",
                schema: "link",
                table: "link_transaction",
                column: "link_code");

            migrationBuilder.CreateIndex(
                name: "ix_link_transaction_transaction_date",
                schema: "link",
                table: "link_transaction",
                column: "transaction_date");

            migrationBuilder.CreateIndex(
                name: "ix_link_customer_link_transaction_id",
                schema: "link",
                table: "link_customer",
                column: "link_transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_link_expiry_date",
                schema: "link",
                table: "link",
                column: "expiry_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_link_transaction_link_code",
                schema: "link",
                table: "link_transaction");

            migrationBuilder.DropIndex(
                name: "ix_link_transaction_transaction_date",
                schema: "link",
                table: "link_transaction");

            migrationBuilder.DropIndex(
                name: "ix_link_customer_link_transaction_id",
                schema: "link",
                table: "link_customer");

            migrationBuilder.DropIndex(
                name: "ix_link_expiry_date",
                schema: "link",
                table: "link");

            migrationBuilder.DropColumn(
                name: "installment_count",
                schema: "link",
                table: "link_transaction");

            migrationBuilder.DropColumn(
                name: "transaction_date",
                schema: "link",
                table: "link_transaction");

            migrationBuilder.DropColumn(
                name: "transaction_type",
                schema: "link",
                table: "link_transaction");

            migrationBuilder.CreateIndex(
                name: "ix_link_transaction_order_id",
                schema: "link",
                table: "link_transaction",
                column: "order_id");
        }
    }
}
