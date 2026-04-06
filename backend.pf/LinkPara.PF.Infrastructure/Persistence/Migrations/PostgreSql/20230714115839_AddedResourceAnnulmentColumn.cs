using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedResourceAnnulmentColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "additional_info_annulment",
                schema: "merchant",
                table: "merchant",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "resource_annulment",
                schema: "merchant",
                table: "merchant",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "additional_info_annulment",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.DropColumn(
                name: "resource_annulment",
                schema: "merchant",
                table: "merchant");
        }
    }
}
