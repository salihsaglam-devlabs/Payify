using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedMoneyTransferStartHourToMerchant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "money_transfer_start_hour",
                schema: "merchant",
                table: "pool",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "money_transfer_start_minute",
                schema: "merchant",
                table: "pool",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "money_transfer_start_hour",
                schema: "merchant",
                table: "merchant",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "money_transfer_start_minute",
                schema: "merchant",
                table: "merchant",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "money_transfer_start_hour",
                schema: "merchant",
                table: "pool");

            migrationBuilder.DropColumn(
                name: "money_transfer_start_minute",
                schema: "merchant",
                table: "pool");

            migrationBuilder.DropColumn(
                name: "money_transfer_start_hour",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.DropColumn(
                name: "money_transfer_start_minute",
                schema: "merchant",
                table: "merchant");
        }
    }
}
