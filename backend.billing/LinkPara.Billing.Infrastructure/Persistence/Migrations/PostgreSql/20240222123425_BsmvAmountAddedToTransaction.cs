using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Billing.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class BsmvAmountAddedToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "bsmv_amount",
                schema: "core",
                table: "transaction",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bsmv_amount",
                schema: "core",
                table: "transaction");
        }
    }
}
