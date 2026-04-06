using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class MoneyTransferPaymentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "wallet_balance_daily",
                newName: "wallet_balance_daily",
                newSchema: "core");

            migrationBuilder.AddColumn<string>(
                name: "payment_type",
                schema: "core",
                table: "transaction",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payment_type",
                schema: "core",
                table: "transaction");

            migrationBuilder.RenameTable(
                name: "wallet_balance_daily",
                schema: "core",
                newName: "wallet_balance_daily");
        }
    }
}
