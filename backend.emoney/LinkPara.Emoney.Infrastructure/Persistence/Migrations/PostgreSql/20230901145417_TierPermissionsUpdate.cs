using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class TierPermissionsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "daily_max_corporate_wallet_transfer_amount",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "max_balance_corporate_wallet",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "monthly_max_corporate_wallet_transfer_amount",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.RenameColumn(
                name: "monthly_max_corporate_wallet_transfer_count",
                schema: "limit",
                table: "tier_level",
                newName: "monthly_max_distinct_iban_withdrawal_count");

            migrationBuilder.RenameColumn(
                name: "daily_max_corporate_wallet_transfer_count",
                schema: "limit",
                table: "tier_level",
                newName: "daily_max_distinct_iban_withdrawal_count");

            migrationBuilder.AddColumn<bool>(
                name: "distinct_iban_withdrawal_limit_check_enabled",
                schema: "limit",
                table: "tier_level",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "own_iban_limit_enabled",
                schema: "limit",
                table: "tier_level",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "tier_level_upgrade_path",
                schema: "limit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tier_level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    iban_validation = table.Column<bool>(type: "boolean", nullable: false),
                    identity_validation = table.Column<bool>(type: "boolean", nullable: false),
                    digital_kyc_validation = table.Column<bool>(type: "boolean", nullable: false),
                    next_tier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tier_level_upgrade_path", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tier_permission",
                schema: "limit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tier_level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    permission_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tier_permission", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tier_level_upgrade_path",
                schema: "limit");

            migrationBuilder.DropTable(
                name: "tier_permission",
                schema: "limit");

            migrationBuilder.DropColumn(
                name: "distinct_iban_withdrawal_limit_check_enabled",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.DropColumn(
                name: "own_iban_limit_enabled",
                schema: "limit",
                table: "tier_level");

            migrationBuilder.RenameColumn(
                name: "monthly_max_distinct_iban_withdrawal_count",
                schema: "limit",
                table: "tier_level",
                newName: "monthly_max_corporate_wallet_transfer_count");

            migrationBuilder.RenameColumn(
                name: "daily_max_distinct_iban_withdrawal_count",
                schema: "limit",
                table: "tier_level",
                newName: "daily_max_corporate_wallet_transfer_count");

            migrationBuilder.AddColumn<decimal>(
                name: "daily_max_corporate_wallet_transfer_amount",
                schema: "limit",
                table: "tier_level",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "max_balance_corporate_wallet",
                schema: "limit",
                table: "tier_level",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "monthly_max_corporate_wallet_transfer_amount",
                schema: "limit",
                table: "tier_level",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
