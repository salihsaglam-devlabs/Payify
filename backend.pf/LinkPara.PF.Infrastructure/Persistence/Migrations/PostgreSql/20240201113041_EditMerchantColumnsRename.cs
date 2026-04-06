using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class EditMerchantColumnsRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_cvv_payment",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.RenameColumn(
                name: "return_approval_status",
                schema: "merchant",
                table: "merchant",
                newName: "is_post_auth_amount_higher_allowed");

            migrationBuilder.RenameColumn(
                name: "is_post_auth_amount_high",
                schema: "merchant",
                table: "merchant",
                newName: "is_cvv_payment_allowed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "is_post_auth_amount_higher_allowed",
                schema: "merchant",
                table: "merchant",
                newName: "return_approval_status");

            migrationBuilder.RenameColumn(
                name: "is_cvv_payment_allowed",
                schema: "merchant",
                table: "merchant",
                newName: "is_post_auth_amount_high");

            migrationBuilder.AddColumn<bool>(
                name: "is_cvv_payment",
                schema: "merchant",
                table: "merchant",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
