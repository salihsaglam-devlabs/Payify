using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedPaymentToMainMerchantColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_payment_to_main_merchant",
                schema: "core",
                table: "pricing_profile",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_payment_to_main_merchant",
                schema: "merchant",
                table: "pool",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_payment_to_main_merchant",
                schema: "merchant",
                table: "merchant",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_payment_to_main_merchant",
                schema: "core",
                table: "pricing_profile");

            migrationBuilder.DropColumn(
                name: "is_payment_to_main_merchant",
                schema: "merchant",
                table: "pool");

            migrationBuilder.DropColumn(
                name: "is_payment_to_main_merchant",
                schema: "merchant",
                table: "merchant");
        }
    }
}
