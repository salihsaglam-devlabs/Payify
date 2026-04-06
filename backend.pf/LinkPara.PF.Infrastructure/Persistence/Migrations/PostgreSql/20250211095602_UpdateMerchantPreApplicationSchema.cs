using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class UpdateMerchantPreApplicationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "merchant_pre_application_history",
                schema: "merchantPreApplication",
                newName: "merchant_pre_application_history",
                newSchema: "merchant");

            migrationBuilder.RenameTable(
                name: "merchant_pre_application",
                schema: "merchantPreApplication",
                newName: "merchant_pre_application",
                newSchema: "merchant");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "merchantPreApplication");

            migrationBuilder.RenameTable(
                name: "merchant_pre_application_history",
                schema: "merchant",
                newName: "merchant_pre_application_history",
                newSchema: "merchantPreApplication");

            migrationBuilder.RenameTable(
                name: "merchant_pre_application",
                schema: "merchant",
                newName: "merchant_pre_application",
                newSchema: "merchantPreApplication");
        }
    }
}
