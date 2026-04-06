using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class merchantTransactionAddedProvisionNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "provision_number",
                schema: "merchant",
                table: "transaction",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vpos_name",
                schema: "merchant",
                table: "transaction",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "provision_number",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "vpos_name",
                schema: "merchant",
                table: "transaction");
        }
    }
}
