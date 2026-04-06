using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class MerchantInfoAddedToEndOfDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "merchant_name",
                schema: "physical",
                table: "end_of_day",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "merchant_number",
                schema: "physical",
                table: "end_of_day",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "merchant_name",
                schema: "physical",
                table: "end_of_day");

            migrationBuilder.DropColumn(
                name: "merchant_number",
                schema: "physical",
                table: "end_of_day");
        }
    }
}
