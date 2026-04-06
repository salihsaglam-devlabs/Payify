using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class OnUsLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "daily_max_on_us_payment_amount",
                schema: "limit",
                table: "tier_level",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "daily_max_on_us_payment_count",
                schema: "limit",
                table: "tier_level",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "max_on_us_payment_limit_enabled",
                schema: "limit",
                table: "tier_level",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "monthly_max_on_us_payment_amount",
                schema: "limit",
                table: "tier_level",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "monthly_max_on_us_payment_count",
                schema: "limit",
                table: "tier_level",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "daily_max_on_us_payment_amount",
                schema: "limit",
                table: "account_current_level",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "daily_max_on_us_payment_count",
                schema: "limit",
                table: "account_current_level",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "monthly_max_on_us_payment_amount",
                schema: "limit",
                table: "account_current_level",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "monthly_max_on_us_payment_count",
                schema: "limit",
                table: "account_current_level",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "daily_max_on_us_payment_amount",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "daily_max_on_us_payment_count",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "max_on_us_payment_limit_enabled",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "monthly_max_on_us_payment_amount",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "monthly_max_on_us_payment_count",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "daily_max_on_us_payment_amount",
                schema: "limit",
                table: "account_current_level");

            migrationBuilder.DropColumn(
                name: "daily_max_on_us_payment_count",
                schema: "limit",
                table: "account_current_level");

            migrationBuilder.DropColumn(
                name: "monthly_max_on_us_payment_amount",
                schema: "limit",
                table: "account_current_level");

            migrationBuilder.DropColumn(
                name: "monthly_max_on_us_payment_count",
                schema: "limit",
                table: "account_current_level");
        }
    }
}
