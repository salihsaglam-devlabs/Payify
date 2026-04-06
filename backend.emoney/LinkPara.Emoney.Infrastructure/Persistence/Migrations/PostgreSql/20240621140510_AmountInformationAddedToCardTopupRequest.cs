using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AmountInformationAddedToCardTopupRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "bsmv_total",
                schema: "core",
                table: "card_topup_request",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "commission_total",
                schema: "core",
                table: "card_topup_request",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "fee",
                schema: "core",
                table: "card_topup_request",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bsmv_total",
                schema: "core",
                table: "card_topup_request");

            migrationBuilder.DropColumn(
                name: "commission_total",
                schema: "core",
                table: "card_topup_request");

            migrationBuilder.DropColumn(
                name: "fee",
                schema: "core",
                table: "card_topup_request");
        }
    }
}
