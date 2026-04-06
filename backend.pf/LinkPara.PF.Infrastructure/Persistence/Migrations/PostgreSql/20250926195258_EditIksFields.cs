using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class EditIksFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "bkm_reference_number",
                schema: "merchant",
                table: "vpos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "service_provider_psp_merchant_id",
                schema: "merchant",
                table: "vpos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "terminal_status",
                schema: "merchant",
                table: "vpos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "hosting_trade_name",
                schema: "merchant",
                table: "merchant",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "hosting_url",
                schema: "merchant",
                table: "merchant",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_pf_main_merchant_id",
                schema: "bank",
                table: "api_key",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "payment_gw_tax_no",
                schema: "bank",
                table: "acquire_bank",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_gw_trade_name",
                schema: "bank",
                table: "acquire_bank",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_gw_url",
                schema: "bank",
                table: "acquire_bank",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bkm_reference_number",
                schema: "merchant",
                table: "vpos");

            migrationBuilder.DropColumn(
                name: "service_provider_psp_merchant_id",
                schema: "merchant",
                table: "vpos");

            migrationBuilder.DropColumn(
                name: "terminal_status",
                schema: "merchant",
                table: "vpos");

            migrationBuilder.DropColumn(
                name: "hosting_trade_name",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.DropColumn(
                name: "hosting_url",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.DropColumn(
                name: "is_pf_main_merchant_id",
                schema: "bank",
                table: "api_key");

            migrationBuilder.DropColumn(
                name: "payment_gw_tax_no",
                schema: "bank",
                table: "acquire_bank");

            migrationBuilder.DropColumn(
                name: "payment_gw_trade_name",
                schema: "bank",
                table: "acquire_bank");

            migrationBuilder.DropColumn(
                name: "payment_gw_url",
                schema: "bank",
                table: "acquire_bank");
        }
    }
}
