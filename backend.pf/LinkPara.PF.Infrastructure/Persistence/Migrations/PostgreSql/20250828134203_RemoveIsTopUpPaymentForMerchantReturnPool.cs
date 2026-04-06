using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class RemoveIsTopUpPaymentForMerchantReturnPool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_top_up_payment",
                schema: "merchant",
                table: "merchant_return_pool");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_top_up_payment",
                schema: "merchant",
                table: "merchant_return_pool",
                type: "boolean",
                nullable: true);
        }
    }
}
