using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AllowNotOnUsAndAllowPoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "allow_point",
                schema: "card",
                table: "loyalty_exception",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "allow_not_on_us",
                schema: "bank",
                table: "acquire_bank",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "allow_point",
                schema: "card",
                table: "loyalty_exception");

            migrationBuilder.DropColumn(
                name: "allow_not_on_us",
                schema: "bank",
                table: "acquire_bank");
        }
    }
}
