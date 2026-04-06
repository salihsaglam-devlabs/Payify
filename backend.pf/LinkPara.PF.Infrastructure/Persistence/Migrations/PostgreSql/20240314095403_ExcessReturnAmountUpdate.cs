using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class ExcessReturnAmountUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "posting_balance_id",
                schema: "merchant",
                table: "merchant_deduction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "is_excess_return_allowed",
                schema: "merchant",
                table: "merchant",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "total_excess_return_amount",
                schema: "posting",
                table: "balance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "total_negative_balance_amount",
                schema: "posting",
                table: "balance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "posting_balance_id",
                schema: "merchant",
                table: "merchant_deduction");

            migrationBuilder.DropColumn(
                name: "is_excess_return_allowed",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.DropColumn(
                name: "total_excess_return_amount",
                schema: "posting",
                table: "balance");

            migrationBuilder.DropColumn(
                name: "total_negative_balance_amount",
                schema: "posting",
                table: "balance");
        }
    }
}
