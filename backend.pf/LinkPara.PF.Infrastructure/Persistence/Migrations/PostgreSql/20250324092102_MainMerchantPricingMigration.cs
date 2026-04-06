using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class MainMerchantPricingMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "amount_without_parent_merchant_commission",
                schema: "merchant",
                table: "transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "parent_merchant_commission_amount",
                schema: "merchant",
                table: "transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "parent_merchant_commission_rate",
                schema: "merchant",
                table: "transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "parent_merchant_commission_rate",
                schema: "core",
                table: "pricing_profile_item",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "profile_type",
                schema: "core",
                table: "pricing_profile",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "amount_without_parent_merchant_commission",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "parent_merchant_commission_amount",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "parent_merchant_commission_rate",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "parent_merchant_commission_rate",
                schema: "core",
                table: "pricing_profile_item");

            migrationBuilder.DropColumn(
                name: "profile_type",
                schema: "core",
                table: "pricing_profile");
        }
    }
}
