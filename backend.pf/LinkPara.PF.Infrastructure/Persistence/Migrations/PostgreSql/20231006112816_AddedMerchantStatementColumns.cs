using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedMerchantStatementColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "merchant_name",
                schema: "merchant",
                table: "merchant_statement",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "statement_month",
                schema: "merchant",
                table: "merchant_statement",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "statement_year",
                schema: "merchant",
                table: "merchant_statement",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "merchant_name",
                schema: "merchant",
                table: "merchant_statement");

            migrationBuilder.DropColumn(
                name: "statement_month",
                schema: "merchant",
                table: "merchant_statement");

            migrationBuilder.DropColumn(
                name: "statement_year",
                schema: "merchant",
                table: "merchant_statement");
        }
    }
}
