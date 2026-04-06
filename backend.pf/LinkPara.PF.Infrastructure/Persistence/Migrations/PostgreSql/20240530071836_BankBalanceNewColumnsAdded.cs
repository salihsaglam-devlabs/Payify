using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class BankBalanceNewColumnsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "old_payment_date",
                schema: "posting",
                table: "bank_balance",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "total_return_amount",
                schema: "posting",
                table: "bank_balance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "transaction_count",
                schema: "posting",
                table: "bank_balance",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "old_payment_date",
                schema: "posting",
                table: "bank_balance");

            migrationBuilder.DropColumn(
                name: "total_return_amount",
                schema: "posting",
                table: "bank_balance");

            migrationBuilder.DropColumn(
                name: "transaction_count",
                schema: "posting",
                table: "bank_balance");
        }
    }
}
