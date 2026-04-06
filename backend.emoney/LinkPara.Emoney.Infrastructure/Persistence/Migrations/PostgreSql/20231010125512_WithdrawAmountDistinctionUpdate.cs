using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class WithdrawAmountDistinctionUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "daily_max_distinct_other_iban_withdrawal_count",
                schema: "limit",
                table: "tier_level",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "daily_max_distinct_own_iban_withdrawal_count",
                schema: "limit",
                table: "tier_level",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "daily_max_other_iban_withdrawal_amount",
                schema: "limit",
                table: "tier_level",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "daily_max_own_iban_withdrawal_amount",
                schema: "limit",
                table: "tier_level",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "monthly_max_distinct_other_iban_withdrawal_count",
                schema: "limit",
                table: "tier_level",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "monthly_max_distinct_own_iban_withdrawal_count",
                schema: "limit",
                table: "tier_level",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "monthly_max_other_iban_withdrawal_amount",
                schema: "limit",
                table: "tier_level",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "monthly_max_own_iban_withdrawal_amount",
                schema: "limit",
                table: "tier_level",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "daily_distinct_other_iban_withdrawal_count",
                schema: "limit",
                table: "account_current_level",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "daily_distinct_own_iban_withdrawal_count",
                schema: "limit",
                table: "account_current_level",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "daily_other_iban_withdrawal_amount",
                schema: "limit",
                table: "account_current_level",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "daily_own_iban_withdrawal_amount",
                schema: "limit",
                table: "account_current_level",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "monthly_distinct_other_iban_withdrawal_count",
                schema: "limit",
                table: "account_current_level",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "monthly_distinct_own_iban_withdrawal_count",
                schema: "limit",
                table: "account_current_level",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "monthly_other_iban_withdrawal_amount",
                schema: "limit",
                table: "account_current_level",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "monthly_own_iban_withdrawal_amount",
                schema: "limit",
                table: "account_current_level",
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
                name: "daily_max_distinct_other_iban_withdrawal_count",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "daily_max_distinct_own_iban_withdrawal_count",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "daily_max_other_iban_withdrawal_amount",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "daily_max_own_iban_withdrawal_amount",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "monthly_max_distinct_other_iban_withdrawal_count",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "monthly_max_distinct_own_iban_withdrawal_count",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "monthly_max_other_iban_withdrawal_amount",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "monthly_max_own_iban_withdrawal_amount",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "daily_distinct_other_iban_withdrawal_count",
                schema: "limit",
                table: "account_current_level");

            migrationBuilder.DropColumn(
                name: "daily_distinct_own_iban_withdrawal_count",
                schema: "limit",
                table: "account_current_level");

            migrationBuilder.DropColumn(
                name: "daily_other_iban_withdrawal_amount",
                schema: "limit",
                table: "account_current_level");

            migrationBuilder.DropColumn(
                name: "daily_own_iban_withdrawal_amount",
                schema: "limit",
                table: "account_current_level");

            migrationBuilder.DropColumn(
                name: "monthly_distinct_other_iban_withdrawal_count",
                schema: "limit",
                table: "account_current_level");

            migrationBuilder.DropColumn(
                name: "monthly_distinct_own_iban_withdrawal_count",
                schema: "limit",
                table: "account_current_level");

            migrationBuilder.DropColumn(
                name: "monthly_other_iban_withdrawal_amount",
                schema: "limit",
                table: "account_current_level");

            migrationBuilder.DropColumn(
                name: "monthly_own_iban_withdrawal_amount",
                schema: "limit",
                table: "account_current_level");
        }
    }
}
