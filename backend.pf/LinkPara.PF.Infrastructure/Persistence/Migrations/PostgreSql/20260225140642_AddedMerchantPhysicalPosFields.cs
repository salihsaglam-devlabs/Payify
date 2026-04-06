using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedMerchantPhysicalPosFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "bkm_reference_number",
                schema: "merchant",
                table: "merchant_physical_pos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "terminal_status",
                schema: "merchant",
                table: "merchant_physical_pos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bkm_reference_number",
                schema: "merchant",
                table: "merchant_physical_pos");

            migrationBuilder.DropColumn(
                name: "terminal_status",
                schema: "merchant",
                table: "merchant_physical_pos");
        }
    }
}
