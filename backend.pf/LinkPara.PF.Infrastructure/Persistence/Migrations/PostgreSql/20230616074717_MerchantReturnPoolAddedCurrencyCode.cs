using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class MerchantReturnPoolAddedCurrencyCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "currency_code",
                schema: "merchant",
                table: "merchant_return_pool",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_merchant_return_pool_merchant_id",
                schema: "merchant",
                table: "merchant_return_pool",
                column: "merchant_id");

            migrationBuilder.AddForeignKey(
                name: "fk_merchant_return_pool_merchant_merchant_id",
                schema: "merchant",
                table: "merchant_return_pool",
                column: "merchant_id",
                principalSchema: "merchant",
                principalTable: "merchant",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_merchant_return_pool_merchant_merchant_id",
                schema: "merchant",
                table: "merchant_return_pool");

            migrationBuilder.DropIndex(
                name: "ix_merchant_return_pool_merchant_id",
                schema: "merchant",
                table: "merchant_return_pool");

            migrationBuilder.DropColumn(
                name: "currency_code",
                schema: "merchant",
                table: "merchant_return_pool");
        }
    }
}
