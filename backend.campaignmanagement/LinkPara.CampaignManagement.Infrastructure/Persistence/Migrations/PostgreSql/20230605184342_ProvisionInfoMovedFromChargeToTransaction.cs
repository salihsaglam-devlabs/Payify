using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.CampaignManagement.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class ProvisionInfoMovedFromChargeToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "provision_conversation_id",
                schema: "core",
                table: "i_wallet_charge");

            migrationBuilder.DropColumn(
                name: "provision_reference_number",
                schema: "core",
                table: "i_wallet_charge");

            migrationBuilder.DropColumn(
                name: "terminal_id",
                schema: "core",
                table: "i_wallet_charge");

            migrationBuilder.AddColumn<string>(
                name: "provision_conversation_id",
                schema: "core",
                table: "i_wallet_charge_transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provision_reference_number",
                schema: "core",
                table: "i_wallet_charge_transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "provision_conversation_id",
                schema: "core",
                table: "i_wallet_charge_transaction");

            migrationBuilder.DropColumn(
                name: "provision_reference_number",
                schema: "core",
                table: "i_wallet_charge_transaction");

            migrationBuilder.AddColumn<string>(
                name: "provision_conversation_id",
                schema: "core",
                table: "i_wallet_charge",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provision_reference_number",
                schema: "core",
                table: "i_wallet_charge",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "terminal_id",
                schema: "core",
                table: "i_wallet_charge",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
