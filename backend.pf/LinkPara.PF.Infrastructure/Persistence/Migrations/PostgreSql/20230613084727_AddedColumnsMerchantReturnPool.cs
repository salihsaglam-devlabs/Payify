using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedColumnsMerchantReturnPool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "bank_code",
                schema: "merchant",
                table: "merchant_return_pool",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "bank_name",
                schema: "merchant",
                table: "merchant_return_pool",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "bank_status",
                schema: "merchant",
                table: "merchant_return_pool",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "card_number",
                schema: "merchant",
                table: "merchant_return_pool",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reject_description",
                schema: "merchant",
                table: "merchant_return_pool",
                type: "character varying(400)",
                maxLength: 400,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bank_code",
                schema: "merchant",
                table: "merchant_return_pool");

            migrationBuilder.DropColumn(
                name: "bank_name",
                schema: "merchant",
                table: "merchant_return_pool");

            migrationBuilder.DropColumn(
                name: "bank_status",
                schema: "merchant",
                table: "merchant_return_pool");

            migrationBuilder.DropColumn(
                name: "card_number",
                schema: "merchant",
                table: "merchant_return_pool");

            migrationBuilder.DropColumn(
                name: "reject_description",
                schema: "merchant",
                table: "merchant_return_pool");
        }
    }
}
