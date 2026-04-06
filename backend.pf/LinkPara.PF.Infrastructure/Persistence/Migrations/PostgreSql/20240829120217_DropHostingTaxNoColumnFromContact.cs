using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class DropHostingTaxNoColumnFromContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "hosting_tax_no",
                schema: "core",
                table: "contact_person");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "hosting_tax_no",
                schema: "core",
                table: "contact_person",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true);
        }
    }
}
