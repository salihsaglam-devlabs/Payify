using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class MerchantStatementMigrationUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "merchant_statement_id",
                schema: "merchant",
                table: "merchant_statement");

            migrationBuilder.DropColumn( 
                name: "error_code",
                schema: "merchant",
                table: "merchant_statement");

            migrationBuilder.DropColumn(
                name: "error_message",
                schema: "merchant",
                table: "merchant_statement");

            migrationBuilder.DropColumn(
                name: "request",
                schema: "merchant",
                table: "merchant_statement");

            migrationBuilder.DropColumn(
                name: "response",
                schema: "merchant",
                table: "merchant_statement");

            migrationBuilder.AddColumn<bool>(
                name: "is_success",
                schema: "merchant",
                table: "merchant_statement",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "merchant_statement_id",
                schema: "merchant",
                table: "merchant_statement",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.DropColumn(
                name: "is_success",
                schema: "merchant",
                table: "merchant_statement");

            migrationBuilder.AddColumn<string>(
                name: "error_code",
                schema: "merchant",
                table: "merchant_statement",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "error_message",
                schema: "merchant",
                table: "merchant_statement",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "request",
                schema: "merchant",
                table: "merchant_statement",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>( 
                name: "response",
                schema: "merchant",
                table: "merchant_statement",
                type: "text",
                nullable: true);

        }
    }
}
