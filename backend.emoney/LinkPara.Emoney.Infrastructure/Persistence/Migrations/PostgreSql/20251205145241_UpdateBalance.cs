using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class UpdateBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "customer_transaction_id",
                schema: "core",
                table: "transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_cancelled",
                schema: "core",
                table: "transaction",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_settlement_received",
                schema: "core",
                table: "transaction",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "payment_channel",
                schema: "core",
                table: "transaction",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "customer_transaction_id",
                schema: "core",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "is_cancelled",
                schema: "core",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "is_settlement_received",
                schema: "core",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "payment_channel",
                schema: "core",
                table: "transaction");
        }
    }
}
