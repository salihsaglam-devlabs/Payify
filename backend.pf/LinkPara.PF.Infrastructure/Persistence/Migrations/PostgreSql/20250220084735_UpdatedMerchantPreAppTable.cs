using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class UpdatedMerchantPreAppTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "product_type",
                schema: "merchant",
                table: "merchant_pre_application");

            migrationBuilder.AddColumn<int[]>(
                name: "product_types",
                schema: "merchant",
                table: "merchant_pre_application",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "product_types",
                schema: "merchant",
                table: "merchant_pre_application");

            migrationBuilder.AddColumn<string>(
                name: "product_type",
                schema: "merchant",
                table: "merchant_pre_application",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
