using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedInsurancePaymentFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_insurance_vpos",
                schema: "vpos",
                table: "vpos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "insurance_payment_allowed",
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
                name: "is_insurance_vpos",
                schema: "vpos",
                table: "vpos");

            migrationBuilder.DropColumn(
                name: "insurance_payment_allowed",
                schema: "merchant",
                table: "merchant");
        }
    }
}
