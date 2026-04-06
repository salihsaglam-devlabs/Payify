using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class MerchantReturnAndReversePermissionAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "payment_return_allowed",
                schema: "merchant",
                table: "merchant",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "payment_reverse_allowed",
                schema: "merchant",
                table: "merchant",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payment_return_allowed",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.DropColumn(
                name: "payment_reverse_allowed",
                schema: "merchant",
                table: "merchant");
        }
    }
}
