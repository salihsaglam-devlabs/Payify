using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class MerchantHistoryAndMerchantTransactionTablesUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "point_commission_amount",
                schema: "merchant",
                table: "transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "point_commission_rate",
                schema: "merchant",
                table: "transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "created_name_by",
                schema: "merchant",
                table: "history",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "point_commission_amount",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "point_commission_rate",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "created_name_by",
                schema: "merchant",
                table: "history");
        }
    }
}
