using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddTransactionLimitTypeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "transaction_limit_type",
                schema: "limit",
                table: "merchant_monthly_usage",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "transaction_limit_type",
                schema: "limit",
                table: "merchant_daily_usage",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "transaction_limit_type",
                schema: "limit",
                table: "merchant_monthly_usage");

            migrationBuilder.DropColumn(
                name: "transaction_limit_type",
                schema: "limit",
                table: "merchant_daily_usage");
        }
    }
}
