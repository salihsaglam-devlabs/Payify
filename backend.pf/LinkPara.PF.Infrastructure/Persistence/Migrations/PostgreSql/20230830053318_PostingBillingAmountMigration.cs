using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PostingBillingAmountMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_posting_bill_merchant_id",
                schema: "posting",
                table: "posting_bill");

            migrationBuilder.RenameColumn(
                name: "total_pf_net_commission_amount",
                schema: "posting",
                table: "posting_bill",
                newName: "total_paying_amount");

            migrationBuilder.AddColumn<int>(
                name: "bill_year",
                schema: "posting",
                table: "posting_bill",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "total_bank_commission_amount",
                schema: "posting",
                table: "posting_bill",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "ix_posting_bill_merchant_id_bill_month_bill_year",
                schema: "posting",
                table: "posting_bill",
                columns: new[] { "merchant_id", "bill_month", "bill_year" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_posting_bill_merchant_id_bill_month_bill_year",
                schema: "posting",
                table: "posting_bill");

            migrationBuilder.DropColumn(
                name: "bill_year",
                schema: "posting",
                table: "posting_bill");

            migrationBuilder.DropColumn(
                name: "total_bank_commission_amount",
                schema: "posting",
                table: "posting_bill");

            migrationBuilder.RenameColumn(
                name: "total_paying_amount",
                schema: "posting",
                table: "posting_bill",
                newName: "total_pf_net_commission_amount");

            migrationBuilder.CreateIndex(
                name: "ix_posting_bill_merchant_id",
                schema: "posting",
                table: "posting_bill",
                column: "merchant_id");
        }
    }
}
