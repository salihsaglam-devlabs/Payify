using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class EditMerchantReturnPoolTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "bank_response_code",
                schema: "merchant",
                table: "merchant_return_pool",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bank_response_description",
                schema: "merchant",
                table: "merchant_return_pool",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bank_response_code",
                schema: "merchant",
                table: "merchant_return_pool");

            migrationBuilder.DropColumn(
                name: "bank_response_description",
                schema: "merchant",
                table: "merchant_return_pool");
        }
    }
}
