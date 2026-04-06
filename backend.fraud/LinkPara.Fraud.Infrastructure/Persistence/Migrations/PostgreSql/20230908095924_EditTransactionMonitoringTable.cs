using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Fraud.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class EditTransactionMonitoringTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "receiver_name",
                schema: "core",
                table: "transaction_monitoring",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sender_name",
                schema: "core",
                table: "transaction_monitoring",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "receiver_name",
                schema: "core",
                table: "transaction_monitoring");

            migrationBuilder.DropColumn(
                name: "sender_name",
                schema: "core",
                table: "transaction_monitoring");
        }
    }
}
