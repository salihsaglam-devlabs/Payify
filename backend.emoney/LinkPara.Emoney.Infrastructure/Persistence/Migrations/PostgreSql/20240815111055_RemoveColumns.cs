using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class RemoveColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "name",
                schema: "core",
                table: "company_pool");

            migrationBuilder.DropColumn(
                name: "trade_registration_number",
                schema: "core",
                table: "company_pool");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "name",
                schema: "core",
                table: "company_pool",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "trade_registration_number",
                schema: "core",
                table: "company_pool",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");
        }
    }
}
