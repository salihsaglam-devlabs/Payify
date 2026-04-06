using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class EditMerchantTransactionBankCommissionRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "bank_commission_rate",
                schema: "merchant",
                table: "transaction",
                type: "numeric(5,3)",
                precision: 5,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(4,2)",
                oldPrecision: 4,
                oldScale: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "bank_commission_rate",
                schema: "merchant",
                table: "transaction",
                type: "numeric(4,2)",
                precision: 4,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,3)",
                oldPrecision: 5,
                oldScale: 3);
        }
    }
}
