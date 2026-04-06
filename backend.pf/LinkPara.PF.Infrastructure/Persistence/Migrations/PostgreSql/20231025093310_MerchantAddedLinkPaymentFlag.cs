using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class MerchantAddedLinkPaymentFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "is_rubik3d_required",
                schema: "merchant",
                table: "merchant",
                newName: "is_manuel_payment3d_required");

            migrationBuilder.AddColumn<bool>(
                name: "is_link_payment3d_required",
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
                name: "is_link_payment3d_required",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.RenameColumn(
                name: "is_manuel_payment3d_required",
                schema: "merchant",
                table: "merchant",
                newName: "is_rubik3d_required");
        }
    }
}
