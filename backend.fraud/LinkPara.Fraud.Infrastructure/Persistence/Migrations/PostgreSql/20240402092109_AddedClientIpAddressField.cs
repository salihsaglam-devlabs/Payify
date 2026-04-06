using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Fraud.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedClientIpAddressField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "client_ip_address",
                schema: "core",
                table: "transaction_monitoring",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "client_ip_address",
                schema: "core",
                table: "search_log",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "client_ip_address",
                schema: "core",
                table: "transaction_monitoring");

            migrationBuilder.DropColumn(
                name: "client_ip_address",
                schema: "core",
                table: "search_log");
        }
    }
}
