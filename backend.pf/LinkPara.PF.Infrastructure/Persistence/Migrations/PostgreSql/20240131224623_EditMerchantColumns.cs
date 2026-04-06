using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class EditMerchantColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_cvv_payment",
                schema: "merchant",
                table: "merchant",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_post_auth_amount_high",
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
                name: "is_cvv_payment",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.DropColumn(
                name: "is_post_auth_amount_high",
                schema: "merchant",
                table: "merchant");
        }
    }
}
