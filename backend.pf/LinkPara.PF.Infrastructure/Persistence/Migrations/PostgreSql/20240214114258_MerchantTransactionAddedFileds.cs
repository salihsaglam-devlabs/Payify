using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class MerchantTransactionAddedFileds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "amount_without_bank_commission",
                schema: "merchant",
                table: "transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "amount_without_commissions",
                schema: "merchant",
                table: "transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "bsmv_amount",
                schema: "merchant",
                table: "transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "pf_commission_amount",
                schema: "merchant",
                table: "transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "pf_commission_rate",
                schema: "merchant",
                table: "transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "pf_net_commission_amount",
                schema: "merchant",
                table: "transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "pricing_profile_item_id",
                schema: "merchant",
                table: "transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "amount_without_bank_commission",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "amount_without_commissions",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "bsmv_amount",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "pf_commission_amount",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "pf_commission_rate",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "pf_net_commission_amount",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "pricing_profile_item_id",
                schema: "merchant",
                table: "transaction");
        }
    }
}
