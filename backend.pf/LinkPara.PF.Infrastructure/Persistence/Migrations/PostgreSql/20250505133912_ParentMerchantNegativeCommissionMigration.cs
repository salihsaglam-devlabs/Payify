using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class ParentMerchantNegativeCommissionMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "amount_without_parent_merchant_commission",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "parent_merchant_commission_amount",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "parent_merchant_commission_rate",
                schema: "posting",
                table: "posting_additional_transaction",
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
                name: "amount_without_parent_merchant_commission",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "parent_merchant_commission_amount",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "parent_merchant_commission_rate",
                schema: "posting",
                table: "posting_additional_transaction");
        }
    }
}
