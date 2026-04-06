using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Accounting.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class NewCustomerProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address",
                schema: "core",
                table: "customer",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city",
                schema: "core",
                table: "customer",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city_code",
                schema: "core",
                table: "customer",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "country",
                schema: "core",
                table: "customer",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "country_code",
                schema: "core",
                table: "customer",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tax_number",
                schema: "core",
                table: "customer",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tax_office",
                schema: "core",
                table: "customer",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tax_office_code",
                schema: "core",
                table: "customer",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address",
                schema: "core",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "city",
                schema: "core",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "city_code",
                schema: "core",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "country",
                schema: "core",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "country_code",
                schema: "core",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "tax_number",
                schema: "core",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "tax_office",
                schema: "core",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "tax_office_code",
                schema: "core",
                table: "customer");
        }
    }
}
