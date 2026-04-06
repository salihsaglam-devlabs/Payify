using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AccountlevelOnUsColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "monthly_max_on_us_payment_count",
                schema: "limit",
                table: "account_current_level",
                newName: "monthly_on_us_payment_count");

            migrationBuilder.RenameColumn(
                name: "monthly_max_on_us_payment_amount",
                schema: "limit",
                table: "account_current_level",
                newName: "monthly_on_us_payment_amount");

            migrationBuilder.RenameColumn(
                name: "daily_max_on_us_payment_count",
                schema: "limit",
                table: "account_current_level",
                newName: "daily_on_us_payment_count");

            migrationBuilder.RenameColumn(
                name: "daily_max_on_us_payment_amount",
                schema: "limit",
                table: "account_current_level",
                newName: "daily_on_us_payment_amount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "monthly_on_us_payment_count",
                schema: "limit",
                table: "account_current_level",
                newName: "monthly_max_on_us_payment_count");

            migrationBuilder.RenameColumn(
                name: "monthly_on_us_payment_amount",
                schema: "limit",
                table: "account_current_level",
                newName: "monthly_max_on_us_payment_amount");

            migrationBuilder.RenameColumn(
                name: "daily_on_us_payment_count",
                schema: "limit",
                table: "account_current_level",
                newName: "daily_max_on_us_payment_count");

            migrationBuilder.RenameColumn(
                name: "daily_on_us_payment_amount",
                schema: "limit",
                table: "account_current_level",
                newName: "daily_max_on_us_payment_amount");
        }
    }
}
