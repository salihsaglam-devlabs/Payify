using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class BulkTransferTablesUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "bsmv_amount",
                schema: "core",
                table: "bulk_transfer_detail",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "commission_amount",
                schema: "core",
                table: "bulk_transfer_detail",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "exception_message",
                schema: "core",
                table: "bulk_transfer_detail",
                type: "character varying(400)",
                maxLength: 400,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bsmv_amount",
                schema: "core",
                table: "bulk_transfer_detail");

            migrationBuilder.DropColumn(
                name: "commission_amount",
                schema: "core",
                table: "bulk_transfer_detail");

            migrationBuilder.DropColumn(
                name: "exception_message",
                schema: "core",
                table: "bulk_transfer_detail");
        }
    }
}
