using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class LimitControlUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "digital_kyc_validation",
                schema: "limit",
                table: "tier_level_upgrade_path");

            migrationBuilder.DropColumn(
                name: "iban_validation",
                schema: "limit",
                table: "tier_level_upgrade_path");

            migrationBuilder.DropColumn(
                name: "identity_validation",
                schema: "limit",
                table: "tier_level_upgrade_path");

            migrationBuilder.DropColumn(
                name: "daily_corporate_wallet_transfer_amount",
                schema: "limit",
                table: "account_current_level");

            migrationBuilder.DropColumn(
                name: "monthly_corporate_wallet_transfer_amount",
                schema: "limit",
                table: "account_current_level");

            migrationBuilder.RenameColumn(
                name: "own_iban_limit_enabled",
                schema: "limit",
                table: "tier_level",
                newName: "max_withdrawal_limit_enabled");

            migrationBuilder.RenameColumn(
                name: "monthly_max_distinct_iban_withdrawal_count",
                schema: "limit",
                table: "tier_level",
                newName: "monthly_max_own_iban_withdrawal_count");

            migrationBuilder.RenameColumn(
                name: "distinct_iban_withdrawal_limit_check_enabled",
                schema: "limit",
                table: "tier_level",
                newName: "max_own_iban_withdrawal_limit_enabled");

            migrationBuilder.RenameColumn(
                name: "daily_max_distinct_iban_withdrawal_count",
                schema: "limit",
                table: "tier_level",
                newName: "monthly_max_other_iban_withdrawal_count");

            migrationBuilder.RenameColumn(
                name: "monthly_corporate_wallet_transfer_count",
                schema: "limit",
                table: "account_current_level",
                newName: "monthly_own_iban_withdrawal_count");

            migrationBuilder.RenameColumn(
                name: "daily_corporate_wallet_transfer_count",
                schema: "limit",
                table: "account_current_level",
                newName: "monthly_other_iban_withdrawal_count");

            migrationBuilder.AddColumn<bool>(
                name: "is_receiver_iban_owned",
                schema: "core",
                table: "withdraw_request",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "validation_type",
                schema: "limit",
                table: "tier_level_upgrade_path",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "daily_max_other_iban_withdrawal_count",
                schema: "limit",
                table: "tier_level",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "daily_max_own_iban_withdrawal_count",
                schema: "limit",
                table: "tier_level",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "max_balance_limit_enabled",
                schema: "limit",
                table: "tier_level",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "max_deposit_limit_enabled",
                schema: "limit",
                table: "tier_level",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "max_internal_transfer_limit_enabled",
                schema: "limit",
                table: "tier_level",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "max_international_transfer_limit_enabled",
                schema: "limit",
                table: "tier_level",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "max_other_iban_withdrawal_limit_enabled",
                schema: "limit",
                table: "tier_level",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "daily_other_iban_withdrawal_count",
                schema: "limit",
                table: "account_current_level",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "daily_own_iban_withdrawal_count",
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
                name: "is_receiver_iban_owned",
                schema: "core",
                table: "withdraw_request");

            migrationBuilder.DropColumn(
                name: "validation_type",
                schema: "limit",
                table: "tier_level_upgrade_path");

            migrationBuilder.DropColumn(
                name: "daily_max_other_iban_withdrawal_count",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "daily_max_own_iban_withdrawal_count",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "max_balance_limit_enabled",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "max_deposit_limit_enabled",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "max_internal_transfer_limit_enabled",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "max_international_transfer_limit_enabled",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "max_other_iban_withdrawal_limit_enabled",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "daily_other_iban_withdrawal_count",
                schema: "limit",
                table: "account_current_level");

            migrationBuilder.DropColumn(
                name: "daily_own_iban_withdrawal_count",
                schema: "limit",
                table: "account_current_level");

            migrationBuilder.RenameColumn(
                name: "monthly_max_own_iban_withdrawal_count",
                schema: "limit",
                table: "tier_level",
                newName: "monthly_max_distinct_iban_withdrawal_count");

            migrationBuilder.RenameColumn(
                name: "monthly_max_other_iban_withdrawal_count",
                schema: "limit",
                table: "tier_level",
                newName: "daily_max_distinct_iban_withdrawal_count");

            migrationBuilder.RenameColumn(
                name: "max_withdrawal_limit_enabled",
                schema: "limit",
                table: "tier_level",
                newName: "own_iban_limit_enabled");

            migrationBuilder.RenameColumn(
                name: "max_own_iban_withdrawal_limit_enabled",
                schema: "limit",
                table: "tier_level",
                newName: "distinct_iban_withdrawal_limit_check_enabled");

            migrationBuilder.RenameColumn(
                name: "monthly_own_iban_withdrawal_count",
                schema: "limit",
                table: "account_current_level",
                newName: "monthly_corporate_wallet_transfer_count");

            migrationBuilder.RenameColumn(
                name: "monthly_other_iban_withdrawal_count",
                schema: "limit",
                table: "account_current_level",
                newName: "daily_corporate_wallet_transfer_count");

            migrationBuilder.AddColumn<bool>(
                name: "digital_kyc_validation",
                schema: "limit",
                table: "tier_level_upgrade_path",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "iban_validation",
                schema: "limit",
                table: "tier_level_upgrade_path",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "identity_validation",
                schema: "limit",
                table: "tier_level_upgrade_path",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "daily_corporate_wallet_transfer_amount",
                schema: "limit",
                table: "account_current_level",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "monthly_corporate_wallet_transfer_amount",
                schema: "limit",
                table: "account_current_level",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
