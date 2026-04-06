using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class EditSubMerchant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "parameter_value",
                schema: "submerchant",
                table: "sub_merchant",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reject_reason",
                schema: "submerchant",
                table: "sub_merchant",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "parameter_value",
                schema: "submerchant",
                table: "sub_merchant");

            migrationBuilder.DropColumn(
                name: "reject_reason",
                schema: "submerchant",
                table: "sub_merchant");
        }
    }
}
