using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddSubmerchantTerminalNoField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "password",
                schema: "merchant",
                table: "vpos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "terminal_no",
                schema: "merchant",
                table: "vpos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sub_merchant_terminal_no",
                schema: "core",
                table: "three_d_verification",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "password",
                schema: "merchant",
                table: "vpos");

            migrationBuilder.DropColumn(
                name: "terminal_no",
                schema: "merchant",
                table: "vpos");

            migrationBuilder.DropColumn(
                name: "sub_merchant_terminal_no",
                schema: "core",
                table: "three_d_verification");
        }
    }
}
