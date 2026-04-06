using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class IsProcessedColumnAddedToWithdrawTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_processed",
                schema: "core",
                table: "withdraw_request",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_processed",
                schema: "core",
                table: "return_transaction_request",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_processed",
                schema: "core",
                table: "withdraw_request");

            migrationBuilder.DropColumn(
                name: "is_processed",
                schema: "core",
                table: "return_transaction_request");
        }
    }
}
